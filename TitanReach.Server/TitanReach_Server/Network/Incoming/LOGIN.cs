using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Enums;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class LOGIN : IncomingPacketHandler
    {

        public int LOGIN_OK = 0;
        public int ERROR = 1;

        public int GetID()
        {
            return Packets.LOGIN;
        }

        public async void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            try
            {
                int subtype = packet.ReadByte();
                switch (subtype)
                {
                    case 3:


                        if (p.loaded)
                        {
                            Server.Suss(p, "Trying to be a dodgy fucker");
                            return;
                        }
                        p.LoginStep = Player.LOGIN_STEPS.CHECKING_CREDENTIALS;
                        string name = packet.ReadString(packet.ReadByte());
                        string pass = packet.ReadString(packet.ReadByte());
                        float ver = packet.ReadFloat();
                        p.Email = Formula.StripEmail(name);
                        p.Waiting = false;
                        //Server.Log("Email: " + p.Email + " attempting login");

                        // ============= CHECKING CREDENTIALS ETC =======================


                        Response res = await Server.Instance.DB.GetAccount(p.Email, pass, p.NetworkActions.peer.IP);
                        string error = null;

                        if (res.Error)
                        {
                            if (res.ErrorObject == null)
                                error = "Unknown Error (1)";
                            else
                                error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error;

                            // Server.ErrorDB(error);
                        }


                        if (Server.Instance.AllPlayers.Count >= Server.MAX_PLAYERS)
                            error = "Server Full";

                        if (Server.CLIENT_VERSION > ver)
                            error = "Client Outdated - Update from Website";

                        if (error != null)
                        {
                            p.NetworkActions.SendLoginResult(ERROR, error);
                            p.Disconnect(error, true, true);
                        }
                        else
                        {
                            AccountData accountData = res.value as AccountData;

                            lock (Server.Instance.WaitingPool)
                            {
                                if (Server.Instance.WaitingPool.SingleOrDefault(pp => pp.AccountID == (uint)accountData.pId) != null)
                                {
                                    error = "You are already logged in (Waiting Pool)";
                                    p.NetworkActions.SendLoginResult(ERROR, error);
                                    p.Disconnect(error, true, true);
                                    return;
                                }
                            }

                            lock (Server.Instance.AllPlayers)
                                if (Server.Instance.AllPlayers.SingleOrDefault(pp => pp.AccountID == (uint)accountData.pId) != null)
                                {
                                    error = "You are already logged in";
                                    p.NetworkActions.SendLoginResult(ERROR, error);
                                    p.Disconnect(error, true, true);
                                    return;
                                }


                            p.AccountID = (uint)accountData.pId;
                            p.UID = (uint)accountData.pId;
                            p.charData = accountData.characters.ToArray();




                            if (accountData.isBanned == 1)
                            {
                                error = "Your account has been banned";
                                p.NetworkActions.SendLoginResult(ERROR, error);
                                p.Disconnect(error, true, true);
                                return;
                            }
                            if (accountData.accountAccessLevels != null)
                                foreach (var vb in accountData.accountAccessLevels)
                                {
                                    if (vb.accessLevel == "Mod")
                                    {
                                        p.Rank = Rank.MOD;
                                    }

                                    if (vb.accessLevel == "Admin" || vb.accessLevel == "Dev")
                                    {
                                        p.Rank = Rank.ADMIN;
                                        break;
                                    }

                                }




                            if (Server.SERVER_DEV)
                            {
                                if (p.Rank != Rank.MOD && p.Rank != Rank.ADMIN)
                                {
                                    error = "This server is for developers only";
                                    p.NetworkActions.SendLoginResult(ERROR, error);
                                    p.Disconnect(error, true, true);
                                    return;
                                }

                                // On a dev server - allow mod rank to have full permission (dev accounts will be Mod rank)
                                p.Rank = Rank.ADMIN;
                            }


                            p.NetworkActions.SendLoginResult(LOGIN_OK);
                        }
                        break;

                    case 4:
                        if (p.loaded)
                            return; // Hack check


                        if (p.LoginStep > Player.LOGIN_STEPS.CHECKING_CREDENTIALS)
                        {
                            Server.Suss(p, "Trying to be a dodgy fucker pt 2");
                            return;
                        }
                        try
                        {
                            p.LoginStep = Player.LOGIN_STEPS.PROCESSING_LOGIN;
                            uint cid = packet.ReadUInt32();


                            //Server.Log("Character Login Attempt for " + cid);
                            if (p.charData?.SingleOrDefault(cha => cid == cha.characterId) == null) // ensuring cid is right
                                return;



                            p.AccountID = p.UID;
                            res = await Server.Instance.DB.GetCharacter((int)cid);
                            error = null;
                            string extError = "";

                            if (res.Error)
                            {
                                if (res.ErrorObject == null)
                                    error = "Unknown Error when fetching character data";
                                else
                                {
                                    error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error;
                                }
                            }

                            if (error != null)
                            {
                                p.Disconnect(error, true, true);
                                Server.Log("Error from AID: " + p.UID + ": " + error);
                                return;
                            }

                            lock (Server.Instance.WaitingPool)
                                Server.Instance.WaitingPool.Remove(p);

                            var val = res.value as Root;
                            

                            if(Server.Instance.AllPlayers.Where(x => x.UID == p.UID).SingleOrDefault() != null) // tried double logging
                            {
                                p.Disconnect("Already logged in", true, true);
                                return;
                            }
                            p.UID = (uint)val.characterId;
                            lock (Server.Instance.AllPlayers)
                                Server.Instance.AllPlayers.Add(p);
                            Server.Instance.DB.SetOnline(p, true);



                            p.Name = val.username;
                            // Load Character Data Here.
                            Location pos = val.location?[0];
                            p.transform.position = pos == null ? Locations.PLAYER_SPAWN_POINT : new Model.Vector3((float)pos.posX, (float)pos.posY, (float)pos.posZ);
                            p.MapID = pos == null ? 0 : pos.map;



                            //Response slayerResponse = await Server.Instance.DB.GetSlayerTask((int)p.AccountID);
                            //if (slayerResponse.Error)
                            //{
                            //    Server.Log("No Slayer Task for accountID " + p.AccountID + " --------------------------------");
                            //    Server.Log("Status Code:" + slayerResponse.StatusCode);
                            //    Server.Log("error :" + slayerResponse.ErrorObject.error);
                            //    Server.Log("error Code:" + slayerResponse.ErrorObject.errorCode);
                            //    Server.Log("internal error:" + slayerResponse.ErrorObject.internalError);
                            //}
                            //else
                            //{
                            //    //Server.Log("There is a slayer task for accoutnID " + p.AccountID + " --------------------------------");
                            //    //Server.Log(slayerResponse.value.ToString());

                            //    //CurrentSlayerTask cst = slayerResponse.value as CurrentSlayerTask;
                            //    //Server.Log(cst.npcCategory);
                            //    //Server.Log(cst.killsRequired);
                            //    //Server.Log(cst.killsComplete);


                            //    //p.SetCurrentSlayerTask(cst.npcCategory, cst.killsRequired, cst.killsComplete);
                            //}


                            if ((val.appearance == null || val.appearance.Count <= 1) && (p.appearanceData == null || p.appearanceData.Count <= 1)) // new char or no appearance data set
                            {
                                //Server.Log("Building new outfit");
                                string[] slotNames = { "Gender", "Skin Color Index", "Hair", "Facial Hair", "Torso", "Pants", "Shoe", "Gloves", "FacePaint" };
                                p.appearanceData = new List<AppearanceData>();
                                for (int i = 0; i < slotNames.Length; i++)
                                {
                                    AppearanceData dat = new AppearanceData();
                                    dat.slotId = i;
                                    dat.clothingId = 0;
                                    dat.slotColor1 = 0;
                                    dat.slotType = slotNames[i];
                                    p.appearanceData.Add(dat);
                                }
                            }


                            if (val.appearance != null && val.appearance.Count > 0 && p.appearanceData == null)
                                p.appearanceData = val.appearance;

                            var inv = val.inventory;
                          
                            if (inv != null && inv.Count > 0) {
                                foreach (var itm in inv) {
                                    if (itm.inventoryItemId < DataManager.ItemDefinitions.Length) {
                                        if (itm != null && DataManager.ItemDefinitions[itm.inventoryItemId] != null && !DataManager.ItemDefinitions[itm.inventoryItemId].Legacy)
                                        {
                                            p.Inventory.AddItem(new Item((ushort)itm.inventoryItemId, itm.inventoryItemAmount));
                                        }
                                    }
                                }
                            }
                            var bank = val.bank;
                            if (bank != null && bank.Count > 0)
                                foreach (var itm in bank)
                                    if (itm != null && DataManager.ItemDefinitions[itm.bankItemId] != null && !DataManager.ItemDefinitions[itm.bankItemId].Legacy)
                                        p.Vault.AddItem((ushort)itm.bankItemId, itm.bankItemAmount);


                            var equip = val.equipment;
                            if (equip != null && equip.Count > 0)
                                foreach (var itm in equip)
                                    if (itm != null && DataManager.ItemDefinitions[itm.equipmentItemId] != null && !DataManager.ItemDefinitions[itm.equipmentItemId].Legacy)
                                        p.Equipment.EquippedItems[itm.slotId] = new Item((ushort)itm.equipmentItemId, itm.equipmentItemAmount);

                            var stats = val.skills;
                            if (stats != null && stats.Count > 0)
                                foreach (var stat in stats)
                                {
                                    if (stat != null)
                                    {
                                        if (Stats.SkillNameToId.ContainsKey(stat.skillName))
                                        {
                                            var id = Stats.SkillNameToId[stat.skillName];
                                            p.Skills.SetExpAndLevel(id, stat.skillExperience);
                                        }

                                    }
                                }





                            //  p.NetworkActions.ChangeMap(0, 0);

                            List<FriendData> friends = val.friendsList;
                            p.FriendsManager.Login(friends);

                            List<TitleData> titles = val.unlockedTitles;
                            int? currentTitleID = val.title;
                            if (titles != null)
                            {
                                p.TitleManager.SetupTitles(titles, currentTitleID);
                            }

                            List<PetData> pets = val.unlockedPets;
                            int? currentPetID = val.activePet;
                            if (pets != null)
                            {
                                p.PetManager.SetupPets(pets, currentPetID);
                            }

                            List<QuestData> quests = val.quests;
                            p.QuestManager.LoadQuestProgress(quests);

                            List<KillData> killDatas = val.characterKills;
                            foreach (KillData killData in killDatas)
                            {
                                if (!p.Bestiary.ContainsKey(killData.npcId))
                                {
                                    p.Bestiary.Add(killData.npcId, new PlayerBestiary(killData.npcId, p));
                                }
                                DateTime.TryParseExact(killData.createdTimestamp, "yyyy-MM-dd hh:mm:ss.ttt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

                                if (dateTime == null)
                                {
                                    Server.Log("invalid format");
                                }
                                else
                                {
                                    p.Bestiary[killData.npcId].UpdateKillTimes(dateTime);
                                }
                            }

                            if (val.properties != null)
                                foreach (Property prop in val.properties)
                                {
                                    switch (prop.propertyName)
                                    {

                                        case "Health":
                                            p.CurrentHealth = int.Parse(prop.propertyValue);
                                            break;

                                        case "Stance":
                                            p.Stance = int.Parse(prop.propertyValue);
                                            break;
                                    }
                                }

                            //   Server.Log("vitality" + p.Skills.GetMaxLevel((int)Stats.SKILLS.Vitality));
                            if (p.Skills.GetMaxLevel((int)Stats.SKILLS.Vitality) < 15) {
                                p.Skills.SetLevelAndExp((int)Stats.SKILLS.Vitality, 15);
                                p.CurrentHealth = 150;
                            }

                            lock (p.Map.Players)
                            {
                                p.Map.Players.ForEach(user =>
                                {
                                    if (user != null)
                                    {
                                        lock (user.Viewport.PlayersInView)
                                        {
                                            user.Viewport.PlayersInView.Add(p);
                                        }
                                    }
                                });

                                p.Map.Players.Add(p);
                            }




                            Server.Instance.playerRefs[p.peer.GetHashCode()] = p;
                            Server.PLAYERS_SINCE_START++;
                            
                         
                            p.NetworkActions.SpawnPlayer(p);
                            p.Init();
                            break;
                        }
                        catch (Exception e)
                        {
                            Server.Error(p.Describe() + "Error during character login. Disconnecting player");
                            Server.Error(e);
                            p.Disconnect("Error during Login");
                            return;
                        }

                    case 5:
                        p?.Disconnect();
                        break;


                    case 6:
                        if (p.OnLogin != null)
                        {
                            p.OnLogin.Invoke();
                            p.OnLogin = null;
                        }

                        break;



                    case 7: // delete character

                        try
                        {

                            int cid = (int)packet.ReadUInt32();
                            int idx = -1;
                            for(int i=0; i < p.charData.Length; i++)
                            {
                                var c = p.charData[i];
                                if(c != null)
                                {
                                    if (c.characterId == cid)
                                    {
                                        idx = i;
                                        break;
                                    }
                                }
                            }

                            if(idx == -1)
                            {
                                Server.Suss(p," trying to delete character they don't own? " + cid);
                                return;
                            }
                            Response re = await Server.Instance.DB.DeleteCharacter(cid);
                            string errorr = null;
                            if (re.Error)
                            {
                                if (re.ErrorObject == null)
                                    errorr = "Unknown Error";
                                else
                                    errorr = re.ErrorObject.errorCode + " - " + re.ErrorObject.error;
                            }

                            if (errorr != null)
                            {
                                p.NetworkActions.SendLoginResult(2, errorr);
                                Server.ErrorDB("Error from AID: " + p.UID + ": " + errorr);
                                return;
                            }

                            p.charData[idx] = null;
                            var accData = new AccountCharacterData[p.charData.Length - 1];
                            int count = 0;
                            foreach(var v in p.charData)
                            {
                                if (v != null)
                                    if(count < accData.Length)
                                        accData[count] = v;
                                count++;
                            }
                            p.charData = accData;
                            p.NetworkActions.SendLoginResult(LOGIN_OK);
                        }
                        catch (Exception e)
                        {
                            Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                            p.Disconnect("Error handling packets");
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                p.Disconnect("Error handling packets");
            }
        }


    }
}
