using ENet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TitanReach_Server.Network;
using TitanReach_Server.Network.Incoming;
using TitanReach_Server.Skills;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Enums;
using static TitanReach_Server.Database;
using static TitanReach_Server.Skills.Slayer;

namespace TitanReach_Server.Model
{
    public class Player : Entity
    {

        public int MapID = -1;
        public Player LastAttackedPlayer;
        public int LastMapChangeTime = Environment.TickCount;
        public int LastAttackedPlayerTime = Environment.TickCount;
        public bool Interacting = false;
        public Viewport Viewport;
        public string Email;
        public int questCountDebug = 0;
        public object outgoingPacketsLock = new object();
        public object incomingPacketsLock = new object();
        public ConcurrentQueue<NetworkActions.Message> outgoingPackets = new ConcurrentQueue<NetworkActions.Message>();
        public ConcurrentQueue<Event> incomingPackets = new ConcurrentQueue<Event>();
        public bool AdminMode = false;
        public int LastMovementViewportCheck = Environment.TickCount;
        public float rotation = 0;
        public int CurrentInteractedShop = -1;
        public int LastHeal = Environment.TickCount;
        public List<Buff> ActiveBuffs = new List<Buff>();
        public QuestManager QuestManager;
        public FriendsManager FriendsManager;
        public Player LastTradeRequested;
        public bool TradeInitiator = false;
        public int LastTradeRequestedTime = -1;
        public bool Trading = false;
        public Player TradingPlayer = null;
        public bool TradingAccepted = false;
        public int PlayerUptime = Environment.TickCount;
        public Dictionary<int, int> LastNpcAttackTimes = new Dictionary<int, int>();
        public Dictionary<uint, int> LastPlayerAttackTimes = new Dictionary<uint, int>();
        private bool invincible = false; // Gets set with the AdminPanel.cs where it sends a packet to be changed here.

        public List<AppearanceData> appearanceData; 

        public CancellationTokenSource ChatTokenSource;
        public Task DialogTask;

        public LOGIN_STEPS LoginStep = LOGIN_STEPS.NONE;
        public enum LOGIN_STEPS
        {
            NONE, CHECKING_CREDENTIALS, PROCESSING_LOGIN, FINALIZED
        }
        
       // public Indicator LastIndicator;
        public Rank Rank = Rank.USER;
        public bool Muted = false;
        public string clan;
        public List<Item> TradingItems = new List<Item>();

        public TitleManager TitleManager;
        public PetManager PetManager;

        public Party Party;
        public Dictionary<int, int> PartyInvites = new Dictionary<int, int>();

        //kill dictionary, maps npcID to bestiary entry
        public Dictionary<int, PlayerBestiary> Bestiary;
        public Action OnMapChange;
        public Action OnLogin;

        public PlayerTracking PlayerTracking;

        public Map Map => MapID == -1 ? null : Server.Instance.Maps[MapID];

        public void AdminChangeMap(int mapID)
        {
            Server.Log("Admin requesting map change");
          
            Vector3 vec = null;
            switch (mapID)
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
            }
            ChangeMap(mapID, vec);
        }

        public void ChangeMap(int mapID, Vector3 location)
        {
            // Remove us from everyones player
            lock (Map.Players)
            {
                Map.Players.Remove(this);
                foreach (Player p in Map.Players)
                {
                    if (p != null)
                    {
                        p.NetworkActions.DestroyPet(this);
                        p.NetworkActions.RemovePlayer(UID);
                        p.Viewport.PlayersInView.Remove(this);
                    }
                }
                Viewport.PlayersInView.Clear();
                Viewport.objectsInView.Clear();
                Viewport.NpcsInView.Clear();
                Viewport.groundItemsInView.Clear();
            }

            // new map load in
            MapID = mapID;


            NetworkActions.ChangeMap(MapID, Map.LandID, location, () => // on map loaded
            {

                transform.position = location;

                lock (Map.Players)
                {
                    foreach (Player p in Map.Players) {
                        p.Viewport.PlayersInView.Add(this);
                    }

                    Map.Players.Add(this);
                }
              


                Viewport.PlayersInView.AddRange(Map.Players);

                NetworkActions.SpawnPlayer(true);

                Viewport.FullUpdate();
                NetworkActions.SyncObjects();
                NetworkActions.SendLocalPlayerAreaEquipment();
                NetworkActions.SendLocalPlayerEquipmentUpdate();
                NetworkActions.SyncGroundItems();
                NetworkActions.SendLocation();
                LastMapChangeTime = Environment.TickCount;

            });

        }

        public float ServerHeight()
        {
            return DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z, Map.LandID);
        }
        public bool HasParty() {
            return Party != null;
        }

        private int stance;
        public int Stance {
            get {
                return stance;
            }
            set{
                if(value != stance) {
                    stance = value;
                    if (ACTION_OnStanceChange.ContainsKey(stance)){
                        ACTION_OnStanceChange[stance](this);
                    }
                }
            }
        }

        public int[] NpcDialogStates = new int[100];
        public Dictionary<int, Action<Player>> ACTION_OnSpawnObject = new Dictionary<int, Action<Player>>();
        public Dictionary<int, Action<Player>> ACTION_OnNpcKill = new Dictionary<int, Action<Player>>();
        public Dictionary<int, Action<Player>> ACTION_OnStanceChange = new Dictionary<int, Action<Player>>();

        public event EventHandler<int> RaiseNPCKilledEvent;
        public void TriggerRaiseNPCKilledEvent(int npcID)
        {
            RaiseNPCKilledEvent?.Invoke(this, npcID);
        }

        public void KillLogTrack(Object sender, int npcID)
        {
            if (!Bestiary.ContainsKey(npcID)) {
                Bestiary.Add(npcID, new PlayerBestiary(npcID, this));
            }

            Bestiary[npcID].AddKill();
        }

        public Vector3[] homes = new Vector3 [5];

        public Random rand;
        public bool Dead = false;
        public int DeathTime;

        public Stats Skills;

        public Bank Vault;
        private int _currentHealth;
        public int CurrentHealth
        {
            get => _currentHealth;
            set {
                _currentHealth = Math.Clamp(value, 0, GetMaxHealth());

                if (HasParty())
                {
                    Party.UpdatePartyInfo(this);
                }

                foreach(Player player in Viewport.PlayersInView)
                {
                    player.NetworkActions.SyncHealth(this);
                }
            }
        }
        public bool isMale = false;
        public bool NewCharacter = false;
        public int appearanceID = 0;
        public int shirtIndex = 0;
        public int pantsIndex = 0;
        public int shoeIndex = 0;
        public int hairIndex = 1;
        public int facialHairIndex = 0;
        public int skinColourIndex = 0;
        public int hairColourIndex = 0;
        public int faceialHairColourIndex = 0;
        public int shirtColourIndex = 0;
        public int pantsColourIndex = 0;
        public int shoesColourIndex = 0;
        public bool loggedOut = false;
        public bool pvp_enabled = false;
        public bool takenSword = false;

        public int GetMaxHealth()
        {

            // any other bonuses can be calculated here.
            return Skills.GetCurLevel((int)Stats.SKILLS.Vitality) * 10;
        }

        public ST_Timer SaveTimer = null;

        public String Describe()
        {
            string main = "[";
            string name = this.Name == null ? "null" : this.Name;
            string IP = this.peer.IP == null ? "null" : this.peer.IP;
            string hash = this.peer.GetHashCode().ToString();
            int CID = (int)this.UID;
            int AID = (int)this.AccountID;
            return main + name + "|" + IP + "|Hash:" + hash + "|AID:" + AID + "|CID:" + CID + "]";
        }

        public void ToggleGender()
        {
            Server.Log("Toggling Gender");
            Server.Log("Gender Was: " + isMale);
            isMale = !isMale;
            Server.Log("Gender Is: " + isMale);
            NetworkActions.PlayerChangeGender();
        }

        public bool IsFull()
        {
            return Inventory.FreeSpace() <= 0;
        }

        public void FullError()
        {
            Error("Your inventory is full");
        }

        public void ApplyBuff(Buff buff)
        {
            if (Dead)
                return;
            ActiveBuffs.Add(buff);

            if (HasParty()) {
                Party.UpdatePartyBuffs(this, buff);
            }

            NetworkActions.UpdateBuff(buff.BuffID, buff.Duration, buff.ArgArray);
        }

        public void SetPVP(bool p_pvp_enabled)
        {
            if (pvp_enabled == p_pvp_enabled) return;
            if (p_pvp_enabled)
            {
                pvp_enabled = true;
                NetworkActions.SendMessage("Arena pvp enabled, good luck");
            }
            if (!p_pvp_enabled)
            {
                pvp_enabled = false;
                NetworkActions.SendMessage("Arena pvp disabled");
            }
        }

        public Player()
        {
            transform = new Transform(Locations.PLAYER_SPAWN_POINT, new Vector3(0f, 177f, 0f), new Vector3(7.654061f, 7.654061f, 7.654061f));
            Viewport = new Viewport(this);
            this.Inventory = new Inventory(this);
            this.Equipment = new Equipment(this);
            this.Skills = new Stats(this);
            this.Vault = new Bank(this);
            this.QuestManager = new QuestManager(this);
            this.FriendsManager = new FriendsManager(this);
            this.TitleManager = new TitleManager(this);
            this.PetManager = new PetManager(this);
            this.PlayerTracking = new PlayerTracking(this);
            Bestiary = new Dictionary<int, PlayerBestiary>();
            RaiseNPCKilledEvent += KillLogTrack;
            LoadPlayerCustomOptions();

        }

        private SlayerTask _slayerTask = null;


        public async void GetSlayerTask()
        {

            if (_slayerTask != null)
            {
                Msg(SlayerTask.AlreadyTaskInfor());
            }
            else
            {
                SlayerTask slayerTask = Slayer.GenerateSlayerTask(this);
                Response response = await Server.Instance.DB.UpdateSlayerTask((int)AccountID, slayerTask._totalAmount,0, slayerTask._npcCategory) ;

                if (response.Error)
                {
                    Msg("Failed obtaining slayertask");
                    Server.Log("Failed obtaining slayertask for player " + Name);
                    return;
                }
                else
                {
                    _slayerTask = slayerTask;
                    Msg(_slayerTask.NewTaskInfo());
                    Msg(_slayerTask.Warning());

                    RaiseNPCKilledEvent += _slayerTask.EnemyKilledTrigger;
                }
            }
        }

        public void CheckSlayerTask()
        {
            if (_slayerTask == null) Msg(SlayerTask.NoTaskInfo());
            else Msg(_slayerTask.CurrentTaskInfo());
           
        }

        public void SetCurrentSlayerTask(int npccategory, int totalkills, int currentKills)
        {
            _slayerTask = Slayer.GenerateCurrentSlayerTask(npccategory, totalkills, currentKills);
            Msg(_slayerTask.CurrentTaskInfo());
        }


        private async void UpdateSlayerDataBase(SlayerTask task)
        {
            Response response = await Server.Instance.DB.UpdateSlayerTask((int)AccountID, task._totalAmount, task._totalAmount - task._currentAmount, task._npcCategory);

        }

        public void TriggerSlayerDatabaseUpdate()
        {
            UpdateSlayerDataBase(_slayerTask);
        }

        public void CompleteSlayerTask()
        {
            UpdateSlayerDataBase(_slayerTask);

            RaiseNPCKilledEvent -= _slayerTask.EnemyKilledTrigger;
            Msg(_slayerTask.CompleteTaskInfo());
            _slayerTask = null;
        }

        public void CancelSlayerTask()
        {
            if(_slayerTask == null) Msg(SlayerTask.NoTaskInfo());
            else
            {
                RaiseNPCKilledEvent -= _slayerTask.EnemyKilledTrigger;
                Msg(SlayerTask.CancelTaskInfo());
                _slayerTask = null;
            }
           
        }

        private void LoadPlayerCustomOptions()
        {
            isMale = true;
            shirtIndex = 1;
            pantsIndex = 1;
            shoeIndex = 1;
            hairIndex = 1;
            facialHairIndex = 0;
            skinColourIndex = 1;
            hairColourIndex = 1;
            faceialHairColourIndex = 1;
            shirtColourIndex = 1;
            pantsColourIndex = 1;
            shoesColourIndex = 2;
        }

        public void RandomizeCustomOptions()
        {
            Random r = new Random();

            isMale = r.NextDouble() < 0.5;
            shirtIndex = (int)(r.NextDouble() * 6) + 1;
            pantsIndex = (int)(r.NextDouble() * 3) + 1;
            shoeIndex = (int)(r.NextDouble() * 2) + 1;
            hairIndex = (int)(r.NextDouble() * 38) + 1;
            facialHairIndex = (int)(r.NextDouble() * 19);
            skinColourIndex = (int)(r.NextDouble() * 5);
            hairColourIndex = (int)(r.NextDouble() * 5);
            faceialHairColourIndex = (int)(r.NextDouble() * 5);
            shirtColourIndex = (int)(r.NextDouble() * 9);
            pantsColourIndex = (int)(r.NextDouble() * 9);
            shoesColourIndex = (int)(r.NextDouble() * 9);
            NetworkActions.PlayerCustomUpdate();
        }

        public bool Busy = false;

        public void SetBusy(bool busy)
        {
            Busy = busy;

        }

        public int GetState(Npc n)
        {
            return NpcDialogStates[n.ID];
        }

        public void SetState(Npc n, int state)
        {
            NpcDialogStates[n.ID] = state;
        }


        public enum BusyType { FULL_LOCK, ACTION };
        public ST_Timer BusyTimer;
        public int LastFood = Environment.TickCount;
        public int LastFoodDelayTime = 0;
        public int LastCraftTime = Environment.TickCount;
        public ST_Timer LastActionTimer = null;
        public int LastActiveAction = Environment.TickCount;

        public void StopInteracting()
        {
            Busy = false;
            Interacting = false;
            NetworkActions.SendBusyState(false);
            if (BusyTimer != null)
                BusyTimer.Stop();
            NetworkActions.StopInteracting();
            if (LastActionTimer != null)
                LastActionTimer.Stop();
        }

        public void BusyDelay(BusyType type, string name, int delay, Action<ST_Timer, Object[]> action)
        {
            SetBusy(true);
            if (type == BusyType.FULL_LOCK)
                NetworkActions.SendBusyState(true, delay, name);

            ST_Timer tim = new ST_Timer();
            tim.Delay = delay;
            BusyTimer = tim;
            tim.Action = action;
            tim.FinishedCallback = () =>
            {

                SetBusy(false);
                NetworkActions.SendBusyState(false);
                BusyTimer = null;
            };

            Server.Instance.timers.Add(tim);

        }

        /*
        public void EndDialog(Npc n, string npcgoodbye)
        {
            this.SetBusy(false);
            n.Busy = false;
            NetworkActions.EndDialog((uint)n.UID, npcgoodbye);
        }

        public void EndDialog(Npc n)
        {
            this.SetBusy(false);
            n.Busy = false;
            NetworkActions.EndDialog((uint)n.UID, "");
        }
        */

        public void Heal(int amount)
        {
            int startHP = CurrentHealth;
            CurrentHealth += amount;
            if(CurrentHealth - startHP > 0)
                NetworkActions.SendHitSplat(this, null, CurrentHealth - startHP, (int)6);


        }

        public void ToggleAdminMode() 
        {
            AdminMode = !AdminMode;
            Msg("Admin mode is " + (AdminMode ? "enabled" : "disabled"));
            NetworkActions.SendAdminMode(AdminMode);
        }

        // Admin Functionality - Heals you to your max current HP.
        public void HealMax()
        {
            CurrentHealth = GetMaxHealth();
        }

        // Admin Functionality - Invincible - The bool gets used when damage is received.
        public void Invincible()
        {
            invincible = !invincible;
        }

        // Admin Functionality - Max out all Skills
        public void MaxAllSkills()
        {
            foreach (int skillInt in Enum.GetValues(typeof(Stats.SKILLS)))
            {
                Skills.SetLevelAndExp(skillInt, 100);
                NetworkActions.SyncExp(this, skillInt);
            }
        }

        // Admin Functionality - Reset All Skills back to 1 including the vitality.
        public void ResetAllSkills()
        {
            foreach (int skillInt in Enum.GetValues(typeof(Stats.SKILLS)))
            {
                Skills.SetLevelAndExp(skillInt, 1);
                NetworkActions.SyncExp(this, skillInt);
            }
        }

        // Admin Functionality - Setting Skills
        public void SetSkill(int skillInt, int setSkillLvl)
        {
            Skills.SetLevelAndExp(skillInt, setSkillLvl);
            NetworkActions.SyncExp(this, skillInt);
        }

        public int Damage(int dmg, string sender, DamageType source, int pid = -1)
        {
            if (sender != null && sender.Length > 1 && Environment.TickCount - LastMapChangeTime < 2500)
                return 0;
                if (this.AdminMode)
                return 0;
            if (Dead)
                return 0;

            if (!invincible)
            {
                CurrentHealth -= dmg;
            }

            foreach (Player pl in Map.Players)
            {
                if(pl == this || (pid != -1 && pl.UID == pid))
                    pl.NetworkActions.SendHitSplat(this, null, dmg, (int)source);
                pl.NetworkActions.PlayerDamaged(UID, dmg);
            }

            //NetworkActions.SyncExp(this, (int)Stats.SKILLS.Vitality);

            if (CurrentHealth <= 0)
            {
                    LastDeathByPlayer = pid;
                    Death(sender != null && sender.Length > 1);
                if (sender != null && sender.Length > 1)
                    Server.Instance.MessageAllPlayers("<color=orange>" + Name + "</color><color=white> has been killed by </color><color=orange>" + sender + " </color>");
            }

            return dmg;
        }
        public bool AltRespawn = false;
        public int LastDeathByPlayer = -1;
        public void Death(bool player = false)
        {
            pvp_enabled = false;
            Dead = true;
            DeathTime = Environment.TickCount;
            Busy = true;
            NetworkActions.SendMessage("<color=red>Oh dear! you are dead.</color>");
            NetworkActions.SendDeath(true);
            //  NetworkActions.SendAnimation(9);
            //NetworkActions.SendBusyState(true, 3500);
        }

        public bool InitDeath = false;

        public void BuffStat(int stat, int ammount)
        {
            int curLevel = Skills.GetCurLevel((int)stat);
            int actualLevel = Skills.GetMaxLevel((int)stat);

            if (curLevel > actualLevel + ammount)
            {
                return;
            }
            else
            {
                Skills.SetCurLevel(stat, actualLevel + ammount);
            }

            NetworkActions.SyncExp(this, stat);
        }

        public void UpdateStats()
        {
            foreach (Stats.SKILLS skill in Enum.GetValues(typeof(Stats.SKILLS)))
            {
                if (Skills.GetCurLevel((int)skill) > Skills.GetMaxLevel((int)skill))
                {
                    Skills.SetCurLevel((int)skill, Skills.GetCurLevel((int)skill) - 1);
                    NetworkActions.SyncExp(this, (int)skill);
                }
            }
        }

        public void Update()
        {
            if (Dead)
            {
                if (!InitDeath)
                {
                    ActiveBuffs.Clear();
                    NetworkActions.ClearBuffs();
                    InitDeath = true;
                }
                if (Environment.TickCount - DeathTime > 3500) // revive
                {
                    InitDeath = false;
                    for (int i = 0; i < Inventory.items.Length; i++)
                    {
                        var ite = Inventory.items[i];
                        if (ite == null)
                            continue;
                        if (ite.Item.ID == 16)
                            continue;
                        if (!ite.Item.Definition.Tradable)
                            continue;
                        var gi = new GroundItem(ite.Item.ID, MapID);
                        gi.Item.Amount = ite.Item.Amount;
                        if (LastDeathByPlayer != -1)
                            gi.ownerUID = (uint)LastDeathByPlayer;


                        gi.transform = new Transform(new Vector3(transform.position.X, DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z, Map.Land) + 1f, transform.position.Z), null, null);


                        int u = ite.Item.UID;


                        gi.Item.UID = u;

                        Server.Instance.Maps[MapID].GroundItems.Add(gi);
                        Inventory.items[i] = null;
                        foreach (Player pl in Map.Players)
                        {
                            pl.Viewport.groundItemsInView.Add(gi);
                            pl.NetworkActions.AddGroundItem(gi);
                        }


                        Server.Instance.Delay(90000, (timer, arg) =>
                        {
                            if (gi != null && Server.Instance.Maps[MapID].GroundItems.Contains(gi))
                            {

                                foreach (Player pl in Map.Players)
                                {
                                    pl.NetworkActions.RemoveGroundItem(gi.groundItemUID);
                                    pl.Viewport.groundItemsInView.Remove(gi);


                                }
                                Server.Instance.Maps[MapID].GroundItems.Remove(gi);
                            }
                        });
                    }

                    for (int i = 0; i < Equipment.EquippedItems.Length; i++)
                    {
                        Equipment.Unequip(i);
                    }

                    /*for (int i = 0; i < Inventory.items.Length; i++)
                    {
                        var ite = Inventory.items[i];
                        if (ite == null)
                            continue;

                        var gi = new GroundItem(ite.Item.ID);
                        gi.transform = new Transform(new Vector3(transform.position.X, DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z) + 1f, transform.position.Z), null, null);


                        int u = ite.Item.UID;

                        gi.Item.UID = u;
                        Server.Instance.groundItems.Add(gi);
                        Inventory.items[i] = null;
                        foreach (Player pl in Server.Instance.players)
                        {
                            pl.Viewport.groundItemsInView.Add(gi);
                            pl.NetworkActions.AddGroundItem(gi);
                        }
                    }*/ // why was this here? ^


                    NetworkActions.SendLocalPlayerEquipmentUpdate();
                    NetworkActions.SendLocalPlayerAreaEquipment();
                    Respawn();

                    // NetworkActions.SendAnimation()
                }
            }

            if (ActiveBuffs.Count > 0)
            {
                for (int i = 0; i < ActiveBuffs.Count; i++)
                {
                    Buff b = ActiveBuffs[i];
                    if (b != null)
                    {
                        if (b.GetDef().TickRate == -1)
                            continue;
                        if (Environment.TickCount - b.LastTick > b.GetDef().TickRate)
                        {
                            b.LastTick = Environment.TickCount;
                            b.OnTick(this);
                        }
                        if (Environment.TickCount - b.StartTime > b.Duration)
                        {
                            ActiveBuffs.Remove(b);
                            continue;
                        }
                    }
                }
            }


        }

        public void Respawn()
        {
            Dead = false;
            Busy = false;
            LastDeathByPlayer = -1;
            DeathTime = 0;
            CurrentHealth = GetMaxHealth();
            NetworkActions.SendDeath(false);

            // Skills.SetCurLevel((int)Stats.SKILLS.Vitality, Skills.GetMaxLevel((int)Stats.SKILLS.Vitality));

            NetworkActions.SendEquipment();
            NetworkActions.SendInventory();
            if (MapID != 0)
                ChangeMap(0, Locations.PLAYER_RESPAWN_POINT);
            else
            transform.position = Locations.PLAYER_RESPAWN_POINT;
         //   AltRespawn = false;
            // need to update player lists here
            lock (Viewport.PlayersInView)
            {
                foreach (Player p in Viewport.PlayersInView)
                    p.NetworkActions.SendLocation(this);
            }
        }

        public void TeleportTo(int land, float x, float y, float z)
        {
            // if you are in the map, then ignore but use SetDestination

            // If ChangeMap needs to occur, then this gets called
            ChangeMap(land, new Vector3(x, y, z));
            PlayerTracking.LastTP = DateTime.Now.Ticks;
        }

        public void Msg(string s)
        {
            NetworkActions.SendMessage(s);
        }

        public void Error(string s)
        {
            NetworkActions.SendMessage("<color=red>" + s + "</color>");
        }

        public void SyncQuestData()
        {

        }

        public bool QuestHistory = false;

        public void Init()
        {


            rand = new Random(Environment.TickCount);


            //Skills.SetLevelAndExp((int)Stats.SKILLS.Vitality, 15);
            Msg("Welcome to TitanReach!");
            if (NewCharacter)
                transform.position = Locations.PLAYER_SPAWN_POINT;
            loaded = true;
            NetworkActions.SendLocation();
            NetworkActions.SendInventory();
            NetworkActions.SendRank(Rank);
            NetworkActions.SyncVault();

            if (NewCharacter)
            {

                Skills.SetLevelAndExp((int)Stats.SKILLS.Vitality, 15);
                CurrentHealth = 150;
                Inventory.AddItem(16, 100);
            }
            NetworkActions.SyncAllExp(this);
            NetworkActions.SyncHealth(this);
          




            QuestManager.InitialiseAllQuestInfo();
            foreach (Action<Player> act in Server.Instance.ScriptManager.ACTION_OnPlayerLogin.Values) {
                act(this);
            }

            SaveTimer = Server.Instance.LoopedDelay(300000, (timer, args) =>
            {
                if (this == null || loggedOut)
                {
                    timer.Stop();
                    return;
                }
                if (loaded && !loggedOut)
                    Server.Instance.DB.SaveCharacter(this);
            });
            NetworkActions.SendCombatStance();


            OnLogin = () => {
                
                Viewport.PlayersInView.AddRange(Map.Players);
                NetworkActions.SpawnPlayer(true);
                Viewport.FullUpdate();
                NetworkActions.SyncObjects();
                NetworkActions.SendLocalPlayerAreaEquipment();
                NetworkActions.SendLocalPlayerEquipmentUpdate();
                NetworkActions.SyncGroundItems();
                PetManager.SyncPets();
                TitleManager.SyncTitles();

            };
            NetworkActions.SendGameReady();

 

        }

        public Obj GetObjectByUID(uint uid)
        {
            foreach (Obj p in Viewport.objectsInView)
            {
                if (p.UID.Equals((int)uid))
                    return p;
            }
            return null;
        }

        public Npc GetNpcByUID(uint uid)
        {
            foreach (Npc p in Viewport.NpcsInView)
            {
                if (p.UID.Equals((int)uid))
                    return p;
            }
            return null;
        }

        public GroundItem GetGroundItemByUID(uint uid)
        {
            foreach (GroundItem p in Viewport.groundItemsInView)
            {
            
                if (p.groundItemUID.Equals((int)uid))
                    return p;
            }
            return null;
        }

        public GroundItem GetGroundItemByID(int id)
        {
            foreach (GroundItem p in Viewport.groundItemsInView)
            {
                if (p.Item.ID.Equals((int)id))
                    return p;
            }
            return null;
        }

        public NetworkActions NetworkActions;


        private Transform _trans;
        public Transform transform
        {
            get
            {
                return _trans;
            } set
            {
                _trans = value;
            }
        }

        public void SetPosition(Vector3 pos)
        {
            this.transform.position = pos;
            lock (Viewport.PlayersInView)
            {
                foreach (Player p in Viewport.PlayersInView)
                    p.NetworkActions.SendLocation(this);
            }
        }

        public Inventory Inventory;

        public Equipment Equipment;

        public ENet.Peer peer;

        public int CurrentDialogOption = -1;

        public int LastPing = -1;

        public int PacketsRecv = 0;
        public int PacketsSent = 0;
        public bool Waiting = false;

        public void QueueIncomingPacket(Event packet)
        {
            this.incomingPackets.Enqueue(packet);
            PacketsRecv++;
        }

        public ST_Timer LastCallbackRef = null;
        public string NpcInteractionUpcomingMessage;

        public void DialogMessage(Npc npc, string text) {
            //show a message from an npc to a player
            NpcInteractionUpcomingMessage = text;
        }

        public async Task<int> DialogChoice(Npc npc, string summary, CancellationToken token, params string [] texts) {
            //show the player options
            //wait for a response
            //return the selected option

            if (token.IsCancellationRequested) {
      
                token.ThrowIfCancellationRequested();
                return -1;
            }

            CurrentDialogOption = -1;
            npc.SetBusy();
            npc.LastInteractAction = Environment.TickCount;
            NetworkActions.SendDialog(npc.Definition.Name, NpcInteractionUpcomingMessage, summary, false,  texts);

            while (CurrentDialogOption < 0)
            {
                if (token.IsCancellationRequested) {
            
                    token.ThrowIfCancellationRequested();
                    return -1;
                }
                //  Server.Log("Started loop");
                await Task.Delay(50);
            }
          //  Server.Log("ended loop");

            npc.LastInteractAction = Environment.TickCount;

            return CurrentDialogOption;
        }

        public async Task<int> Dialog(Npc npc, string text, CancellationToken token, bool speaker) {
            if (token.IsCancellationRequested) {
    
                token.ThrowIfCancellationRequested();
                return -1;
            }
            NpcInteractionUpcomingMessage = text;

            CurrentDialogOption = -1;
            npc.SetBusy();
            npc.LastInteractAction = Environment.TickCount;

            NetworkActions.SendDialog(npc.Definition.Name, NpcInteractionUpcomingMessage, null, speaker,   new string [] {"Next"});

            while (CurrentDialogOption < 0) {
                //  Server.Log("Started loop");
                if (token.IsCancellationRequested) {
           
                    token.ThrowIfCancellationRequested();
                    return -1;
                }
                await Task.Delay(50);
            }
            //  Server.Log("ended loop");

            npc.LastInteractAction = Environment.TickCount;

            return CurrentDialogOption;
        }

        public void KillInteraction()
        {

        }

        public bool loaded = false;

        public Database.AccountCharacterData[] charData;
        public uint AccountID;

        public void Disconnect(string reason = "", bool wait = false, bool dontsave = false)
        {
            if (loggedOut)
                return;
            lock (Server.Instance.WaitingPool)
                if (Server.Instance.WaitingPool.Contains(this))
                    Server.Instance.WaitingPool.Remove(this);

            Server.Log(Describe() + " left the server. Reason: " + reason);

            if (!dontsave && loaded) {
                Server.Instance.DB.SaveCharacter(this);
                Server.Instance.DB.SetOnline(this, false);
                FriendsManager.Logout();
            }
            if (Map != null)
            {
                lock (Map.Players)
                {
                    foreach (Player p in Map.Players)
                    {
                        if (p != this)
                        {
                            p.NetworkActions.DestroyPet(this);
                            p.NetworkActions.RemovePlayer(UID);
                            p.Viewport.PlayersInView.Remove(this);
                        }
                    }
                }
            }
            loggedOut = true;

            NetworkActions.SendLogout(reason);
            
            // Server.Instance.SendMessage(Name + " has left the server");

            if (HasParty()) {
                Server.Instance.PM.RemovePartyMember(Party.ID, this);
            }
            if (Trading)
                TRADING.ResetTrade(this);

            PlayerTracking.EndTracking();

            Server.Instance.AllPlayers.Remove(this);
            if (Map != null)
            {
                Map.Players.Remove(this);
            }
            LastActionTimer?.Stop();
            LastActionTimer = null;
            Server.Instance.playerRefs.Remove(peer.GetHashCode(), out Player pl);
            if (SaveTimer != null)
                SaveTimer.Stop();
            Server.Instance.timers.Remove(SaveTimer);
            SaveTimer = null;
            incomingPackets.Clear();
            try
            {
                if (wait)
                    peer.DisconnectLater(0);
                else
                    peer.DisconnectNow(0);
            }
            catch(Exception e) {

                Server.Log("Error @ Disconnect: " + e.Message + " - " + e.StackTrace);
            
            }
           

        }

        public bool OnSameMap(Player other) {
            return Map == other.Map;
        }

        public void Mute() {
            Muted = true;
        }

        public void UnMute() {
            Muted = false;
        }

        public int GetAccuracyBonus(DamageType damageType)
        {
            return Equipment.GetAccuracyBonus(damageType);
        }

        public int GetPowerBonus(DamageType damageType)
        {
            return Equipment.GetPowerBonus(damageType);
        }

        public int GetDefenceBonus(DamageType damageType)
        {
            return Equipment.GetDefenceBonus(damageType);
        }

        public int GetAccuracyLevel(DamageType damageType)
        {
            if (damageType == DamageType.MELEE) return Skills.GetCurLevel((int)Stats.SKILLS.Dexterity);
            else if (damageType == DamageType.RANGED) return Skills.GetCurLevel((int)Stats.SKILLS.Ranged);
            else if (damageType == DamageType.MAGIC) return Skills.GetCurLevel((int)Stats.SKILLS.Sorcery);
            else
            {
                Server.Error("invalid damageType Selected");
                return 0;
            }
        }

        public int GetPowerLevel(DamageType damageType)
        {
            if (damageType == DamageType.MELEE) return Skills.GetCurLevel((int)Stats.SKILLS.Strength);
            else if (damageType == DamageType.RANGED) return Skills.GetCurLevel((int)Stats.SKILLS.Ranged);
            else if (damageType == DamageType.MAGIC) return Skills.GetCurLevel((int)Stats.SKILLS.Sorcery);
            else
            {
                Server.Error("invalid damageType Selected");
                return 0;
            }
        }

        public int GetDefenceLevel(DamageType damageType)
        {
            if (damageType == DamageType.MELEE) return Skills.GetCurLevel((int)Stats.SKILLS.Defence);
            else if (damageType == DamageType.RANGED) return Skills.GetCurLevel((int)Stats.SKILLS.Defence);
            else if (damageType == DamageType.MAGIC) return Skills.GetCurLevel((int)Stats.SKILLS.Defence);
            else
            {
                Server.Error("invalid damageType Selected");
                return 0;
            }
        }

        public void AddCombatXP(WeaponCategory weaponCategory, int damage)
        {
            if (damage > 0)
            {
                DamageType damageType = TRShared.Data.Formula.EvaluateDamageType(weaponCategory);

                if (damageType == DamageType.MAGIC)
                {
                    Skills.AddExp((int)Stats.SKILLS.Vitality, (damage * 2) / 10);
                    Skills.AddExp((int)Stats.SKILLS.Sorcery, (damage * 4) / 10);
                    Skills.AddExp((int)Stats.SKILLS.Defence, (damage * 2) / 10);
                }
                else if (damageType == DamageType.RANGED)
                {
                    Skills.AddExp((int)Stats.SKILLS.Vitality, (int)(damage * 1.5f) / 10);
                    if (Stance == 2)
                    {
                        Skills.AddExp((int)Stats.SKILLS.Ranged, (int)(damage * 4.5) / 10);
                        Skills.AddExp((int)Stats.SKILLS.Defence, (int)(damage * 1.5f) / 10);
                    }
                    else
                    {
                        Skills.AddExp((int)Stats.SKILLS.Ranged, (damage * 6) / 10);
                    }

                }
                else if (damageType == DamageType.MELEE)
                {
                    Skills.AddExp((int)Stats.SKILLS.Vitality, (int)(damage * 1.5f) / 10);
                    if (Stance == 0)
                        Skills.AddExp((int)Stats.SKILLS.Dexterity, (damage * 6) / 10);
                    if (Stance == 1)
                        Skills.AddExp((int)Stats.SKILLS.Strength, (damage * 6) / 10);
                    if (Stance == 2)
                        Skills.AddExp((int)Stats.SKILLS.Defence, (damage * 6 / 10));
                }
            }
        }

        public override bool Equals(object obj) {
            return obj is Player player &&
                   UID == player.UID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(UID);
        }
    }
}
