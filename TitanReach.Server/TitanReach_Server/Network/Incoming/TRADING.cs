using System;
using System.Collections.Generic;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared;

namespace TitanReach_Server.Network.Incoming {
    class TRADING : IncomingPacketHandler {

        public const int TRADE_TIMEOUT = 30000;

        public int GetID() {
            return Packets.TRADE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet) {
            var subtype = packet.ReadByte();

            if (subtype == 0) // invitation request/rec
            {
                Player target = Server.GetPlayerByUID(packet.ReadUInt32());
                if (target != null) {
                    if (p.Trading)
                        return;

                    if (target.Trading) {
                        p.Msg("Target is already in a trade");
                        return;
                    }

                    if (p.FriendsManager.IsIgnored(target)) {
                        p.Msg("You cannot trade with this player");
                        return;
                    }

                    if (!Formula.InRadius(p, target, 5)) {
                        p.NetworkActions.SendMessage(target.Name + " is too far away");
                        return;
                    }

                    if (p.TradeInitiator && p.TradingPlayer != null && target == p.TradingPlayer && p.LastTradeRequestedTime <= TRADE_TIMEOUT)
                        return;

                    if (p.LastTradeRequestedTime != -1 && Environment.TickCount - p.LastTradeRequestedTime >= TRADE_TIMEOUT)
                        p.LastTradeRequested = null;


                    if (p.LastTradeRequested != null && p.LastTradeRequested == target && !p.TradeInitiator) // start trade
                    {

                        p.NetworkActions.StartTrade(target);
                        p.LastTradeRequested = null;
                        p.TradingPlayer = target;
                        p.Trading = true;

                        target.NetworkActions.StartTrade(p);
                        target.LastTradeRequested = null;
                        target.Trading = true;
                        target.TradingPlayer = p;

                    } else {
                        if (target == p) {
                            p.Msg("You cannot trade with yourself");
                            return;
                        }
                        p.TradeInitiator = true;
                        p.Msg("Sent trade request to " + target.Name);
                        target.Msg(p.Name + " wishes to trade");
                        target.NetworkActions.TradeRequest(p);
                        target.LastTradeRequested = p;

                        target.LastTradeRequestedTime = Environment.TickCount;
                        p.LastTradeRequested = target;
                        p.LastTradeRequestedTime = Environment.TickCount;
                    }

                }

            } else if (subtype == 2) // add item
              {
                if (!TradeOngoing(p))
                    return;

                if (p.TradingItems.Count >= 30)
                    return;

                if (p.TradingPlayer == null)
                    return;

                p.TradingItems.Clear();

                int count = (int)packet.ReadByte();
                for (int i = 0; i < count; i++) {
                    ushort id = packet.ReadUInt16();
                    int amount = (int)packet.ReadUInt32();

                   

                    if (id < 0 || id + 1 > DataManager.ItemDefinitions.Length || amount < 0)
                        continue;

                    

                        if (!p.Inventory.HasItem(id, (int)amount))
                        continue;

                    p.TradingItems.Add(new Item(id, amount));
                }

                TradeUnaccept(p);
                p.TradingPlayer.NetworkActions.TradeUpdateTheirItems(p.TradingItems);
            } else if (subtype == 3) // update/remove item
              {
                if (!TradeOngoing(p))
                    return;

                int id = (int)packet.ReadUInt16();
                int amount = (int)packet.ReadUInt32();

                for (int i = 0; i < p.TradingItems.Count; i++) {
                    Item ite = p.TradingItems[i];
                    if (ite != null) {
                        if (ite.ID == id) {
                            if (amount == 0) {
                                p.TradingPlayer.NetworkActions.TradeUpdateItem(ite.ID, 0);
                                p.TradingItems.Remove(ite);
                            } else {
                                ite.Amount = amount;
                            }
                        }
                    }
                }

                TradeUnaccept(p);
                p.TradingPlayer.NetworkActions.TradeUpdateTheirItems(p.TradingItems);

            } else if (subtype == 4) // accept
              {
                if (!TradeOngoing(p))
                    return;
                p.TradingAccepted = true;
                if (p.TradingPlayer.TradingAccepted) {
                    // do trade!
                    try {
                        List<Item> OtherItems = p.TradingPlayer.TradingItems;
                        List<Item> OurItems = p.TradingItems;

                        InvItem[] OurInventoryTemp = (InvItem[])p.Inventory.items.Clone();
                        InvItem[] TheirInventoryTemp = (InvItem[])p.TradingPlayer.Inventory.items.Clone();

                        /** Remove our traded items from our temporary inventory **/
                        foreach (Item tradeItem in OurItems) {
                            if (!Inventory.HasItemV(tradeItem.ID, tradeItem.Amount, OurInventoryTemp)) {
                                Error("1 an item cannot be found", p);
                                return;
                            }
                            Inventory.RemoveItemV(tradeItem.ID, tradeItem.Amount, OurInventoryTemp);
                        }

                        /** Remove their traded items from their temporary inventory **/
                        foreach (Item tradeItem in OtherItems) {
                            if (!Inventory.HasItemV(tradeItem.ID, tradeItem.Amount, TheirInventoryTemp)) {
                                Error("3  an item cannot be found", p);
                                return;
                            }
                            Inventory.RemoveItemV(tradeItem.ID, tradeItem.Amount, TheirInventoryTemp);
                        }
                        /** Add their traded items to our temporary inventory **/
                       
                            foreach (Item tradeItem in OtherItems)
                            {
                                if (!(p.Rank == TRShared.Data.Enums.Rank.ADMIN || p.TradingPlayer.Rank == TRShared.Data.Enums.Rank.ADMIN)) // allow an admin to trade or accept an untradeble item
                                {
                                    if (!tradeItem.Definition.Tradable)
                                    {
                                        Error("An untradeble item was involved, trade cancelled", p);
                                        return;
                                    }
                                }
                                if (!Inventory.AddItemV(tradeItem, OurInventoryTemp))
                                {
                                    Error("One of the traders has a full inventory", p);
                                    return;
                                }
                            }

                            foreach (Item tradeItem in OurItems)
                            {
                                if (!(p.Rank == TRShared.Data.Enums.Rank.ADMIN || p.TradingPlayer.Rank == TRShared.Data.Enums.Rank.ADMIN)) // allow an admin to trade or accept an untradeble item
                                {
                                    if (!tradeItem.Definition.Tradable)
                                    {
                                        Error("An untradeble item was involved, trade cancelled", p);
                                        return;
                                    }
                                }
                                if (!Inventory.AddItemV(tradeItem, TheirInventoryTemp))
                                {
                                    Error("One of the traders has a full inventory", p);
                                    return;
                                }
                            }
                   

                        /** Add their traded items to our temporary inventory **/
                        p.Inventory.items = OurInventoryTemp;
                        p.TradingPlayer.Inventory.items = TheirInventoryTemp;

                        p.NetworkActions.SendInventory();
                        p.TradingPlayer.NetworkActions.SendInventory();

                        p.TradingPlayer.Msg("Trade complete");
                        p.Msg("Trade complete");
                        ResetTrade(p);
                    } catch (Exception e) {
                        Server.Error(e.Message + "\n" + e.StackTrace.ToString());
                    }
                } else {
                    p.TradingPlayer.NetworkActions.SendOtherPlayerTradeAccept();
                }
            } else if (subtype == 5) // decline
              {
                if (p.TradingPlayer != null && p.TradingPlayer.TradingPlayer == p) {
                    p.TradingPlayer.Msg(p.Name + " has declined the trade");
                    ResetTrade(p);
                }
            } else if (subtype == 7) //unaccept
              {
                TradeUnaccept(p);
            }
        }

        private void TradeUnaccept(Player p) {
            if (!TradeOngoing(p)) {
                Server.Log("stopped invalid unaccept");
                return;
            }

            p.TradingAccepted = false;
            p.TradingPlayer.TradingAccepted = false;
            p.NetworkActions.SendTradeUnAccept();
            p.TradingPlayer.NetworkActions.SendTradeUnAccept();
        }

        public void Error(string s, Player p) {
            if (p.TradingPlayer != null)
                p.TradingPlayer.Error(s);

            p.Error(s);
            ResetTrade(p);
        }

        public bool TradeOngoing(Player p) {
            if (p.Trading && p.TradingPlayer != null)
                if (p.TradingPlayer.Trading && p.TradingPlayer.TradingPlayer == p)
                    return true;

            return false;
        }

        public static void ResetTrade(Player p) {
            if (p.TradingPlayer != null) {
                p.TradingPlayer.Trading = false;
                p.TradingPlayer.TradingAccepted = false;
                p.TradingPlayer.TradingItems.Clear();
                p.TradingPlayer.LastTradeRequested = null;
                p.TradingPlayer.NetworkActions.EndTrade();
                p.TradingPlayer.TradeInitiator = false;
                p.TradingPlayer = null;
            }
            p.Trading = false;
            p.TradingAccepted = false;
            p.TradingItems.Clear();
            p.LastTradeRequested = null;
            p.NetworkActions.EndTrade();
            p.TradeInitiator = false;
        }
    }
}
