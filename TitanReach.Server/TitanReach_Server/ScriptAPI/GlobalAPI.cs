using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using TRShared;
using TRShared.Data.Definitions;
using static TitanReach_Server.ScriptAPI.ScriptManager;

namespace TitanReach_Server.ScriptAPI {
    public class GlobalAPI {
        public CompiledScript Script;

        public const int EXIT = -1;
        public const int EXIT_FULL_INV = -2;

        public GlobalAPI(CompiledScript scr) {
            this.Script = scr;
        }

        #region Listener Registers

        public void RegisterPlayer(int scriptID, Action<Player> act) {
            Dictionary<int, Action<Player>> playerLoginDict = Server.Instance.ScriptManager.ACTION_OnPlayerLogin;
            if (playerLoginDict.ContainsKey(scriptID)) {
                playerLoginDict[scriptID] = null;
                Server.Log("[SCRIPT=" + Script.Name + "] Reloaded Player Listener");
            } else {
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded player Listener ");
            }

            playerLoginDict[scriptID] = act;
        }

        public void RegisterQuest(int questID, string questName, int completedState, Dictionary<int, QuestInfo> questInfo) {
            QuestManager.AddQuest(questID, questName, completedState, questInfo);
        }

        public void RegisterNPCQuest(int npcid, Func<Npc, Player, int, CancellationToken, Task<int>> func, int questID) {
            Dictionary<int, List<NPCQuestRegister>> NPCDictionary = Server.Instance.ScriptManager.ACTION_QuestNpc;

            NPCQuestRegister register = new NPCQuestRegister(questID, func);

            if (NPCDictionary.ContainsKey(npcid)) {
                List<NPCQuestRegister> registers = NPCDictionary[npcid];

                if (registers.Contains(register)) {
                    //replace old register
                    List<NPCQuestRegister> newRegisters = new List<NPCQuestRegister> {
                        register
                    };
                    NPCDictionary[npcid] = newRegisters;
                    Server.Log("[SCRIPT=" + Script.Name + "] Reloaded NpcInteract Listener");

                } else {
                    //add to exisiting register
                    registers.Add(register);
                    Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcInteract Listener");
                }

            } else {
                //create new register
                List<NPCQuestRegister> newRegisters = new List<NPCQuestRegister> {
                    register
                };
                NPCDictionary.Add(npcid, newRegisters);
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcInteract Listener");
            }
        }

        public void RegisterNPCChat(int npcid, Func<Npc, Player, int, CancellationToken, Task<int>> func) {
            Dictionary<int, Func<Npc, Player, int, CancellationToken, Task<int>>> ChatDictionary = Server.Instance.ScriptManager.ACTION_ChatNpc;
            if (ChatDictionary.ContainsKey(npcid)) {
                Server.Log("[SCRIPT=" + Script.Name + "] Reloaded NpcChat Listener");
            } else {
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcChat Listener");
            }

            ChatDictionary[npcid] = func;
        }

        public void RegisterNPCShop(int npcid, Action<Player> act, int shopID) {
            Dictionary<int, List<NPCShopRegister>> NPCDictionary = Server.Instance.ScriptManager.ACTION_ShopNpc;

            NPCShopRegister register = new NPCShopRegister(shopID, act);

            if (NPCDictionary.ContainsKey(npcid)) {
                List<NPCShopRegister> registers = NPCDictionary[npcid];

                if (registers.Contains(register)) {
                    //replace old register
                    List<NPCShopRegister> newRegisters = new List<NPCShopRegister> {
                        register
                    };
                    NPCDictionary[npcid] = newRegisters;
                    Server.Log("[SCRIPT=" + Script.Name + "] Reloaded NpcShop Listener");

                } else {
                    //add to exisiting register
                    registers.Add(register);
                    Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcShop Listener");
                }

            } else {
                //create new register
                List<NPCShopRegister> newRegisters = new List<NPCShopRegister> {
                    register
                };
                NPCDictionary.Add(npcid, newRegisters);
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcShop Listener");
            }
        }

        public void RegisterNPCBank(int npcid) {
            Dictionary<int, NPCBankRegister> NPCDictionary = Server.Instance.ScriptManager.ACTION_BankNpc;
            NPCDictionary[npcid] = new NPCBankRegister();
        }

        public void RegisterSpawnObject(int objectID, Player player, Action<Player> act) {
            Dictionary<int, Action<Player>> spawnObjectDictionary = player.ACTION_OnSpawnObject;
            if (spawnObjectDictionary.ContainsKey(objectID)) {
                Server.Log("[SCRIPT=" + Script.Name + "] Reloaded SpawnObject Listener for " + player.Name);
            } else {
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded SpawnObject Listener for " + player.Name);
            }

            spawnObjectDictionary[objectID] = act;
        }
        public void UnRegisterSpawnObject(int objectID, Player player) {
            Server.Log("[SCRIPT=" + Script.Name + "] Removed SpawnObject Listener for " + player.Name);
            player.ACTION_OnSpawnObject.Remove(objectID);
        }

        public void RegisterNpcKill(int npcID, Player player, Action<Player> act) {
            Dictionary<int, Action<Player>> npcKillDictionary = player.ACTION_OnNpcKill;
            if (npcKillDictionary.ContainsKey(npcID)) {
                Server.Log("[SCRIPT=" + Script.Name + "] Reloaded NpcKill Listener for " + player.Name);
            } else {
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded NpcKill Listener for " + player.Name);
            }

            npcKillDictionary[npcID] = act;
        }

        public void UnRegisterNpcKill(int npcID, Player player) {
            Server.Log("[SCRIPT=" + Script.Name + "] Removed NpcKill Listener for " + player.Name);
            player.ACTION_OnNpcKill.Remove(npcID);
        }

        public void RegisterStanceChange(int stance, Player player, Action<Player> act) {
            Dictionary<int, Action<Player>> stanceChangeDictionary = player.ACTION_OnStanceChange;
            if (stanceChangeDictionary.ContainsKey(stance)) {
                Server.Log("[SCRIPT=" + Script.Name + "] Reloaded StanceChange Listener for " + player.Name);
            } else {
                Server.Log("[SCRIPT=" + Script.Name + "] Loaded StanceChange Listener for " + player.Name);
            }

            stanceChangeDictionary[stance] = act;
        }

        public void UnRegisterStanceChange(int stance, Player player) {
            Server.Log("[SCRIPT=" + Script.Name + "] Removed NpcKill Listener for " + player.Name);
            player.ACTION_OnStanceChange.Remove(stance);
        }

        #endregion

        #region Chat methods

        public void Prompt(Player p, Npc n, string text) {
            p.DialogMessage(n, text);
        }

        public async Task Talk(Player p, Npc n, CancellationToken token, string text) {
            Task dialog = Task.Run(() => p.Dialog(n, text, token, true), token);
            await dialog;
        }

        public async Task Talk(Npc n, Player p, CancellationToken token, string text) {
            Task dialog = Task.Run(() => p.Dialog(n, text, token, false), token);
            await dialog;
        }

        public async Task<int> Response(Player p, Npc n, CancellationToken token, string summary, params string [] texts) {
            Task<int> response = Task.Run(() => p.DialogChoice(n, summary, token, texts), token);
            return await response;
        }

        public string NpcLink(int id, string extension) {
            return QuestManager.Npc(id, extension);
        }

        public string ItemLink(int id, string extension) {
            return QuestManager.Item(id, extension);
        }

        public string NpcLink(int id) {
            return NpcLink(id, "");
        }

        public string ItemLink(int id) {
            return ItemLink(id, "");
        }

        #endregion

        #region Quest Methods
        public int GetQuestState(Player p, int questID) {
            return p.QuestManager.GetQuestState(questID);
        }

        public bool CheckQuestFinished(Player p, int questID) {
            return p.QuestManager.Completed(questID);
        }

        public void CompleteStep(Player p, int questID, int newState, int step) {
            p.QuestManager.CompleteStep(questID, newState, step);
        }

        public void AddQuestStep(Player p, int questID, int nextStep, string text) {
            if(text == null) {
                Server.Log("got invalid quest step");
                return;
            }
            p.QuestManager.AddStep(questID, nextStep, text);
        }

        #endregion

        #region Misc methods

        public void ShowCharacterCreation(Player p)
        {
            p.NetworkActions.SendAppearanceRequest();
        }
        public string GetPlayerName(Player p) {
            return p.Name;
        }

        public bool CheckSkillLevel(Player p, int skill, int val) {
            return p.Skills.GetMaxLevel(skill) >= val;
        }

        public bool CheckCombatLevel(Player p, int val) {
            return p.Skills.GetCombatLevel() > val;
        }

        public void GiveExperience(Player player, int skill, int exp) {
            player.Skills.AddExp(skill, exp);
        }

        public void TeleportPlayer(Player p, int x, int y, int z) {
            p.TeleportTo(p.Map.LandID, x, y, z);
        }

        //public void SpawnAggressiveNpc(Player p, int npcID) {
        //    Vector3 pos = p.transform.position;
        //    Npc npc = SpawnNPC(p.Map, npcID, (int) pos.X, (int) pos.Y, (int) pos.Z);
        //    npc.AgroTargets.Add(p, 0);
        //}

        //public Npc SpawnNPC(Map map, int npcID, int x, int y, int z) {

        //    Npc npc = null;

        //    NpcSpawnDef def = new NpcSpawnDef();
        //    def.SpawnedAtRuntime = true;
        //    def.ID = npcID;
        //    def.Radius = 20;
        //    def.RespawnTime = -1;
        //    def.Amount = 1;
        //    //def.X = x;
        //    //def.Z = z;
        //    //def.Y = y;
        //    def.Position = new System.Numerics.Vector3(x, y, z);
        //    def.Direction = -1;
        //    def.CanMove = def.Radius != 0;

        //    map.Land.NpcSpawnDefinitions.Add(def);
        //    int npcidx = map.Land.NpcSpawnDefinitions.IndexOf(def);
        //    for (int i = 0; i < def.Amount; i++) {
        //        npc = new Npc(def.ID, npcidx, map.MapID);
        //        map.Npcs.Add(npc);
        //        foreach (Player pl in map.Players) {
        //            pl.Viewport.NpcsInView.Add(npc);
        //        }
        //    }

        //    foreach (Player pl in map.Players) {
        //        pl.NetworkActions.SyncNpcs();
        //    }

        //    return npc;
        //}

        public bool HasEquipped(Player player, int id) {
            foreach (var eq in player.Equipment.EquippedItems)
                if (eq != null)
                    if (eq.ID == id)
                        return true;

            return false;
        }

        public bool GiveItem(Player p, int itemID) {
            return GiveItem(p, itemID, 1);
        }

        public bool GiveStackingItem(Player p, int itemID, int amount) {
            if (p.Inventory.FreeSpace() > 0) {
                p.Inventory.AddItem((ushort)itemID, amount, true);
                return true;
            }
            return false;
        }

        public bool HasSpace(Player p, int amount) {
            return p.Inventory.FreeSpace() >= amount;
        }

        public bool GiveItem(Player p, int itemID, int amount) {
            if (HasSpace(p, amount)) {
                p.Inventory.AddItem((ushort)itemID, amount, true);
                return true;
            }
            return false;
        }

        public bool RemoveItem(Player p, int itemID, int amount) {
            if (HasInventory(p, itemID, amount)) {
                p.Inventory.RemoveItem((ushort)itemID, amount, true);
                return true;
            }
            return false;
        }

        public bool HasItem(Player p, int itemID) {
            return HasInventory(p, itemID, 1) || HasEquipped(p, itemID);
        }

        public bool HasInventory(Player p, int itemID, int amount) {
            return p.Inventory.CountItem((ushort)itemID) >= amount;
        }


        public void OpenShop(int shopNum, Player p) {
            p.NetworkActions.SendShop(DataManager.ShopDefinitions[shopNum]);
            p.CurrentInteractedShop = shopNum;
        }

        public void Message(Player p, string text) {
            p.Msg(text);
        }

        public void Log(string s) {
            //Vellak was here
            Server.Log(s);
        }

        public void Error(string s) {
            //KamiWala was here
            Server.Error(s);
        }



        #endregion
    }
}
