using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ENet;
using Nethereum.Signer;
using Newtonsoft.Json.Bson;
using TitanReach.Crypto;
using TitanReach.Server;
using TitanReach_Server.Model;
using TitanReach_Server.Network;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Scriptable;
using TitanReach_Server.ScriptAPI;
using TitanReach_Server.Skills;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using static TitanReach_Server.Database;

namespace TitanReach_Server
{
    public class Server
    {
        public ConcurrentDictionary<int, Map> Maps = new ConcurrentDictionary<int, Map>();

        public static string[] args;
        public static Assembly assembly;
        public static int CONNECTIONS_SINCE_START = 0;
        public static List<string> DiscordChatMessages = new List<string>();
        public static List<string> DiscordMessages = new List<string>();
        public static int IncomingPacketsPerSecond;
        public static int MAX_CLIENTS = 4020;
        public static int MAX_PLAYERS = 1000;
        public static int OutgoingPacketsPerSecond;
        public static int PLAYERS_SINCE_START = 0;
        public static ushort PORT = 12919;
        public static bool SERVER_DEV = false;
        public static string SERVER_IP = "localhost";
        public static bool SERVER_LOCAL;
        public static string SERVER_World = "200";
        public static int ShutdownTicks = 6;
        public static int TICK_RATE = 30;
        public static float CLIENT_VERSION = 0;
        private static readonly object _MessageLock = new object();
        private static Server singleton;
        public List<Vector3> ActiveFires = new List<Vector3>();
        public List<int> average = new List<int>();
        public List<string> BANNED_IPS = new List<string>();
        public ScriptManager ScriptManager;
        public Database DB;
        public PartyManager PM = new PartyManager();
        public Bestiary Bestiary;
        public Dictionary<int, Title> AllTitles;
        //public Dictionary<int, Pet> AllPets;
        private int fps;
        public int StartupTime = Environment.TickCount;

        private int frametime = Environment.TickCount;

        public HealthEngine healthengine;
        public List<int> incom = new List<int>();
        public List<int> outgo = new List<int>();

        public Dictionary<int, IncomingPacketHandler> incomingPacketHandlers =
            new Dictionary<int, IncomingPacketHandler>();

        public int LastAverage = Environment.TickCount;
        public int LastDiscordChat = Environment.TickCount;
        public int LastDiscordSend = Environment.TickCount;

        private int lastestopps = -1;

        private int lastframetime = -1;
        public int LastTicks = -1;

        private int latestipps = -1;
        public NetworkEngine network = new NetworkEngine();
        public Dictionary<int, INpcInteract> Npc_Interact_Listeners = new Dictionary<int, INpcInteract>();
        public ServerController npcmanager;
        public ConcurrentDictionary<int, Player> playerRefs = new ConcurrentDictionary<int, Player>();
        public List<Player> AllPlayers = new List<Player>();

        private readonly List<Utilities.Skill> skillClassesIgnore = new List<Utilities.Skill>();
        public List<ST_Timer> timers = new List<ST_Timer>();
        public bool UseDiscord;
        public string LocationFlag = "";
        public List<Player> WaitingPool = new List<Player>();
     

        public static Server Instance
        {
            get
            {
                if (singleton == null)
                    singleton = new Server();
                return singleton;
            }
        }

        public static void Debug(string msg)
        {
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Debug");

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(msg + "\n");
                var disc = "[Debug: " + msg;
                DiscordMessages.Add(disc);
                //  Console.WriteLine("[" + Thread.CurrentThread.Name + "]: " + msg);
            }
        }

        public static void Error(Exception e)
        {
            Error(e.Message + " - " + e.StackTrace);
        }

        public static void Error(string msg)
        {
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(msg + "\n");
                var disc = "[ERROR]: " + msg;
                DiscordMessages.Add(disc);
                //  Console.WriteLine("[" + Thread.CurrentThread.Name + "]: " + msg);
            }
        }

        public static void ErrorDB(string msg, Exception e = null)
        {
            if (e != null)
                msg += "\n" + e.Message;
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error");

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(msg + "\n");
                var disc = "(Database) " + msg;
                Discord.MessageDB(disc);
               // DiscordMessages.Add(disc);
                //  Console.WriteLine("[" + Thread.CurrentThread.Name + "]: " + msg);
            }
        }

        public static void Important(string msg)
        {
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Important");

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(msg + "\n");
                var disc = msg;
                Discord.MessageDB(disc);
                // DiscordMessages.Add(disc);
                //  Console.WriteLine("[" + Thread.CurrentThread.Name + "]: " + msg);
            }
        }

        public static Player GetPlayerByName(string name)
        {
            string low = name.ToLower();
            lock (Instance.AllPlayers)
            {
                foreach (var p in Instance.AllPlayers)
                    if (p != null && p.Name.ToLower().Equals(low))
                        return p;
            }

            return null;
        }

        public static Player GetPlayerByUID(uint uid)
        {
            lock (Instance.AllPlayers)
            {
                foreach (var p in Instance.AllPlayers)
                    if (p != null && p.UID == uid)
                        return p;
            }

            return null;
        }

        public static void Log(string msg)
        {
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor =
                    Thread.CurrentThread.Name == "WebServer" ? ConsoleColor.Yellow : ConsoleColor.Green;
                Console.Write(Thread.CurrentThread.Name);

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(msg + "\n");
                var disc = "[" + SERVER_World + "]: " + msg;
                DiscordMessages.Add(disc);
                // Discord.Message("[**" + Thread.CurrentThread.Name + "**]: " + msg);
                //  Console.WriteLine("[" + Thread.CurrentThread.Name + "]: " + msg);
            }
        }

        public static void Log(int msg)
        {
            Log("" + msg);
        }

        public static void Log(float msg)
        {
            Log("" + msg);
        }

        public static void Log(Vector3 msg)
        {
            Log("x: " + msg.X + ", y: " + msg.Y + ", z: " + msg.Z);
        }

        public static ushort ReverseBytes(ushort value)
        {
            return (ushort) (((value & 0xFFU) << 8) | ((value & 0xFF00U) >> 8));
        }

        public static void Suss(Player player, string msg)
        {
            lock (_MessageLock)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Warn");

                Console.ResetColor();
                Console.Write("]: ");
                Console.Write(player.Describe() + ":" + msg + "\n");
                var disc = "[WARN]: " + player.Describe() + ":" + msg;
                DiscordMessages.Add(disc);
            }
        }

        public void Delay(int delay, Action<ST_Timer, object[]> action, params object[] arr)
        {
            var tim = new ST_Timer();
            tim.Delay = delay;
            tim.Action = action;
            tim.param = arr;

            Instance.timers.Add(tim);
        }

        public void Delay(int delay, Action<ST_Timer, object[]> action)
        {
            var tim = new ST_Timer();
            tim.Delay = delay;
            tim.Action = action;

            Instance.timers.Add(tim);
        }

        public string GetStatsString()
        {
            var stats = "------- World " + SERVER_World + " Stats -------\n";
            stats += "Players Online: " + AllPlayers.Count + "/" + MAX_PLAYERS + "\n";
            stats += "Cumulative Players: " + PLAYERS_SINCE_START + "\n";
            stats += "Cumulative Connections: " + CONNECTIONS_SINCE_START + "\n";
            stats += "Host.ConnectedPeers: " + network.server.PeersCount + "\n";
            stats += "Waiting Pool Size: " + WaitingPool.Count + "\n";
            stats += "FrameTime: " + lastframetime + "\n";
            stats += "Outgoing PPS: " + lastestopps + "\n";
            stats += "Incoming PPS: " + latestipps + "\n";
            stats += "Largest Small-Packet Size: " + NetworkActions.SMALL_PACKET_COUNT + "/" +
                     NetworkActions.SMALL_PACKET_MAX_SIZE + "\n";
            stats += "Largest Large-Packet Size: " + NetworkActions.LARGE_PACKET_COUNT + "/" +
                     NetworkActions.LARGE_PACKET_MAX_SIZE + "\n";
            //     stats += "Throttle: " + ENet.Library. + "\n";

            return stats;
        }

        public void HandleIncomingPacket(Player p, Event netEvent)
        {
            if (p.peer.ID == netEvent.Peer.ID)
            {
             
                byte[] dat = new byte[netEvent.Packet.Length];
                
                netEvent.Packet.CopyTo(dat);
                int len = (dat[2]) | dat[3] << 8;
                byte[] data = new byte[len];
                Array.Copy(dat, data, len);
                dat = null;
                var packet = new MessageBuffer(data, false);
                netEvent.Packet.Dispose();
                var PacketID = packet.ReadUInt16();
                var Len = packet.ReadUInt16();
                
              
                if (Len != len)
                {
                    Log("INVALID PACKET " + netEvent.Packet.Length + " / " + Len);
                    packet.Dispose();
                    packet = null;
                    return;
                }
                try
                {
                    if (incomingPacketHandlers.ContainsKey(PacketID))
                        incomingPacketHandlers[PacketID].OnPacketReceivedAsync(p, Len, packet);
                } catch(Exception e)
                {
                    Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                    p.Disconnect("Error handling packets");
                }
                packet.Dispose();
                packet = null;
                
            }
        }

        public static string ServerIP;
        public static int ServerPort;
        public async Task Init()
        {
            TRShared.Logger.OnSharedLog += Logger_OnSharedLog; ;

            //   TRShared
            assembly = Assembly.GetEntryAssembly();
            Thread.CurrentThread.Name = "CoreThread";
            Library.Initialize();
            LoadHandlers();

            DataLoader.Load();

            StartNpcManager();
            StartHealthEngine();
            ProcessArgs();

            Map map = new Map();
            map.MapID = 0;
            map.LandID = (int)Lands.MAIN_WORLD;
         
            Maps.TryAdd(map.MapID, map);
            map.Load();

            map = new Map();
            map.MapID = 1;
            map.PvpEnabled = true;
            map.LandID = (int)Lands.PVP_ISLAND;
            Maps.TryAdd(map.MapID, map);
            map.Load();


            map = new Map();
            map.MapID = 2;
            map.LandID = (int)Lands.ALYSSIA_CAVE;
            Maps.TryAdd(map.MapID, map);
            map.Load();

            map = new Map();
            map.MapID = 3;
            map.LandID = (int)Lands.DEV_WORLD;
            Maps.TryAdd(map.MapID, map);
            map.Load();


            map = new Map();
            map.MapID = 4;
            map.LandID = (int)Lands.OASIS;
            Maps.TryAdd(map.MapID, map);
            map.Load();

            map = new Map();
            map.MapID = 5;
            map.LandID = (int)Lands.DROMHEIM;
            Maps.TryAdd(map.MapID, map);
            map.Load();

            network.server = new Host();

            var address = new Address();

            var ip = SERVER_World == "200" ? SERVER_World : SERVER_IP;
            if (SERVER_World == "200")
                SERVER_LOCAL = true;
            if (SERVER_World == "150")
                SERVER_DEV = true;

            
            address.SetHost(ip);
            address.Port = PORT;
            ServerIP = ip;
            ServerPort = PORT;
            DB = new Database();
            await Server.Instance.DB.RefreshAccessToken();
            AllTitles = new Dictionary<int, Title>();
            await Server.Instance.DB.GetAllTitles(AllTitles);
            //AllPets = new Dictionary<int, Pet>();  //replaced with local pets database
            //await Server.Instance.DB.GetAllPets(AllPets);
            //DB.ClearLoggedInPlayersInThisWorld(int.Parse(SERVER_World));
            ScriptManager = new ScriptManager();
            ScriptManager.Init();

            Bestiary = new Bestiary();
            Log("Started World " + SERVER_World + " [" + ip + ":" + PORT + "]");
            Discord.Message("----- " + SERVER_World + " has Started -----");
            Server.Log("UDP: Listening on " + address.GetIP() + address.Port);
            network.server.Create(address, MAX_CLIENTS, 0);
            ServerCheckin();
            Discord.MessageDB("Server Online");

        }

        private void Logger_OnSharedLog(object sender, string e)
        {
            Console.WriteLine(e);
        }

        public void LoadHandlers()
        {
            // Load Packet Handlers
            foreach (var ti in assembly.DefinedTypes)
                if (ti.ImplementedInterfaces.Contains(typeof(IncomingPacketHandler)))
                {
                    var pkt = assembly.CreateInstance(ti.FullName) as IncomingPacketHandler;
                    incomingPacketHandlers[pkt.GetID()] = pkt;
                }

            // Load NPC Scripts
            foreach (var ti in assembly.DefinedTypes)
                if (ti.ImplementedInterfaces.Contains(typeof(INpcInteract)))
                {
                    var pkt = assembly.CreateInstance(ti.FullName) as INpcInteract;
                    Npc_Interact_Listeners[pkt.NpcID()] = pkt;
                }

            // Load NPC Behaviours
            foreach (var ti in assembly.DefinedTypes)
                if (ti.BaseType == typeof(Behaviour))
                {
                    var pkt = assembly.CreateInstance(ti.FullName) as Behaviour;
                    var id = pkt.NpcID();
                    DataLoader.NpcBehaviours[id] = ti.FullName + "";
                    pkt = null;
                    Log("Loaded NPC AI for " + ti.Name);
                }

            //Initiate Skill classes
            foreach (var ti in assembly.DefinedTypes)
                if (ti.BaseType == typeof(Utilities.Skill))
                {
                    var skill = assembly.CreateInstance(ti.FullName) as Utilities.Skill;
                    skillClassesIgnore.Add(skill);
                }
        }
        bool set = false;
        public async void Loop()
        {
            if (set)
                return;

            set = true;

            await Init();
            var LastCheck = Environment.TickCount;
            var Last10Check = Environment.TickCount;
          //  new Thread(network.Start).Start();
            while (true)
            {
                try
                {
                    if (Environment.TickCount - Last10Check >= 60000 && !Server.SHUTDOWN)
                    {
                        //Debug(GetStatsString());
                        ServerCheckin();
                        Last10Check = Environment.TickCount;
                    }

                    if (Environment.TickCount - LastCheck >= 1000)
                    {
                        /* if (Environment.TickCount - LastAverage >= 60000)
                         {
                             float round = 0;
                             foreach (int va in average)
                                 round += va;

                             float avg = round / (float)average.Count;

                             round = 0;
                             foreach (int va in incom)
                                 round += va;

                             float avgincom = round / (float)incom.Count;

                             round = 0;
                             foreach (int va in outgo)
                                 round += va;

                             float avgoutgo = round / (float)outgo.Count;
                             Console.WriteLine("Last Minute: Averaged " + (int)avg + " TPS. Averaged " + (int)avgincom + " Incoming. Averaged " + (int)avgoutgo + " Outgoing");
                             LastAverage = Environment.TickCount;
                             average.Clear();
                             incom.Clear();
                             outgo.Clear();
                         }
                         */

                        var newi = IncomingPacketsPerSecond;
                        var newo = OutgoingPacketsPerSecond;
                        latestipps = IncomingPacketsPerSecond;
                        lastestopps = OutgoingPacketsPerSecond;
                        var newfps = fps;
                        average.Add(newfps);
                        incom.Add(newi);
                        outgo.Add(newo);
                        //  Console.Title = "Ticks Per Second: " + fps;
                        lastframetime = Environment.TickCount - frametime;
                        //Console.Title = SERVER_World + " - TPS: " + fps + " Outgoing: " + OutgoingPacketsPerSecond + " Incoming: " + IncomingPacketsPerSecond + " Players: " + players.Count() + " FrameTime: " + (Environment.TickCount - frametime);
                        LastTicks = fps;

                        IncomingPacketsPerSecond = 0;
                        OutgoingPacketsPerSecond = 0;

                        fps = 0;
                        LastCheck = Environment.TickCount;
                    }

                    frametime = Environment.TickCount;
                    ProcessTimers();
                    //  network.CheckData();
                    network.Poll();
                    network.ProcessIncomingPackets();
                    // network.ProcessOutgoingPackets();
                    if (Server.Instance.UseDiscord)
                        SendDiscord();
                    SendChat();
                    fps++;
                }
                catch (Exception e)
                {
                    Error("A BAD EXCEPTION - SERVER IS PROB FUCKED NOW. MAIN SERVER LOOP THREW THIS, DONT LET IT HAPPEN AGAIN: " +
                        e.Message + " | " + e.StackTrace);
                }

                Thread.Sleep(1);
            }
        }
        public static bool SHUTDOWN = false;
        Thread IdleThread;

        public void Shutdown(int type = 0) {
            if (!SHUTDOWN)
            {
                
                DisconnectAllPlayers();
                SHUTDOWN = true;
                ServerCheckin();
                Server.Instance.Delay(30000, (timer, other) =>
                {
                    Discord.MessageDB("Server OFFLINE");
                    Server.Log("---- Server has  Ended! ----");
                    if (type != 1)
                    {
                        SHUTDOWN = true;
                        if (IdleThread == null)
                        {
                            IdleThread = new Thread(() =>
                            {
                                while (true)
                                    Thread.Sleep(1000);
                            });
                            IdleThread.Start();
                        }
                    }
                    else Environment.Exit(0);
                });
                
            }
        }

        public void Restart() {
            Shutdown(1);
        }

        public void SaveAllPlayers() {
            foreach (var p in AllPlayers) {
                DB.SaveCharacter(p);
            }
        }

        public void DisconnectAllPlayers()
        {
            for(int i=0; i < AllPlayers.Count; i++)
            {
                Player p = AllPlayers[i];
                if(p != null)
                {
                    p.Disconnect("Server Shutdown");
                }
            }
        }

        public ST_Timer LoopedDelay(int delay, Action<ST_Timer, object[]> action)
        {
            var tim = new ST_Timer();
            tim.Delay = delay;
            tim.Action = action;
            tim.FinishedCallback = () => { LoopedDelay(delay, action); };
            Instance.timers.Add(tim);
            return tim;
        }
        public static string EXTERNAL_IP = null;
        public ST_Timer LoopedDelay(int loops, int delay, Action<ST_Timer, object[]> action)
        {
            var tim = new ST_Timer();
            tim.Delay = delay;
            tim.Action = action;
            tim.LoopCount = loops;
            tim.FinishedCallback = () =>
            {
                tim.LoopCount--;
                if (tim.LoopCount > 0)
                    tim.Repeat = true;
            };
            Instance.timers.Add(tim);
            return tim;
        }

        public void ProcessArgs()
        {

            foreach (var s in args)
            {
                var type = s.Split(':');
                switch (type[0])
                {
                    case "-Discord":
                        if (type[1] == "true")
                            Instance.UseDiscord = true;
                        break;

                    case "-Location":
                        Instance.LocationFlag = type[1].Trim();
                        break;

                    case "-IP":
                        SERVER_IP = GetIPAddress();
                        break;

                    case "-ExtIP":
                        EXTERNAL_IP = type[1].Trim();
                        break;

                    case "-Port":
                        PORT = (ushort) int.Parse(type[1]);
                        break;

                    case "-World":
                        SERVER_World = type[1];
                        break;

                    case "-MaxPlayers":
                        MAX_PLAYERS = int.Parse(type[1]);
                        break;

                    case "-ClientVersion":
                        CLIENT_VERSION = float.Parse(type[1]);
                        break;

                    case "-Dev":
                        SERVER_DEV = bool.Parse(type[1]);
                        break;

                }
            }
        }

        public void ProcessChatCommand(Player p, string msg) {
            string[] args = msg.Trim().Split(' ');
            if (args.Length == 0) return;

            //find relevant method
            MethodInfo methodInfo = GetMethodInfo(args[0]);
            if(methodInfo != null) {
                Command c = methodInfo.GetCustomAttribute<Command>();

                //check permissions
                if (!HasPermission(p, c.RequiredRank)) {
                    p.Error("You don't have permission to use this command.");
                    return;
                }

                //check args
                args = args.Skip(1).ToArray();
                if (args.Length != c.Args.Length) {
                    p.Error(c.GetExampleText());
                    return;
                }

                //call the method
                try {
                    methodInfo.Invoke(new ServerCommands(PM, this), new object[] { p, args });
                } catch (Exception e) {
                    Server.Log("Error in command: " + msg + ". Error: " + e.StackTrace);
                }
            } else {
                p.Error("Invalid command.");
            }
        }

        private MethodInfo GetMethodInfo(string commandString) {
            
            MethodInfo [] methods = typeof(ServerCommands).GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Command>().Count() > 0).ToArray();
            if (methods.Length > 0) {
                MethodInfo[] ms = methods.Where(m => m.GetCustomAttributes(false).OfType<Command>().First().CommandStrings.Contains(commandString.ToLower())).ToArray();
                return ms.Count() > 0 ? ms.First() : null;
            }
            return null;
        }

        private bool HasPermission(Player p, Rank requiredRank) {
            return p.Rank >= requiredRank || Server.SERVER_DEV;
        }

        public void ProcessTimers()
        {
            for (var i = 0; i < timers.Count; i++)
            {
                var p = timers[i];
                if (p == null)
                    continue;

                try
                {
                    if (Environment.TickCount - p.TimeStarted >= p.Delay)
                    {
                        p.Repeat = false;
                        p.Action?.Invoke(p, p.param);
                        p.FinishedCallback?.Invoke();
                        if (!p.Repeat)
                        {
                            timers.Remove(p);
                            p = null;
                        }
                        else
                        {
                            p.TimeStarted = Environment.TickCount;
                        }
                    }
                }
                catch (Exception e)
                {
                    Error(" Exception @ ProcessTimers: " + e.Message + " Stack: " + e.StackTrace);
                    timers.Remove(p);
                }
            }
        }

        public void RemovePlayer(uint peerID, string reason)
        {
            Player pl = null;
            lock (Instance.AllPlayers)
            {
                foreach (var p in AllPlayers)
                    if (p.peer.ID == peerID)
                    {
                        pl = p;
                        break;
                    }
            }
            if (pl == null)
            {
                lock (Instance.WaitingPool)
                {
                    foreach (var p in WaitingPool)
                        if (p.peer.ID == peerID)
                        {
                            pl = p;
                            break;
                        }
                }
            }

            if (pl != null) pl.Disconnect(reason, false, pl.Waiting);
        }

        public void SendChat()
        {
            if (Environment.TickCount - LastDiscordChat > 30000)
            {
                if (DiscordChatMessages.Count < 1)
                {
                    LastDiscordChat = Environment.TickCount;
                    return;
                }

                var total = "```";
                foreach (var str in DiscordChatMessages)
                {
                    if (total.Length > 1500)
                    {
                        Discord.Chat(total + "```");
                        total = "```";
                    }

                    total += "\n" + str;
                }

                total += "```";
                Discord.Chat(total);
                LastDiscordChat = Environment.TickCount;
                DiscordChatMessages.Clear();
            }
        }

        public void SendDiscord()
        {
            if (Environment.TickCount - LastDiscordSend > 60000)
            {
                if (DiscordMessages.Count < 1)
                {
                    LastDiscordSend = Environment.TickCount;
                    return;
                }

                var total = "```";
                foreach (var str in DiscordMessages)
                {
                    if (total.Length > 1500)
                    {
                        Discord.Message(total + "```");
                        total = "```";
                    }

                    total += "\n" + str;
                }

                total += "```" + "Tickrate: **" + LastTicks + "** Outgoing PPS: **" + OutgoingPacketsPerSecond +
                         "** Incoming PPS: **" + IncomingPacketsPerSecond + "** Players Online: __**" +
                         AllPlayers.Count() + "**__";
                Server.Log(total);
                Discord.Message(total);
                LastDiscordSend = Environment.TickCount;
                DiscordMessages.Clear();
            }
        }

        public void MessageAllPlayers(string msg)
        {
            lock (Instance.AllPlayers)
            {
                foreach (var pl in AllPlayers) pl.NetworkActions.SendMessage(msg);
            }
        }

        public static string GetIPAddress()
        {
            var firstAddress = (from address in NetworkInterface.GetAllNetworkInterfaces()
                    .Select(x => x.GetIPProperties()).SelectMany(x => x.UnicastAddresses).Select(x => x.Address)
                where !IPAddress.IsLoopback(address) && address.AddressFamily == AddressFamily.InterNetwork
                select address).FirstOrDefault();
            return firstAddress.ToString();
        }

        private void StartHealthEngine()
        {
            healthengine = new HealthEngine();
        }

        private void StartNpcManager()
        {
            npcmanager = new ServerController();
            npcmanager.StartManager();
        }

        public async void ServerCheckin()
        {
            Database.ServerCheckin data = new Database.ServerCheckin();
            data.worldPort = ServerPort;
            data.worldIPAddress = EXTERNAL_IP == null ? ServerIP : EXTERNAL_IP;
            data.worldRegion = LocationFlag;
            data.worldName = Server.SERVER_LOCAL ? "Local" : "World " + int.Parse(SERVER_World);
            data.worldMaxPlayers = MAX_PLAYERS;
            data.averageTickRate = LastTicks;
            data.worldOnline = Server.SHUTDOWN ? 0 : 1;
            data.consoleLog = "l8r";
            data.cumulativeConnections = CONNECTIONS_SINCE_START;
            data.cumulativePlayers = PLAYERS_SINCE_START;
            data.inPacketsPerSecond = latestipps;
            data.outPacketsPerSecond = lastestopps;
            data.serverUptime = Environment.TickCount - StartupTime;
            data.worldNumber = int.Parse(SERVER_World);
            data.players = new List<PlayerCheckin>();

            lock(Instance.AllPlayers)
            {
                foreach(Player p in Instance.AllPlayers)
                {
                    if(p != null)
                    {
                        Database.PlayerCheckin pla = new PlayerCheckin();
                        pla.characterId = (int)p.UID;
                        pla.characterUptime = Environment.TickCount - p.PlayerUptime;
                        data.players.Add(pla);
                    }
                }
            }


            Response res = await Instance.DB.WorldCheckIn(data);
            if(res.Error)
            {
                Server.Error("Error checking in world data: ");
            }
        }

        /*
         * 
         * 
         *             var stats = "------- World " + SERVER_World + " Stats -------\n";
            stats += "Players Online: " + players.Count + "/" + MAX_PLAYERS + "\n";
            stats += "Cumulative Players: " + PLAYERS_SINCE_START + "\n";
            stats += "Cumulative Connections: " + CONNECTIONS_SINCE_START + "\n";
            stats += "Host.ConnectedPeers: " + network.server.PeersCount + "\n";
            stats += "Waiting Pool Size: " + WaitingPool.Count + "\n";
            stats += "FrameTime: " + lastframetime + "\n";
            stats += "Outgoing PPS: " + lastestopps + "\n";
            stats += "Incoming PPS: " + latestipps + "\n";
            stats += "Largest Small-Packet Size: " + NetworkActions.SMALL_PACKET_COUNT + "/" +
                     NetworkActions.SMALL_PACKET_MAX_SIZE + "\n";
            stats += "Largest Large-Packet Size: " + NetworkActions.LARGE_PACKET_COUNT + "/" +
                     NetworkActions.LARGE_PACKET_MAX_SIZE + "\n";*/
    }
}
 