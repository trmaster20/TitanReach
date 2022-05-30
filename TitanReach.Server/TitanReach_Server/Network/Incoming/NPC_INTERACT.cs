using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class NPC_INTERACT : IncomingPacketHandler {

        private const int OPTION_COUNT = 4;

        public int GetID() {
            return Packets.NPC;
        }

        public async void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet) {
            try {
                int type = (int)packet.ReadByte();
                uint uid = packet.ReadUInt32();

                if (type == 12) {
                    p.SetBusy(false);
                    p.CurrentDialogOption = -1;
                    if (p.LastCallbackRef != null) {
                        p.LastCallbackRef.Stop();
                        Server.Instance.timers.Remove(p.LastCallbackRef);
                        p.LastCallbackRef = null;
                    }

                } else if (type == 11) {

                    Npc n = p.GetNpcByUID(uid);
                    if (n == null)
                        return;

                    if (p.ChatTokenSource != null) {
                        return;
                    }

                    p.ChatTokenSource = new CancellationTokenSource();
                    CancellationToken token = p.ChatTokenSource.Token;
                    Task conv = Task.Run(() => ProcessConversation(p, n, token), token);
                    try {
                        await conv;
                    } catch (OperationCanceledException) {
                        p.SetBusy(false);
                        p.CurrentDialogOption = -1;
                        n.LastInteractAction = Environment.TickCount;
                        n.SetNotBusy();
                    }

                    p.ChatTokenSource.Dispose();
                    p.ChatTokenSource = null;
                }
            } catch (Exception e) {
                Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                p.Disconnect("Error handling packets");
            }
        }

        private async Task ProcessConversation(Player p, Npc n, CancellationToken token) {

            List<NPCQuestRegister> questRegisters = null;
            List<NPCShopRegister> shopRegisters = null;
            NPCBankRegister bankRegister = null;
            Func<Npc, Player, int, CancellationToken, Task<int>> chat = null;

            int questCount = 0;
            int shopCount = 0;
            int chatCount = 0;
            int bank = 0;

            if (Server.Instance.ScriptManager.ACTION_QuestNpc.ContainsKey(n.ID)) {
                questRegisters = Server.Instance.ScriptManager.ACTION_QuestNpc[n.ID];

                questRegisters = Server.Instance.ScriptManager.ACTION_QuestNpc[n.ID].Where(qr => !p.QuestManager.Completed(qr.ID)).ToList();
                if (questRegisters != null) {
                    questCount = questRegisters.Count;
                }
            }

            if (Server.Instance.ScriptManager.ACTION_ShopNpc.ContainsKey(n.ID)) {
                shopRegisters = Server.Instance.ScriptManager.ACTION_ShopNpc[n.ID];
                shopCount = shopRegisters.Count;
            }

            if (Server.Instance.ScriptManager.ACTION_BankNpc.ContainsKey(n.ID)) {
                bankRegister = Server.Instance.ScriptManager.ACTION_BankNpc[n.ID];
                bank = 1;
            }

            if (Server.Instance.ScriptManager.ACTION_ChatNpc.ContainsKey(n.ID)) {
                chat = Server.Instance.ScriptManager.ACTION_ChatNpc[n.ID];
                chatCount++;
            }

            if (questCount + shopCount + chatCount + bank> 1) {

                p.DialogMessage(n, "Hi there!");
                List<string> allChoices = new List<string>();

                if (questCount > 0) {
                    QuestChoices(questRegisters, allChoices, p);
                }

                if (shopCount > 0) {
                    ShopChoices(shopRegisters, allChoices);
                }

                if (chatCount > 0) {
                    allChoices.Add("Chat");
                }

                if(bank == 1) {
                    allChoices.Add("Open Bank");
                }

                allChoices.Add("Exit");

                List<List<string>> choices = FormatChoices(allChoices);
                if (token.IsCancellationRequested) {
                    token.ThrowIfCancellationRequested();
                }
                Task pickTask = Task.Run(() => PickOption(questCount, questRegisters, shopCount, shopRegisters, chat, chatCount, bankRegister, bank, n, p, choices, 0, token), token);
                await pickTask;

            } else {
                if (questCount == 1) {
                    Task conversation = Task.Run(() => Conversation(questRegisters[0].func, n, p, token), token);
                    await conversation;
                } else if (shopCount == 1) {
                    shopRegisters[0].act(p);
                } else if (chatCount == 1) {
                    Task conversation = Task.Run(() => Conversation(chat, n, p, token), token);
                    await conversation;
                } else if (bank == 1) {
                    bankRegister.act(p);
                } else {
                    await Task.Run(() => p.Dialog(n, "Hi there.", token, true), token);
                    await Task.Run(() => p.Dialog(n, "Good day.", token, false), token);
                }
            }

            p.NetworkActions.EndDialog((uint)n.UID, "");
            p.SetBusy(false);
            p.CurrentDialogOption = -1;
            n.LastInteractAction = Environment.TickCount;
            n.SetNotBusy();
        }

        private async Task PickOption(int questCount, List<NPCQuestRegister> questRegisters, int shopCount,
            List<NPCShopRegister> shopRegisters, Func<Npc, Player, int, CancellationToken, Task<int>> chat, int chatCount, NPCBankRegister bankRegister, int bank, Npc n, Player p,
            List<List<string>> choices, int currentPage, CancellationToken token) {

            if (token.IsCancellationRequested) {
                token.ThrowIfCancellationRequested();
            }
            int dialogID = await Task.Run(() => p.DialogChoice(n, "What would you like to talk about?", token, choices[currentPage].ToArray()), token);

            if (dialogID == OPTION_COUNT - 1 && choices.Count > 1) {
                if (currentPage + 1 >= choices.Count) {
                    currentPage = 0;
                } else {
                    currentPage++;
                }

                await Task.Run(() => PickOption(questCount, questRegisters, shopCount, shopRegisters, chat, chatCount, bankRegister, bank, n, p, choices, currentPage, token), token);

            } else if ((dialogID + (3 * currentPage)) < questCount) {
                await Task.Run(() => Conversation(questRegisters[dialogID + (3 * currentPage)].func, n, p, token), token);

            } else if (dialogID + (3 * currentPage) - questCount < shopCount) {
                shopRegisters[dialogID + (3 * currentPage) - questCount].act(p);

            } else if (dialogID + (3 * currentPage) == questCount + shopCount && bank > 0) {
                bankRegister.act(p);

            } else if (dialogID + (3 * currentPage) == questCount + shopCount && chatCount > 0) {
                await Task.Run(() => Conversation(chat, n, p, token), token);
            }

        }

        private List<List<string>> FormatChoices(List<string> allChoices) {
            List<List<string>> choices = new List<List<string>> {
                new List<string>()
            };

            int currentArray = 0;

            if(allChoices.Count > 4) {
                for (int i = 0; i < allChoices.Count; i++) {
                    if (i != 0 && i % 3 == 0) {
                        choices[currentArray].Add("More Options");
                        currentArray++;
                        choices.Add(new List<string>());
                    }
                    choices[currentArray].Add(allChoices[i]);
                }
                choices[currentArray].Add("More Options");
            } else {
                choices[0].AddRange(allChoices);
            }

            return choices;
        }

        private async Task Conversation(Func<Npc, Player, int, CancellationToken, Task<int>> func, Npc n, Player p, CancellationToken token) {
            int convoState = 0;

            while (convoState >= 0) {
                if (token.IsCancellationRequested) {
                    token.ThrowIfCancellationRequested();
                }

                convoState = await Task.Run(() => func(n, p, convoState, token), token);
            }

            if (convoState == -2) {
                await Task.Run(() => p.Dialog(n, "Looks like you don't have space right now, come back later.", token, false), token);
                await Task.Run(() => p.Dialog(n, "Ok", token, true), token);
            }
        }

        private void QuestChoices(List<NPCQuestRegister> questRegisters, List<string> allChoices, Player p) {
            for (int i = 0; i < questRegisters.Count; i++) {
                allChoices.Add(p.QuestManager.GetQuestName(questRegisters[i].ID));
            }
        }

        private void ShopChoices (List<NPCShopRegister> shopRegisters, List<string> allChoices) {
            for (int i = 0; i < shopRegisters.Count; i++) {
                allChoices.Add("View Shop");
            }
        }
    }
}
