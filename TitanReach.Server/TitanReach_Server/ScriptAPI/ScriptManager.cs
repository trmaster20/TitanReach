using SQLitePCL;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using TitanReach_Server.Model;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TitanReach_Server.ScriptAPI
{
    public class ScriptManager
    {

        public static string SCRIPT_PATH;
        List<RawScript> RawScripts;
        List<CompiledScript> Scripts;
        FileSystemWatcher Watcher;
        public static int LastFSChangeDetected = Environment.TickCount;

        public Dictionary<int, Func<Npc, Player, int, CancellationToken, Task<int>>> ACTION_ChatNpc = new Dictionary<int, Func<Npc, Player, int, CancellationToken, Task<int>>>();
        public Dictionary<int, List<NPCQuestRegister>> ACTION_QuestNpc = new Dictionary<int, List<NPCQuestRegister>>();
        public Dictionary<int, List<NPCShopRegister>> ACTION_ShopNpc = new Dictionary<int, List<NPCShopRegister>>();
        public Dictionary<int, NPCBankRegister> ACTION_BankNpc = new Dictionary<int, NPCBankRegister>();
        public Dictionary<int, Action<Player>> ACTION_OnPlayerLogin = new Dictionary<int, Action<Player>>();

        public void Init()
        {
            if(Server.SERVER_LOCAL)
                SCRIPT_PATH = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Scripts\"));
            else
                SCRIPT_PATH = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"Scripts/"));

            Server.Log("SCRIPT_PATH: " + SCRIPT_PATH);
            InitWatcher();
            ReloadScripts();
            CompileScripts();
            LaunchScripts();
        }

        /// <summary>
        /// Runs the scripts once to register all listeners
        /// </summary>
        public void LaunchScripts()
        {
            foreach(var scr in Scripts)
            {
                try {
                    scr.Script.RunAsync(new GlobalAPI(scr));
                } catch (Exception e){
                    Server.Log(e.Message + ":\n" + e.StackTrace);
                }
            }
        }
    

        /// <summary>
        /// Load/Reload all scripts from disk to memory, ready to be compiled
        /// </summary>
        public void ReloadScripts()
        {
            ACTION_ChatNpc.Clear();
            ACTION_QuestNpc.Clear();
            ACTION_ShopNpc.Clear();
            ACTION_BankNpc.Clear();

            var sw = Stopwatch.StartNew();
            RawScripts = new List<RawScript>();
            AddScriptsRecursive(SCRIPT_PATH);
            Scripts = new List<CompiledScript>();
            if (RawScripts.Count > 0)
            {
                foreach(RawScript scr in RawScripts)
                {

                    CompiledScript cs = new CompiledScript();

                    cs.Script = CSharpScript.Create(scr.Code, Support(scr), typeof(GlobalAPI));
                    cs.Name = scr.Name;
                    Scripts.Add(cs);
                }
            }
            else Server.Error("No scripts found in " + SCRIPT_PATH);
            sw.Stop();
            Server.Log("Reloaded Scripts - Took " + sw.ElapsedMilliseconds + " ms");

        }


        /// <summary>
        /// Provide support to the script in the form of imports, references or instances
        /// </summary>
        /// <param name="raw">the raw script (used for the name/path)</param>
        /// <returns></returns>
        public static ScriptOptions Support(RawScript raw)
        {
            ScriptOptions so;

            /**
             * ============================ GLOBAL IMPORTS ============================ 
             * */

            so = ScriptOptions.Default.WithReferences(typeof(GlobalAPI).Assembly).WithImports(new string[] { "System", "TitanReach_Server.Model", "TitanReach_Server", "TRShared" });


            /**
             * ============================ CUSTOM IMPORTS ============================ 
             * */

            // for example, if you were targetting only NPC scripts you may provide them with different imports or globals

            return so;
        }
  

        /// <summary>
        /// Compiles all loaded scripts using the Roslyn compiler
        /// </summary>
        public void CompileScripts()
        {
            if (Scripts.Count > 0)
            {
                var sw = Stopwatch.StartNew();
                foreach (CompiledScript cs in Scripts)
                {
                    var errors = cs.Script.Compile();
                    if (errors.Length > 0)
                    {
                        Server.Error("Error compiling script " + cs.Name + ":");
                        foreach (var err in errors)
                        {
                            Server.Error(err.ToString());
                        }
                        cs.Error = true;
                        continue;
                    }
                    cs.Compiled = true;
                }
                sw.Stop();
                Server.Log("Compiled Scripts - Took " + sw.ElapsedMilliseconds + " ms");
            }
        }

        /// <summary>
        /// Creates a listener for changed scripts in the folder
        /// </summary>
        public void InitWatcher()
        {
            Watcher = new FileSystemWatcher();
            Watcher.Path = SCRIPT_PATH;
            Watcher.Changed += OnScriptUpdated;
            Watcher.Created += OnScriptUpdated;
            Watcher.Deleted += OnScriptUpdated;
            Watcher.Renamed += OnScriptUpdated;
            Watcher.EnableRaisingEvents = true;
        }

        //todo: Make this reload/recompile only the changed script and not them all
        /// <summary>
        /// Reloads & Compiles scripts when one changes.
        /// </summary>
        void OnScriptUpdated(object source, FileSystemEventArgs e)
        {
            if (Environment.TickCount - LastFSChangeDetected > 200)
            {
                // todo: have this notify our main game thread to do the loading and we won't need the sleep.
                Thread.Sleep(50); // Without this, race conditions happen (the file does not exist sometimes).
                Server.Log("Script Change Detected.. Reloading");
                ReloadScripts();
                CompileScripts();
                LastFSChangeDetected = Environment.TickCount;
                LaunchScripts();

                foreach (Player p in Server.Instance.AllPlayers) {
                    foreach (Action<Player> act in ACTION_OnPlayerLogin.Values) {
                        act(p);
                    }
                }
            }
        }

        /// <summary>
        /// Will loop through all scripts in the scripts folder recursively and load their raw code into the server
        /// </summary>
        /// <param name="path">the scripts folder</param>
        public void AddScriptsRecursive(string path)
        {
            string[] files = Directory.GetFiles(path, "*.csx");
            foreach (string file in files)
            {
                RawScript raw = new RawScript {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Code = File.ReadAllText(file)
                };
                RawScripts.Add(raw);
            }

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
                AddScriptsRecursive(dir);
        }


        public struct RawScript
        {
            public string Name;
            public string Code;
        }

        public class CompiledScript
        {
            public string Name;
            public Script Script;
            public bool Compiled = false;
            public bool Error = false;
        }
    }
}
