using System;
using TitanReach_Server.Model;
using TitanReach_Server.Skills;
using TitanReach_Server.Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using static TitanReach_Server.Database;
using TRShared.Data.Enums;
using TRShared.Data.Definitions;
using TRShared;

namespace TitanReach_Server {
    class ServerCommands {

        private PartyManager pm;
        private Server server;

        public ServerCommands(PartyManager pm, Server server) {
            this.pm = pm;
            this.server = server;
        }

        [Command(Rank.USER, "gives a list of available commands", new string[] { }, "help")]
        public void Help(Player p, string[] args) {
            MethodInfo[] methods = typeof(ServerCommands).GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Command>().Count() > 0).ToArray();
            if (methods.Length > 0) {
                MethodInfo[] ms = methods.Where(m => m.GetCustomAttributes(false).OfType<Command>().First().RequiredRank <= p.Rank).ToArray();
                foreach(MethodInfo m in ms) {
                    p.Msg(m.GetCustomAttribute<Command>().HelpText());
                }
            }
        }

        [Command(Rank.ADMIN, "opens your bank", new string[] {}, "bank", "vault")]
        public void OpenBank(Player p, string [] args) {
            p.NetworkActions.SendBank();
           
        }
        
        
        [Command(Rank.ADMIN, "hurts you for testing purposes", new string[] {}, "hurt")]
        public void DamagePlayer(Player p, string [] args)
        {
            p.Damage(p.GetMaxHealth() / 4, null, DamageType.MAGIC);
        }

        [Command(Rank.ADMIN, "hurts you for testing purposes", new string[] { }, "hurtloop")]
        public void DamagePlayerr(Player p, string[] args)
        {
            for(int i=0; i < 9; i++)
            {
                p.Damage(i + 1, null, DamageType.MAGIC);
            }
           
        }

        [Command(Rank.ADMIN, "hurts npcs  for testing purposes", new string[] { }, "hurtnpc")]
        public void DamageNpc(Player p, string[] args)
        {
            foreach (Npc n in p.Map.Npcs)
            {
                // n.NpcSpawnDefinition.RespawnTime = int.MaxValue;
                n.Damage(25, p, DamageType.MAGIC);
            }
        }



                           


    [Command(Rank.ADMIN, "changes map instance", new string[] {"map_id" }, "map")]
        public void Map(Player p, string[] args)
        {
            Server.Log("Requesting map change");
            if (!int.TryParse(args[0], out int mapID))
            {
                p.Msg("Invalid argument: /map <id>");
            }
            Vector3 vec = null;
            switch(mapID)
            {
                case 0:
                    vec = Locations.PLAYER_SPAWN_POINT;
                    break;

                case 1:
                    vec = new Vector3(134, -80f, 284);
                    break;

                case 2:
                    vec = new Vector3(58.6f, -32f, 6.4f);
                    break;

                case (int)Lands.DEV_WORLD:
                    vec = Locations.DEV_WORLD_SPAWN;
                    break;

                case (int)Lands.OASIS:
                    vec = Locations.OASIS_SPAWN;
                    break;

                case (int)Lands.DROMHEIM:
                    vec = Locations.DROMHEIM_SPAWN;
                    break;

                default:
                    p.Error("INvalid map you goose");
                    return;
            }
            p.ChangeMap(mapID, vec);
        }

        [Command(Rank.MOD, "shuts down the server", new string [] {"time"}, "shutdown")]
        public void ShutdownServer(Player p, string[] args) {
            if (!int.TryParse(args[0], out int shutdownTicks)) {
                p.Msg("Invalid argument: /shutdown <time>");
            }

            Discord.MessageDB("Server shutdown in " + (shutdownTicks * 10) + " seconds. Requested by " + p.Name);
            server.MessageAllPlayers("<color=yellow>===== SERVER IS SHUTTING DOWN IN " + shutdownTicks * 10 +
                        " SECONDS. LOG OUT! =====</color>");
            server.LoopedDelay(10000, (timer, arg) => {
                shutdownTicks--;
                server.MessageAllPlayers("<color=yellow>===== SERVER IS SHUTTING DOWN IN " + shutdownTicks * 10 +
                            " SECONDS. LOG OUT! =====</color>");
                if (shutdownTicks <= 0) {
                    server.Shutdown();
                }
            });
        }

        [Command(Rank.ADMIN, "restarts the server", new string[] { "time" }, "restart")]
        public void RestartServer(Player p, string[] args) {
            if (!int.TryParse(args[0], out int shutdownTicks)) {
                p.Msg("Invalid argument: /restart <time>");
            }

            Discord.MessageDB("Server restarting in " + (shutdownTicks * 10) + " seconds. Requested by " + p.Name);
            server.MessageAllPlayers("<color=yellow>===== SERVER IS RESTARTING IN " + shutdownTicks * 10 +
                        " SECONDS. LOG OUT! =====</color>");
            server.LoopedDelay(10000, (timer, arg) => {
                shutdownTicks--;
                server.MessageAllPlayers("<color=yellow>===== SERVER IS RESTARTING IN " + shutdownTicks * 10 +
                            " SECONDS. LOG OUT! =====</color>");
                if (shutdownTicks <= 0) {
                    server.Restart();
                }
            });
        }

        [Command(Rank.ADMIN, "unlocks all available pets", new string[] { }, "unlockpets")]
        public void UnlockAllPets(Player p, string[] args) {
            p.PetManager.UnlockAllPets();
        }

        [Command(Rank.ADMIN, "clear pets", new string[] { }, "clearpets")]
        public void ClearAllPets(Player p, string[] args)
        {
            p.PetManager.ClearPets();
        }

        [Command(Rank.ADMIN, "clear bank", new string[] { }, "clearbank")]
        public void ClearBank(Player p, string[] args)
        {
            p.Vault.items = new Item[short.MaxValue];
            p.NetworkActions.SyncVault();
            p.Msg("Bank cleared");
        }

        [Command(Rank.MOD, "unlocks cow1", new string[] { }, "fishisthebestdevontheteam")]
        public void UnlockCow1(Player p, string[] args)
        {
            p.Msg("Baited");
            p.Msg("try /pogpogpogpogpogpogpogpogpogpogpogpogpogpogpogpogpog");
        }

        [Command(Rank.MOD, "unlocks cow2", new string[] { }, "pogpogpogpogpogpogpogpogpogpogpogpogpogpogpogpogpog")]
        public void UnlockCow2(Player p, string[] args)
        {
            p.Msg("Baited");
            p.Msg("try /asdmkosdinflqwmepotrnsklamsdlkdfjglknamanfdgafnioeewoi");

        }

        [Command(Rank.MOD, "unlocks cow3", new string[] { }, "asdmkosdinflqwmepotrnsklamsdlkdfjglknamanfdgafnioeewoi")]
        public void UnlockCow3(Player p, string[] args)
        {
            p.Msg("Baited");
            p.Msg("try /unlock-cow");
        }

        [Command(Rank.MOD, "clears waiting pool", new string[] { }, "clearpool")]
        public void ClearPool(Player p, string[] args)
        {
            
            p.Msg("Cleared " + Server.Instance.WaitingPool.Count + " Players stuck");

            Server.Instance.WaitingPool.Clear();
        }

        [Command(Rank.MOD, "waiting pool", new string[] { }, "pool")]
        public void Pool(Player p, string[] args)
        {

            p.Msg(Server.Instance.WaitingPool.Count + " Players in waiting pool");

        }
        [Command(Rank.MOD, "unlocks cow4", new string[] { }, "unlock-cow")]
        public void UnlockCow4(Player p, string[] args)
        {
            p.Msg("gz");
            p.PetManager.Unlock1Pet(17);
        }
        

        [Command(Rank.USER, "gets your killcount on a given npc", new string[] { "npc_name / all"}, "kc", "killcount")]
        public void GetKillCount(Player p, string[] args) {
            string npcName = args[0].Replace("_", " ");

            NpcDef npcDef;
            bool found = false;

            if (npcName.Equals("all")) {
                int total = 0;
                for (int i = 0; i < DataManager.NpcDefinitions.Length; i++) {
                    npcDef = DataManager.NpcDefinitions[i];
                    if (npcDef != null && p.Bestiary.ContainsKey(npcDef.NpcID)) {
                        int kills = p.Bestiary[npcDef.NpcID].GetKills();
                        p.Msg("You have killed " + kills + " " + npcDef.Name);
                        total += kills;
                    }
                }
                p.Msg("You have " + total + " total kills");
                return;
            }

            for(int i = 0; i < DataManager.NpcDefinitions.Length; i++) {
                npcDef = DataManager.NpcDefinitions[i];
                if(npcDef != null && npcDef.Name.ToLower().Equals(npcName.ToLower())) {
                    found = true;

                    if (!p.Bestiary.ContainsKey(npcDef.NpcID)) {
                        p.Msg("You have never killed this creature");
                    } else {
                        p.Msg("You have killed " + p.Bestiary[npcDef.NpcID].GetKills() + " " + npcDef.Name);
                    }
                    break;
                }
            }

            if (!found) {
                p.Msg("Invalid argument: /kc <npc_name / all>");
                return;
            }
        }

        [Command(Rank.ADMIN, "unlocks all available titles", new string[] { }, "unlocktitles")]
        public void UnlockAllTitles(Player p, string[] args) {
            p.TitleManager.UnlockAllTitles();
        }

        [Command(Rank.ADMIN, "make ya fresh and clean", new string[] { }, "makeover")]
        public void Makeover(Player p, string[] args)
        {
            p.NetworkActions.SendAppearanceRequest();
        }

        struct Purchase
        {

            public Purchase(int shopID, int rewardID, string titleName, PurchaseType type, int[] items = null)
            {
                this.ShopID = shopID;
                this.RewardID = rewardID;
                this.TitleName = titleName;
                this.Type = type;
                this.MultiItems = items;
            }

            public int[] MultiItems;
            public int ShopID;
            public int RewardID;
            public string TitleName;
            public PurchaseType Type;
        }
        public enum PurchaseType { PET, ITEM, TITLE, SERVICE, UNIMPLEMENTED }

        static Purchase[] Rewards =
        {
            new Purchase(1, -1, "Lifetime Membership", PurchaseType.UNIMPLEMENTED),
            new Purchase(2, -1, "Early Access", PurchaseType.UNIMPLEMENTED),
            new Purchase(3, -1, "Closed Alpha", PurchaseType.UNIMPLEMENTED),
            new Purchase(4, -1, "One Month Subscription", PurchaseType.UNIMPLEMENTED),
            new Purchase(5, 23, "Storm Cat", PurchaseType.PET),
            new Purchase(6, 25, "Blood Hound", PurchaseType.PET),
            new Purchase(7, 22, "Mystic Squirrel", PurchaseType.PET),
            new Purchase(8, 24, "Lava Fox", PurchaseType.PET),
            new Purchase(9, -1, "Valkyrie Angel", PurchaseType.UNIMPLEMENTED),
            new Purchase(10, 2, "Adventurer", PurchaseType.TITLE),
            new Purchase(11, 4, "Glyphbinder", PurchaseType.TITLE),
            new Purchase(12, 3, "Dawnbringer", PurchaseType.TITLE),
            new Purchase(13, 5, "Titan Master", PurchaseType.TITLE),
            new Purchase(14, 583, "Founders Cape", PurchaseType.ITEM),
            new Purchase(15, -1, "Founders Outfit", PurchaseType.ITEM, new int[]{590, 591, 592, 593, 594 }),
            new Purchase(16, -1, "Teleport FX", PurchaseType.SERVICE),
            new Purchase(17, 589, "Golden Cape", PurchaseType.ITEM),
            new Purchase(18, -1, "Name an Npc", PurchaseType.UNIMPLEMENTED),
            new Purchase(19, -1, "Design A Weapon", PurchaseType.UNIMPLEMENTED),
            new Purchase(20, -1, "Design A Pet", PurchaseType.UNIMPLEMENTED),
            new Purchase(21, -1, "Design AN Armour Set", PurchaseType.UNIMPLEMENTED),
            new Purchase(22, -1, "Digital Soundtrack", PurchaseType.UNIMPLEMENTED),
            new Purchase(23, -1, "Name Reservation", PurchaseType.SERVICE),
            new Purchase(24, 20, "Founders", PurchaseType.TITLE),
            new Purchase(25, 588, "Early Access Cape", PurchaseType.ITEM)
        };

        public bool HasItemOnCharacter(Player p, int id)
        {
            bool found = false;
            if (p.Inventory.HasItem((ushort)id, 1))
                return true;
            if (p.Vault.HasItem(id, 1))
                return true;
            foreach(Item i in p.Equipment.EquippedItems)
            {
                if (i != null && i.ID == id)
                    return true;
            }
            return false;
        }

        public async void ProcessReward(Player p, int index)
        {
            if (index - 1 >= Rewards.Length)
                return;
            var reward = Rewards[index - 1];
            switch (reward.Type)
            {
                case PurchaseType.ITEM:

                    if(reward.RewardID != -1 && !HasItemOnCharacter(p, reward.RewardID))
                    {
                        p.Vault.AddItem(new Item((ushort)reward.RewardID, 1));
                        p.NetworkActions.SyncVault();
                        p.NetworkActions.SendMessage(reward.TitleName + " has been added to your bank!");
                        return;
                    } else if(reward.RewardID == -1 && reward.MultiItems != null)
                    {
                        bool found = false;
                        foreach(int itm in reward.MultiItems)
                        {
                            if(!HasItemOnCharacter(p, itm))
                            {
                                found = true;
                                p.Vault.AddItem(new Item((ushort)itm, 1));
                            }
                        }
                        if (found)
                        {
                            p.NetworkActions.SendMessage(reward.TitleName + " has been added to your bank!");
                            p.NetworkActions.SyncVault();
                        }
                    }

                    break;

                case PurchaseType.PET:
                    if(reward.RewardID != -1 && !p.PetManager.UnlockedPets.Contains(reward.RewardID))
                    {
                        await p.PetManager.UnlockPet(reward.RewardID);
                        p.NetworkActions.SendMessage(reward.TitleName + " pet has been unlocked");
                    }

                    break;

                case PurchaseType.TITLE:
                    if (reward.RewardID != -1 && !p.TitleManager.UnlockedTitles.Contains(Server.Instance.AllTitles[reward.RewardID]))
                    {
                        p.TitleManager.UnlockTitle(Server.Instance.AllTitles[reward.RewardID]);
                        p.NetworkActions.SendMessage(reward.TitleName + " title has been unlocked");
                    }

                    break;
            }
        }
   


        [Command(Rank.USER, "claim purchases", new string[] { }, "claim")]
        public async void ClaimPurchases(Player p, string[] args)
        {
            var res = await Server.Instance.DB.GetPurchases(p);
            if (p == null)
                return;
            var val = (List<ShopResult>)res.value;
            if (val == null)
                Server.ErrorDB(p.Describe() + " used command /claim with a failed result: " + res.ErrorObject.errorCode + " - " + res.ErrorObject.error);
            else
            {

                foreach (var v in val)
                {
                   // p.Msg(v.itemName + " x " + v.purchaseQuantity);
                    ProcessReward(p, v.itemId);
                }

            }
        }

    [Command(Rank.USER, "Gets your current location", new string[] { }, "loc", "location")]
        public void PrintLocation(Player p, string[] args) {
            int x = (int)p.transform.position.X;
            int z = (int)p.transform.position.Z;
            p.Msg("Current Location: X=" + x + " Y=" + (int)p.transform.position.Y + " Z=" + z);
            p.Msg("Heightmap for : X=" + x + "Y=" + p.Map.Land.HEIGHT_MAP[(p.Map.Land.HEIGHT_MAP_RESOLUTION / 2) + x][(p.Map.Land.HEIGHT_MAP_RESOLUTION / 2) + z] + " Z=" + z);
        }

        [Command(Rank.USER, "manually saves your character", new string[] { }, "save")]
        public void Save(Player p, string[] args) {
            Server.Instance.DB.SaveCharacter(p);
        }

        [Command(Rank.USER, "gets your player id", new string[] { }, "pid")]
        public void GetPlayerID(Player p, string[] args) {
            p.Msg("Player ID: " + p.UID);
        }

        [Command(Rank.MOD, "forces an account to log out of the sever", new string[] { "player_name" }, "unban")]
        public async void UnbanPlayer(Player p, string[] args)
        {
            string name = args[0].Replace("_", " ");
            var res = await Server.Instance.DB.BanPlayer(name, true);
            var val = (Database.BanResult)res.value;
            if(val == null)
                Server.ErrorDB(p.Describe() + " used command /unban with a failed result: " + res.ErrorObject.errorCode + " - " + res.ErrorObject.error);
            else {
                Server.Important(p.Describe() + " has unbanned " + args[0]);
                p.Msg(val.message);
            }

        }

        [Command(Rank.MOD, "bans a player", new string[] {"player_name"}, "ban")]
        public async void Ban(Player p, string[] args) {

            Player pl = null;

            string target = args[0].Replace("_", " ").ToLower();
            lock(Server.Instance.AllPlayers)
            {
                foreach (Player player in Server.Instance.AllPlayers)
                    if (player != null && player.Name.ToLower() == target)
                    {
                        pl = player;
                        break;
                    }
            }
            if (pl != null)
            {
                if (pl.Rank == Rank.ADMIN)
                {
                    p.Error("You can't ban an admin");
                    return;
                }
                else if (pl.Rank == Rank.MOD && p.Rank != Rank.ADMIN)
                {
                    p.Error("You can't ban another mod");
                    return;
                }
            }

            var res = await Server.Instance.DB.BanPlayer(target, false);
            var val = (Database.BanResult)res.value;
            if (val == null)
                Server.ErrorDB(p.Describe() + " used command /ban with a failed result: " + res.ErrorObject.errorCode + " - " + res.ErrorObject.error);
            else
            {
                Server.Important(p.Describe() + " has banned " + target);
                p.Msg(val.message);
                server.Delay(500, (timer, arg) => {
                    pl?.Disconnect();
                });
            }
        }

        [Command(Rank.MOD, "gets your player rank", new string[] {}, "rank")]
        public void RankCheck(Player p, string[] args)
        {
            p.Msg("Your rank is: " + p.Rank + " " + p.Rank.ToString());
        }

        [Command(Rank.MOD, "gets a list of online players", new string[] { }, "online")]
        public void OnlinePlayers(Player p, string[] args) {
            var s = "";
            lock (Server.Instance.AllPlayers) {
                foreach (Player pl in Server.Instance.AllPlayers) s += "," + pl.Name;
            }

            p.Msg("Online(" + Server.Instance.AllPlayers.Count + ") ");
        }

        [Command(Rank.MOD, "Gets the id of specific player", new string[] { "player_name"}, "fetchID")]
        public void FetchPlayerID(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            p.Msg("Player " + p.Name + "ID is : " + p.UID);
        }

        [Command(Rank.MOD, "Summon a player to your location", new string[] {"player_name"}, "summon", "fetch")]
        public void SummonPlayer(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            pl.TeleportTo(p.Map.LandID, p.transform.position.X, p.transform.position.Y, p.transform.position.Z);
        }


        [Command(Rank.MOD, "kicks a player from the server", new string[] { "player_name" }, "kick")]
        public void Kick(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            pl.Disconnect();
            p.Msg(pl.Name + " has been Kicked!");
        }

        [Command(Rank.MOD, "mute a player", new string[] { "player_name" }, "mute")]
        public async void Mute(Player p, string[] args) {
            string name = args[0].Replace("_", " ");

            Response res = await Server.Instance.DB.MutePlayer(name, true);
            if (res.Error) {
                p.Msg(res.ErrorObject.error);
                return;
            }
            p.Msg(name + " has been muted!");

            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                return;
            }

            pl.Mute();
            p.Msg(pl.Name + " has been muted!");
        }

        [Command(Rank.MOD, "unmute a player", new string[] { "player_name" }, "unmute")]
        public async void UnMute(Player p, string[] args) {
            string name = args[0].Replace("_", " ");

            Response res = await Server.Instance.DB.MutePlayer(name, false);
            if (res.Error) {
                p.Msg(res.ErrorObject.error);
                return;
            }
            p.Msg(name + " has been unmuted!");

            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                return;
            }

            pl.UnMute();
        }

        [Command(Rank.MOD, "teleport to a player", new string[] { "player_name" }, "goto", "tp")]
        public void TeleportToPlayer(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            p.TeleportTo(pl.Map.LandID, pl.transform.position.X, pl.transform.position.Y, pl.transform.position.Z);
        }

        [Command(Rank.ADMIN, "uses an emote", new string[] {"emote name"}, "emote")]
        public void Emote(Player p, string[] args) {
            Enum.TryParse<EmoteTypes>(args[0].ToUpper(), out EmoteTypes emote);
            if (emote == 0) {
                p.Error("Invalid emote name");
                return;
            }

            p.NetworkActions.SendEmote((int)emote);
        }

        [Command(Rank.ADMIN, "gets your quest states", new string[] {}, "queststates")]
        public void GetQuestStates(Player p, string[] args) {
            for (int i = 0; i < p.QuestManager.questStates.Length; i++) {
                if (p.QuestManager.questStates[i].Number != 0) {
                    p.Msg("Quest " + i + " State: " + p.QuestManager.questStates[i].Number);
                }
            }
        }

        [Command(Rank.ADMIN, "gets a list of all titles", new string[] { }, "alltitles")]
        public void GetAllTitles(Player p, string[] args) {
            foreach (Title title in server.AllTitles.Values) {
                p.Msg(title.ID + ": " + title.Display);
            }
        }

        [Command(Rank.ADMIN, "unlocks a title for a player", new string[] {"player_name", "title ID"}, "unlocktitle")]
        public void UnlockTitle(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            if (!int.TryParse(args[1], out int titleID)) {
                p.Msg("Invalid argument: /unlocktitle <player_name> <titleID>");
            }

            if (!server.AllTitles.ContainsKey(titleID)) {
                p.Error("Invalid title ID");
                return;
            }

            Title title = server.AllTitles[titleID];
            pl.TitleManager.UnlockTitle(title);
        }

        [Command(Rank.USER, "invites a player to your party", new string[] { "player_name"}, "invite")]
        public void PartyInvite(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player toInvite = Server.GetPlayerByName(name);
            if (toInvite != null) {
                pm.InvitePlayer(p, toInvite);
            } else {
                p.Msg("Could not find that player to invite");
            }
        }

        [Command(Rank.ADMIN, "sets all levels to max and fills bank", new string[] {}, "f")]
        public void QuickCheats(Player p, string[] args) {
            Arcana.CastSpell(p, 1, true);

            SetLevel(p, new string[] { "all", "100" });

            p.CurrentHealth = p.GetMaxHealth();
            p.NetworkActions.SyncExp(p, (int)Stats.SKILLS.Vitality);

            FillBank(p, args);
        }

        [Command(Rank.ADMIN, "fills your bank", new string[] { }, "g")]
        public void FillBank(Player p, string[] args) {
            foreach (var def in DataManager.ItemDefinitions) {
                if (def == null) continue;
                if (def.Legacy) continue;
                if (def.IsToken) continue;

                p.Vault.AddItem(def.ItemID, def.MaxStackSize > 1 ? 50000 : 5000);
            }
            p.Msg("All items now in vault theoretically");

            p.NetworkActions.SyncVault();
        }

        [Command(Rank.ADMIN, "randomises your appearance", new string[] { }, "r")]
        public void RandomiseAppearance(Player p, string[] args) {
            p.RandomizeCustomOptions();
        }

        [Command(Rank.USER, "teleports your character to spawn", new string[] { }, "stuck")]
        public void Stuck(Player p, string[] args) {
            TimeSpan cd = PlayerTracking.StuckCooldown(p.UID);
            if (cd.TotalSeconds > 0 && p.Rank < Rank.MOD) {
                p.Msg("You need to wait another " + cd.Minutes + ":" + cd.Seconds + " minutes before you can use /stuck again");
                return;
            }
            if(p.MapID != 0)
            {
                p.Error("You cannot use this command on this map");
                return;
            }
            if (p.Dead)
                return;

            p.TeleportTo(0, Locations.PLAYER_SPAWN_POINT.X, Locations.PLAYER_SPAWN_POINT.Y, Locations.PLAYER_SPAWN_POINT.Z);
           /* p.transform.position = Locations.PLAYER_SPAWN_POINT;
            p.NetworkActions.SendLocation();
            p.Viewport.FullUpdate();
            p.SetPVP(false);
            p.PlayerTracking.LastTP = DateTime.Now.Ticks;
           */
            PlayerTracking.UpdateStuckTime(p.UID);
        }

        [Command(Rank.ADMIN, "applies a test buff to your character", new string[] { }, "buff")]
        public void ApplyBuff(Player p, string[] args) {
            p.ApplyBuff(Buff.Freeze(3, 8, 8000));
        }

        [Command(Rank.ADMIN, "teleports all players on your map to your location", new string[] { }, "teleall")]
        public void TeleAll(Player p, string[] args) {
            lock (p.Map.Players) {
                foreach (var pl in p.Map.Players) {
                    pl.transform.position = p.transform.position;
                    pl.NetworkActions.SendLocation();
                    pl.PlayerTracking.LastTP = DateTime.Now.Ticks;
                    pl.Viewport.FullUpdate();
                }
            }
        }

        [Command(Rank.ADMIN, "sets a skill to the specified level", new string[] {"skill", "level"}, "set")]
        public void SetLevel(Player p, string[] args) {
            if (!int.TryParse(args[1], out int level)) {
                p.Msg("Invalid argument: /set <skill> <level>");
            }
            string skillString = args[0];

            if (level <= 0 || level > 100) {
                p.Error("Invalid level");
                return;
            }

            if (skillString.Equals("all")) {
                foreach (int skillInt in Enum.GetValues(typeof(Stats.SKILLS))) {
                    p.Skills.SetLevelAndExp(skillInt, level);
                    p.NetworkActions.SyncExp(p, skillInt);
                }
            } else {
                if (!Enum.TryParse(skillString, out Stats.SKILLS skill)) {
                    p.Error("Invalid skill");
                    return;
                }

                p.Skills.SetLevelAndExp((int)skill, level);
                p.Msg(skillString + " Level set to " + level);
                p.NetworkActions.SyncExp(p, (int)skill);
            }
        }

        [Command(Rank.ADMIN, "sets the level of a skill on another player", new string[] { "player_name", "skill", "level" }, "setother")]
        public void SetOtherLevel(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            if (!int.TryParse(args[2], out int level)) {
                p.Msg("Invalid argument: /setother <player_name> <skill> <level>");
            }

            string skillString = args[1];

            if (level <= 0 || level > 100) {
                p.Error("Invalid level");
                return;
            }

            if (skillString.Equals("all")) {
                foreach (int skillInt in Enum.GetValues(typeof(Stats.SKILLS))) {
                    pl.Skills.SetLevelAndExp(skillInt, level);
                    pl.NetworkActions.SyncExp(p, skillInt);
                }
            } else {
                if (!Enum.TryParse(skillString, out Stats.SKILLS skill)) {
                    p.Error("Invalid skill");
                    return;
                }

                pl.Skills.SetLevelAndExp((int)skill, level);
                p.Msg(pl.Name + " " + skillString + " Level set to " + level);
                pl.NetworkActions.SyncExp(p, (int)skill);
            }
        }

        [Command(Rank.ADMIN, "opens the admin pannel", new string[] { }, "admin")]
        public void AdminMode(Player p, string[] args) {
            p.ToggleAdminMode();
        }

        [Command(Rank.ADMIN, "teleports you to one of your set homes", new string[] {"home num"}, "home")]
        public void TeleHome(Player p, string[] args) {
            if (int.TryParse(args[0], out int num)) {
                num--;
                if (num >= 0 && num < p.homes.Length && p.homes[num] != null) {
                    p.transform.position = p.homes[num];
                    p.PlayerTracking.LastTP = DateTime.Now.Ticks;
                    p.NetworkActions.SendLocation();
                } else {
                    p.Msg("Error! /sethome <home_num max=" + p.homes.Length + ">");
                }
            } else {
                p.Msg("Error! /sethome <home_num>");
            }
        }

        [Command(Rank.ADMIN, "sets a home at your current location", new string[] { "home num" }, "sethome")]
        public void SetHome(Player p, string[] args) {
            if (int.TryParse(args[0], out int num)) {
                num--;
                if (num >= 0 && num < p.homes.Length) {
                    p.homes[num] = p.transform.position;
                    p.homes[num].Y++;
                    p.Msg("home " + (num + 1) + " set to: " + p.homes[num].ToString());
                } else {
                    p.Msg("Error! /sethome <home_num max=" + p.homes.Length + ">");
                }
            } else {
                p.Msg("Error! /sethome <home_num>");
            }
        }

        [Command(Rank.ADMIN, "toggles pvp for the current map", new string[] { }, "pvp")]
        public void ToggleGlobalPvp(Player p, string[] args) {
            p.Map.PvpEnabled = !p.Map.PvpEnabled;
            lock (p.Map.Players) {
                foreach (var pl in p.Map.Players) {
                    if (p.Map.PvpEnabled) pl.Msg("<color=green>Pvp Enabled</color>");
                    if (!p.Map.PvpEnabled) pl.Msg("<color=green>Pvp Disabled</color>");
                }
            }
        }

        [Command(Rank.ADMIN, "uses a test spell", new string[] {"spell ID"}, "spell")]
        public void CastSpell(Player p, string[] args) {
            if (true)
            {
                p.Error("No spells avail");
                return;
            }
            if (!int.TryParse(args[0], out int spellId)) {
                p.Error("Invalid spell ID argument");
                return;
            }

            Arcana.CastSpell(p, spellId, true);
        }

        [Command(Rank.MOD, "reset a player's quest", new string[] { "player_name", "quest id" }, "resetquest")]
        public void ResetQuestState(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null) {
                p.Error("Invalid player name");
                return;
            }

            if (int.TryParse(args[1], out int id)) {
                pl.QuestManager.questStates[id].ForceState(0);
                Server.Instance.ScriptManager.ACTION_OnPlayerLogin[id](pl);
            } else {
                p.Error("Invalid arguments, /resetquest <player_name> <quest id>");
            }
        }

        [Command(Rank.ADMIN, "sets your quest state", new string[] { "quest ID", "quest state" }, "quest")]
        public void SetQuestState(Player p, string[] args) {
            if (int.TryParse(args[0], out int id) && int.TryParse(args[1], out int state)) {
                p.QuestManager.questStates[id].ForceState(state);
            } else {
                p.Error("Invalid arguments, /quest <questID> <questState>");
            }
        }

        [Command(Rank.MOD, "Teleports you to a set location", new string[] { "location name", }, "tele")]
        public void Tele(Player p, string[] args) {
            Dictionary<string, Vector3> teleportLocations = new Dictionary<string, Vector3>() {
                { "gmbank", new Vector3(-47.2f,126f,773.3f) },
                { "gmtower", new Vector3(-2.7f,175f,770.2f) },
                { "gmtower2", new Vector3(-86.9f,148.3f,801.3f) },
                { "spawn", new Vector3(-186f,85f,1017f) },
                { "lake", new Vector3(-301.4f,83.3f,1007.3f) },
                { "windmill", new Vector3(-102.1f,134.5f,692.2f) },
                { "alch", new Vector3(-313.4f,94.6f,861.1f) },
                { "beach", new Vector3(-635.4f,27.6f,737.5f) },
                { "island", new Vector3(-960.3f,34.9f,770.8f) },
                { "gmcoal", new Vector3(-320.4f,64.8f,808.2f) },
                { "farmine", new Vector3(-29f,101.1f,1153.6f) },
                { "farm", new Vector3(-132.2f,43f,284.1f) },
                { "lumber", new Vector3(-175.2f,90.1f,965.3f) },
                { "statue", new Vector3(-197.7f,88.8f,835.4f) },
                { "fishing", new Vector3(-265.8f,83.6f,990.7f)},
                { "murray", new Vector3(-122.4f,98.5f,1050.4f)},


            };

            var area = args[0].ToLower();
            if (!teleportLocations.ContainsKey(area)) {
                p.Error("Invalid TP Name");
                return;
            }

            Vector3 loc = teleportLocations[area];
            p.TeleportTo(0, loc.X, loc.Y + 2, loc.Z);
        }

        [Command(Rank.USER, "makes your character follow another player", new string[] { "player_name"}, "follow")]
        public void Follow(Player p, string [] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null && !pl.Equals(p)) {
                p.Error("Invalid player name");
                return;
            }

            p.NetworkActions.SendFollow(pl);
        }

        [Command(Rank.MOD, "tracks the player's actions - report is sent to discord", new string[] { "player_name", "time(s)" }, "track")]
        public void Track(Player p, string[] args) {
            string name = args[0].Replace("_", " ");
            Player pl = Server.GetPlayerByName(name);
            if (pl == null && !pl.Equals(p)) {
                p.Error("Invalid player name");
                return;
            }

            if (!int.TryParse(args[1], out int time) || time <= 0) {
                p.Error("Invalid time");
                return;
            }

            pl.PlayerTracking.TrackPlayer(time);
            p.Msg("Tracking has begun for " + name);
        }

        [Command(Rank.USER, "tells you what world you are on", new string[] { }, "world")]
        public void World(Player p, string[] args)
        {
            p.Msg("You are on World " + Server.SERVER_World);
        }

        [Command(Rank.MOD, "slayer task functions", new string[] { "type" }, "slayer")]
        public void Slayer(Player p, string[] args)
        {
            String type = args[0];
            if(type == "check")
            {
                p.CheckSlayerTask();
            }
            if (type == "new")
            {
                p.GetSlayerTask();

            }
            if (type == "cancel")
            {
                p.CancelSlayerTask();

            }
        }

        [Command(Rank.ADMIN, "changes the characters Gender", new string[] {}, "swapgender")]
        public void ChangeGender(Player p, string[] args)
        {
            p.ToggleGender();
        }
    }
}