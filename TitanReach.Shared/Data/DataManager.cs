
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TRShared.Data.Definitions;

namespace TRShared
{
    public class DataManager
    {

        public static bool IS_SERVER = false;
        public static BuffDef[] BuffDefinitions;
        public static ItemDefinition[] ItemDefinitions;
        public static NpcDef[] NpcDefinitions;
        public static PetDef[] PetDefinitions;
        public static ObjectDef[] ObjectDefinitions;
        public static ProjectileDefinition[] ProjectileDefinitions;
        public static CookingDef[] CookingDefinitions;
        public static LandDef[] LandDefinitions = new LandDef[2];
        public static TeleportLocationDef[] TeleportLocations;
        public static ForgingDef[] ForgingDefinitions;
        public static PickableDef[] PickableDefinitions;
        public static MiningDef[] MiningDefinitions;
        public static WoodcuttingDef[] WoodcuttingDefinitions;
        public static ObjectLocationDef[] ObjectLocations;
        public static ShopDef[] ShopDefinitions;
        public static List<NpcSpawnDef> NpcSpawnDefinitions;
        public static SpellDef[] SpellDefinitions;
        public static FishingDef[] FishingDefinitions;
        public static EatibleDef[] EatibleDefinitions;
        public static DropDef[] DropTables;


        public delegate DataTable GetDataTable(string def, string query);
        public static event GetDataTable OnTable;
        public delegate void UpdateDB(string query, string db);
        public static event UpdateDB UpdateDBEvent;

        public static event EventHandler OnAllDefinitionsLoaded;

        public static void Load()
        {
            LoadDefs();
            GenerateStaticClasses();
        }


        public static void LoadDefs(params string[] only)
        {


            Logger.Log("Loading Defs...");

			MethodInfo[] defMethods = null;
			try
			{
				defMethods = Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(typeof(Definition), false).Length > 0).ToArray();
			} catch(Exception e)
            {
				Logger.Log("First Error ", e);
			}
			if(defMethods == null)
				Logger.Log("No definition methods found");
			else Logger.Log("Found " + defMethods.Length + " Definitions!");
			if (defMethods != null)
            foreach (MethodInfo meth in defMethods)
            {
                try
                {
					
						string defName = meth.GetCustomAttribute<Definition>().Name;
					
						if (only != null && only.Length > 0 && !only.Contains(defName))
						continue;

						Console.WriteLine("About to load " + defName);
						var tab = OnTable(defName, "Select * FROM " + defName);
					
						DTWrapper dt = new DTWrapper(tab);
                    meth.Invoke(null, new object[] { dt });
						
					}
                catch (Exception e)
                {
                    Logger.Log("Error ", e);
                }
            }
				 OnAllDefinitionsLoaded(null, EventArgs.Empty);
        }

        public static void Update(string query, string db) => UpdateDBEvent(query, db);


        public static float GetHeight(int x, int z, int LandID)
        {
            return GetHeight(x, z, LandDefinitions[LandID]);
        }

        public static float GetHeight(int x, int z, LandDef land)
        {
			if (land.LandID == 3) return 100.0f; //dev world is a flat plane at y = 100;

            int mid_point = land.HEIGHT_MAP_RESOLUTION / 2;

            int HEIGHT_MAP_X = mid_point + x;// + (int)((float)x * (float)land.MAP_MESH_SIZE / (float)land.MAP_RESOLUTION);
            int HEIGHT_MAP_Z = mid_point + z;// + (int)((float)z * (float)land.MAP_MESH_SIZE / (float)land.MAP_RESOLUTION);

            if (HEIGHT_MAP_X > land.HEIGHT_MAP.Length || HEIGHT_MAP_X < 0)
            {
                Logger.Log("DataManager.GetHeight falied 1");
                return float.MaxValue;
            }
            if (land.HEIGHT_MAP[HEIGHT_MAP_X] == null)
            {

                Logger.Log("DataManager.GetHeight falied 2 - [HEIGHT_MAP_X] " + HEIGHT_MAP_X + " X: " + x + ", Z: " + z + " LandID: " + land.LandID);
                Logger.Log(Environment.StackTrace);
                return float.MaxValue;
            }
            if (HEIGHT_MAP_Z > land.HEIGHT_MAP[HEIGHT_MAP_X].Length || HEIGHT_MAP_Z < 0)
            {
                Logger.Log("DataManager.GetHeight falied 3");
                return float.MaxValue;
            }

            return land.HEIGHT_MAP[HEIGHT_MAP_X][HEIGHT_MAP_Z];
        }


        public static void GenerateStaticClasses()
        {
            string path = "StaticClass.txt";
            string text = "public class ObjectID { " + Environment.NewLine + Environment.NewLine;
            foreach (var obj in TRShared.DataManager.ObjectDefinitions)
            {
                if (obj == null)
                    continue;
                text += "\t public const int " + obj.Name.Replace(' ', '_').ToUpper().Trim() + " = " + obj.ID + ";" + Environment.NewLine;
            }
            text += Environment.NewLine + "} " + Environment.NewLine;


            text += "public class ItemID { " + Environment.NewLine + Environment.NewLine;
            foreach (var obj in TRShared.DataManager.ItemDefinitions)
            {
                if (obj == null)
                    continue;
                if (obj.IsToken)
                    break;
                text += "\t public const int " + obj.ItemName.Replace(' ', '_').ToUpper().Trim() + " = " + obj.ItemID + ";" + Environment.NewLine;
            }
            text += Environment.NewLine + "} " + Environment.NewLine;
            File.WriteAllText(path, text);
        }

        public static void LoadHeightMap()
        {

            foreach (LandDef def in LandDefinitions)
            {
                int mid_point = def.HEIGHT_MAP_RESOLUTION / 2;
                int offset = def.POSITIVE_CHUNKS ? 0 : -def.MAP_MESH_SIZE / 2;
				string dir = "Assets/";
				if(Directory.Exists(@"..\..\..\..\ServerData"))
					dir = @"..\..\..\..\ServerData\";


				string name = dir + "Terrain/" + def.LandID + "/";
				Logger.Log("Height Map Path: " + name);
				foreach (var fi in System.IO.Directory.GetFiles(name))
                {
                    using (var file = System.IO.File.OpenRead(fi))
                    using (var reader = new System.IO.BinaryReader(file))
                    {

                        var nam = System.IO.Path.GetFileName(fi).Replace(".r16", "");
                        var nem = nam.Split('_');
                        int startX = (int.Parse(nem[0]) * 1024) + offset;
                        int startZ = (int.Parse(nem[1]) * 1024) + offset;
                        int startY = 0;


                        for (int localZ = 0; localZ < def.MAP_RESOLUTION; localZ++)
                        {
                            int globalZ = startZ + localZ;
                            // Console.WriteLine("" + localZ);

                            for (int localX = 0; localX < def.MAP_RESOLUTION; localX++)
                            {
                                int globalX = startX + localX;
                                float localY = (float)(reader.ReadUInt16()) / 0xFFFF;
                                float globalY = startY + localY;

                                if (def.HEIGHT_MAP[mid_point + globalX] == null)
                                    def.HEIGHT_MAP[mid_point + globalX] = new float[def.HEIGHT_MAP_RESOLUTION];

                                def.HEIGHT_MAP[mid_point + globalX][mid_point + globalZ] = (globalY * def.MAP_MESH_HEIGHT) - def.HEIGHTMAP_OFFSET;

							}
                        }
							//Console.In.Read();
						Logger.Log("Heightmap loaded for Terrain Chunk " + startX + ", " + startZ);
                    }
                }
            }
            Logger.Log("Loaded Heightmaps");
        }







		public class ObjectID
		{

			public const int COPPER = 0;
			public const int COPPER_2 = 1;
			public const int COPPER_3 = 2;
			public const int IRON = 3;
			public const int IRON_2 = 4;
			public const int IRON_3 = 5;
			public const int ZINC = 6;
			public const int ZINC_2 = 7;
			public const int ZINC_3 = 8;
			public const int COAL = 9;
			public const int COAL_2 = 10;
			public const int COAL_3 = 11;
			public const int PHOSPHORITE = 12;
			public const int PHOSPHORITE_2 = 13;
			public const int PHOSPHORITE_3 = 14;
			public const int VERTIUM = 15;
			public const int VERTIUM_2 = 16;
			public const int VERTIUM_3 = 17;
			public const int SILVER = 18;
			public const int SILVER_2 = 19;
			public const int SILVER_3 = 20;
			public const int COBALT = 21;
			public const int COBALT_2 = 22;
			public const int COBALT_3 = 23;
			public const int XELNITE = 24;
			public const int XELNITE_2 = 25;
			public const int XELNITE_3 = 26;
			public const int GOLD = 27;
			public const int GOLD_2 = 28;
			public const int GOLD_3 = 29;
			public const int AURORIUM = 30;
			public const int AURORIUM_2 = 31;
			public const int AURORIUM_3 = 32;
			public const int KRONYX = 33;
			public const int KRONYX_2 = 34;
			public const int KRONYX_3 = 35;
			public const int PLATINUM = 36;
			public const int PLATINUM_2 = 37;
			public const int PLATINUM_3 = 38;
			public const int TITANITE = 39;
			public const int TITANITE_2 = 40;
			public const int TITANITE_3 = 41;
			public const int FLAX = 42;
			public const int FLAX_2 = 43;
			public const int FLAX_3 = 44;
			public const int TREE = 45;
			public const int TREE_2 = 46;
			public const int TREE_3 = 47;
			public const int TREE_4 = 48;
			public const int TREE_5 = 49;
			public const int TREE_6 = 50;
			public const int TREE_7 = 51;
			public const int TREE_8 = 52;
			public const int TREE_9 = 53;
			public const int TREE_10 = 54;
			public const int WILLOW = 55;
			public const int WILLOW_2 = 56;
			public const int WILLOW_3 = 57;
			public const int COTTONWOOD_TREE = 58;
			public const int COTTONWOOD_TREE_2 = 59;
			public const int COTTONWOOD_TREE_3 = 60;
			public const int EUCALYPTUS = 61;
			public const int EUCALYPTUS_2 = 62;
			public const int EUCALYPTUS_3 = 63;
			public const int CURVED_ASPEN = 64;
			public const int CURVED_ASPEN_2 = 65;
			public const int CURVED_ASPEN_3 = 66;
			public const int CHERRY_TREE = 67;
			public const int CHERRY_TREE_2 = 68;
			public const int CHERRY_TREE_3 = 69;
			public const int OAK = 70;
			public const int OAK_2 = 71;
			public const int OAK_3 = 72;
			public const int PALM_TREE = 73;
			public const int PALM_TREE_2 = 74;
			public const int PALM_TREE_3 = 75;
			public const int SYCAMORE = 76;
			public const int SYCAMORE_2 = 77;
			public const int SYCAMORE_3 = 78;
			public const int DARKWOOD = 79;
			public const int DARKWOOD_2 = 80;
			public const int DARKWOOD_3 = 81;
			public const int FROSTBARK = 82;
			public const int FROSTBARK_2 = 83;
			public const int FROSTBARK_3 = 84;
			public const int ROYAL_PALM = 85;
			public const int ROYAL_PALM_2 = 86;
			public const int ROYAL_PALM_3 = 87;
			public const int AIDENLEER = 88;
			public const int AIDENLEER_2 = 89;
			public const int AIDENLEER_3 = 90;
			public const int CAULDRON = 91;
			public const int CAULDRON_2 = 92;
			public const int FOUNTAIN = 93;
			public const int FOUNTAIN_2 = 94;
			public const int WELL = 95;
			public const int WELL_2 = 96;
			public const int TANNING_RACK = 97;
			public const int LOOM = 98;
			public const int RANGE = 99;
			public const int STOVE = 100;
			public const int CAMPFIRE = 101;
			public const int CAMPFIRE_2 = 102;
			public const int CAMPFIRE_3 = 103;
			public const int ANVIL = 104;
			public const int ANVIL_2 = 105;
			public const int ANVIL_3 = 106;
			public const int FURNACE = 107;
			public const int FURNACE_2 = 108;
			public const int FORGE = 109;
			public const int FORGE_2 = 110;
			public const int SPINNING_WHEEL = 111;
			public const int SPINNING_WHEEL_2 = 112;
			public const int WORKBENCH = 113;
			public const int WORKBENCH_2 = 114;
			public const int WORKBENCH_3 = 115;
			public const int BANK = 116;
			public const int CHUB = 117;
			public const int PERCH = 118;
			public const int PRAWN = 119;
			public const int HAGNESH = 120;
			public const int YABBY = 121;
			public const int MUSHROOM_RED = 122;
			public const int MUSHROOM_BLUE = 123;
			public const int MUSHROOM_BROWN = 124;
			public const int MUSHROOM_PURPLE_01 = 125;
			public const int MUSHROOM_YELLOW_01 = 126;
			public const int LADDER = 127;
			public const int PORTAL = 128;
			public const int FISHSPOT = 129;
			public const int CURVED_ASPEN_4 = 130;
			public const int WILLOW_4 = 131;
			public const int OAK_4 = 132;

		}
		public class ItemID
		{

			public const int COPPER_ORE = 1;
			public const int COPPER_INGOT = 4;
			public const int IRON_INGOT = 5;
			public const int IRON_ORE = 6;
			public const int STEEL_INGOT = 9;
			public const int VERTIUM_INGOT = 11;
			public const int COBALT_INGOT = 12;
			public const int PLATINUM_INGOT = 13;
			public const int TITANITE_INGOT = 14;
			public const int COINS = 16;
			public const int VERTIUM_ORE = 74;
			public const int COBALT_ORE = 75;
			public const int PLATINUM_ORE = 76;
			public const int TITANITE_ORE = 77;
			public const int COAL = 78;
			public const int FLINT = 79;
			public const int HAMMER = 130;
			public const int FISHING_ROD = 160;
			public const int YABBY_CAGE = 161;
			public const int RAW_YABBY = 162;
			public const int YABBY = 163;
			public const int BURNT_YABBY = 164;
			public const int RAW_PERCH = 165;
			public const int PERCH = 166;
			public const int BURNT_PERCH = 167;
			public const int RAW_CHUB = 168;
			public const int CHUB = 169;
			public const int BURNT_CHUB = 170;
			public const int WORMS = 171;
			public const int BLUE_MUSHROOM = 173;
			public const int BROWN_MUSHROOM = 174;
			public const int PURPLE_MUSHROOM = 175;
			public const int RED_MUSHROOM = 176;
			public const int YELLOW_MUSHROOM = 177;
			public const int APPLE = 178;
			public const int GARLIC = 180;
			public const int POTATO = 181;
			public const int REED = 182;
			public const int TOMATO = 183;
			public const int FLAX = 184;
			public const int EMPTY_POT = 191;
			public const int POT_OF_WATER = 192;
			public const int EMPTY_BOWL = 193;
			public const int BOWL_OF_WATER = 194;
			public const int EMPTY_BUCKET = 195;
			public const int BUCKET_OF_WATER = 196;
			public const int EMPTY_JUG = 197;
			public const int JUG_OF_WATER = 198;
			public const int COPPER_ARROW = 201;
			public const int IRON_ARROW = 202;
			public const int STEEL_ARROW = 203;
			public const int VERTIUM_ARROW = 204;
			public const int COBALT_ARROW = 205;
			public const int PLATINUM_ARROW = 206;
			public const int TITANITE_ARROW = 207;
			public const int COPPER_ARROWHEAD = 208;
			public const int IRON_ARROWHEAD = 209;
			public const int STEEL_ARROWHEAD = 210;
			public const int VERTIUM_ARROWHEAD = 211;
			public const int COBALT_ARROWHEAD = 212;
			public const int PLATINUM_ARROWHEAD = 213;
			public const int TITANITE_ARROWHEAD = 214;
			public const int BOW_STRING = 215;
			public const int FEATHER = 216;
			public const int HEADLESS_ARROW = 217;
			public const int ARROW_SHAFT = 218;
			public const int SAW = 219;
			public const int LONGBOW = 221;
			public const int OAK_LONGBOW = 222;
			public const int ASPEN_LONGBOW = 223;
			public const int WILLOW_LONGBOW = 224;
			public const int CHERRY_LONGBOW = 225;
			public const int LONGBOW_UNSTRUNG = 226;
			public const int OAK_LONGBOW_UNSTRUNG = 227;
			public const int ASPEN_LONGBOW_UNSTRUNG = 228;
			public const int WILLOW_LONGBOW_UNSTRUNG = 229;
			public const int CHERRY_LONGBOW_UNSTRUNG = 230;
			public const int SHORTBOW = 231;
			public const int OAK_SHORTBOW = 232;
			public const int ASPEN_SHORTBOW = 233;
			public const int WILLOW_SHORTBOW = 234;
			public const int CHERRY_SHORTBOW = 235;
			public const int SHORTBOW_UNSTRUNG = 236;
			public const int OAK_SHORTBOW_UNSTRUNG = 237;
			public const int ASPEN_SHORTBOW_UNSTRUNG = 238;
			public const int WILLOW_SHORTBOW_UNSTRUNG = 239;
			public const int CHERRY_SHORTBOW_UNSTRUNG = 240;
			public const int EMERALD = 241;
			public const int SAPPHIRE = 242;
			public const int RUBY = 243;
			public const int DIAMOND = 244;
			public const int AMETHYST = 245;
			public const int PEARL = 246;
			public const int AMBER = 247;
			public const int TOPAZ = 248;
			public const int LOGS = 249;
			public const int OAK_LOGS = 250;
			public const int ASPEN_LOGS = 251;
			public const int WILLOW_LOGS = 252;
			public const int CHERRY_LOGS = 253;
			public const int BOWL_OF_MILK = 254;
			public const int JUG_OF_MILK = 255;
			public const int POT_OF_FLOUR = 256;
			public const int BUCKET_OF_MILK = 257;
			public const int JUG_OF_WINE = 258;
			public const int RAW_WOLF_MEAT = 344;
			public const int WOLF_MEAT = 345;
			public const int BURNT_WOLF_MEAT = 346;
			public const int RAW_CHICKEN = 347;
			public const int CHICKEN = 348;
			public const int BURNT_CHICKEN = 349;
			public const int RAW_HAM = 350;
			public const int HAM = 351;
			public const int BURNT_HAM = 352;
			public const int RAW_STEAK = 353;
			public const int STEAK = 354;
			public const int BURNT_STEAK = 355;
			public const int LEATHER_ROLL = 366;
			public const int WOLF_LEATHER = 367;
			public const int COWHIDE = 371;
			public const int WOLF_PELT = 372;
			public const int THREAD = 376;
			public const int NEEDLE = 377;
			public const int VIAL = 378;
			public const int VIAL_OF_WATER = 379;
			public const int UNFINISHED_REGENERATION_POTION = 380;
			public const int REGENERATION_POTION = 381;
			public const int REGENERATION_POTION_3_DOSE = 382;
			public const int REGENERATION_POTION_2_DOSE = 383;
			public const int REGENERATION_POTION_1_DOSE = 384;
			public const int UNFINISHED_MINOR_HEALTH_POTION = 385;
			public const int MINOR_HEALTH_POTION = 386;
			public const int MINOR_HEALTH_POTION_3_DOSE = 387;
			public const int MINOR_HEALTH_POTION_2_DOSE = 388;
			public const int MINOR_HEALTH_POTION_1_DOSE = 389;
			public const int BEARSKIN_COWL = 390;
			public const int BEARSKIN_CHEST = 391;
			public const int BEARSKIN_LEGS = 392;
			public const int BEARSKIN_GLOVES = 393;
			public const int BEARSKIN_BOOTS = 394;
			public const int COPPER_AXE = 405;
			public const int COPPER_BATTLEAXE = 406;
			public const int COPPER_DAGGER = 407;
			public const int COPPER_GREATSWORD = 408;
			public const int COPPER_PICKAXE = 409;
			public const int COPPER_SWORD = 410;
			public const int IRON_AXE = 411;
			public const int IRON_BATTLEAXE = 412;
			public const int IRON_DAGGER = 413;
			public const int IRON_GREATSWORD = 414;
			public const int IRON_PICKAXE = 415;
			public const int IRON_SWORD = 416;
			public const int STEEL_AXE = 417;
			public const int STEEL_BATTLEAXE = 418;
			public const int STEEL_DAGGER = 419;
			public const int STEEL_GREATSWORD = 420;
			public const int STEEL_PICKAXE = 421;
			public const int STEEL_SWORD = 422;
			public const int COPPER_HELMET = 423;
			public const int COPPER_LEGS = 424;
			public const int COPPER_GLOVES = 425;
			public const int COPPER_BOOTS = 426;
			public const int IRON_HELMET = 427;
			public const int IRON_CHEST = 428;
			public const int IRON_LEGS = 429;
			public const int IRON_GLOVES = 430;
			public const int IRON_BOOTS = 431;
			public const int STEEL_HELMET = 432;
			public const int STEEL_CHEST = 433;
			public const int STEEL_LEGS = 434;
			public const int STEEL_GLOVES = 435;
			public const int STEEL_BOOTS = 436;
			public const int COPPER_CHEST = 442;
			public const int AIR_STAFF_1 = 443;
			public const int EARTH_STAFF_1 = 444;
			public const int WATER_STAFF_1 = 445;
			public const int FIRE_STAFF_1 = 446;
			public const int AIR_STAFF_2 = 447;
			public const int EARTH_STAFF_2 = 448;
			public const int WATER_STAFF_2 = 449;
			public const int FIRE_STAFF_2 = 450;
			public const int AIR_STAFF_3 = 451;
			public const int EARTH_STAFF_3 = 452;
			public const int WATER_STAFF_3 = 453;
			public const int FIRE_STAFF_3 = 454;
			public const int LEATHER_COWL = 455;
			public const int LEATHER_CHEST = 456;
			public const int LEATHER_LEGS = 457;
			public const int LEATHER_GLOVES = 458;
			public const int LEATHER_BOOTS = 459;
			public const int WOLFPELT_COWL = 460;
			public const int WOLFPELT_CHEST = 461;
			public const int WOLFPELT_LEGS = 462;
			public const int WOLFPELT_GLOVES = 463;
			public const int WOLFPELT_BOOTS = 464;
			public const int BONES = 465;
			public const int SPELLBOUND_HAT = 466;
			public const int SPELLBOUND_ROBETOP = 467;
			public const int SPELLBOUND_ROBEBOTTOM = 468;
			public const int SPELLBOUND_GLOVES = 469;
			public const int SPELLBOUND_BOOTS = 470;
			public const int DIABOLICAL_HAT = 471;
			public const int DIABOLICAL_ROBETOP = 472;
			public const int DIABOLICAL_ROBEBOTTOM = 473;
			public const int DIABOLICAL_GLOVES = 474;
			public const int DIABOLICAL_BOOTS = 475;
			public const int ENSORCELL_HAT = 476;
			public const int ENSORCELL_ROBETOP = 477;
			public const int ENSORCELL_ROBEBOTTOM = 478;
			public const int ENSORCELL_GLOVES = 479;
			public const int ENSORCELL_BOOTS = 480;
			public const int BEAR_HIDE = 483;
			public const int BEAR_LEATHER = 484;
			public const int DEER_HIDE = 485;
			public const int YABBY_SPEAR = 486;
			public const int FISH_SPEAR = 487;
			public const int TEST_RING = 488;
			public const int TEST_AMULET = 489;
			public const int ATTACK_AMULET = 490;
			public const int DEFENCE_AMULET = 491;
			public const int HEALTH_AMULET = 492;
			public const int MAGIC_AMULET = 493;
			public const int RANGED_AMULET = 494;
			public const int WOODEN_SHIELD = 495;
			public const int EMERALD_AMULET = 496;
			public const int RUBY_AMULET = 497;
			public const int SAPPHIRE_AMULET = 498;
			public const int EMERALD_RING = 499;
			public const int RUBY_RING = 500;
			public const int SAPPHIRE_RING = 501;
			public const int UNFINISHED_ATTACK_POTION = 502;
			public const int ATTACK_POTION = 503;
			public const int ATTACK_POTION_3_DOSE = 504;
			public const int ATTACK_POTION_2_DOSE = 505;
			public const int ATTACK_POTION_1_DOSE = 506;
			public const int UNFINISHED_STRENGTH_POTION = 507;
			public const int STRENGTH_POTION = 508;
			public const int STRENGTH_POTION_3_DOSE = 509;
			public const int STRENGTH_POTION_2_DOSE = 510;
			public const int STRENGTH_POTION_1_DOSE = 511;
			public const int UNFINISHED_DEFENCE_POTION = 512;
			public const int DEFENCE_POTION = 513;
			public const int DEFENCE_POTION_3_DOSE = 514;
			public const int DEFENCE_POTION_2_DOSE = 515;
			public const int DEFENCE_POTION_1_DOSE = 516;
			public const int UNFINISHED_RANGING_POTION = 517;
			public const int RANGING_POTION = 518;
			public const int RANGING_POTION_3_DOSE = 519;
			public const int RANGING_POTION_2_DOSE = 520;
			public const int RANGING_POTION_1_DOSE = 521;
			public const int UNFINISHED_SUPER_STRENGTH_POTION = 522;
			public const int SUPER_STRENGTH_POTION = 523;
			public const int SUPER_STRENGTH_POTION_3_DOSE = 524;
			public const int SUPER_STRENGTH_POTION_2_DOSE = 525;
			public const int SUPER_STRENGTH_POTION_1_DOSE = 526;
			public const int UNFINISHED_GREATER_HEALTH_POTION = 527;
			public const int GREATER_HEALTH_POTION = 528;
			public const int GREATER_HEALTH_POTION_3_DOSE = 529;
			public const int GREATER_HEALTH_POTION_2_DOSE = 530;
			public const int GREATER_HEALTH_POTION_1_DOSE = 531;
			public const int RAW_HAGNESH = 532;
			public const int HAGNESH = 533;
			public const int BURNT_HAGNESH = 534;
			public const int SPEAR = 535;
			public const int COBALT_SWORD = 536;
			public const int RAW_PRAWN = 537;
			public const int PRAWN = 538;
			public const int BURNT_PRAWN = 539;
			public const int DARKSTEEL_HELMET = 540;
			public const int DARKSTEEL_CHEST = 541;
			public const int DARKSTEEL_LEGS = 542;
			public const int DARKSTEEL_GLOVES = 543;
			public const int DARKSTEEL_BOOTS = 544;
			public const int VERTIUM_HELMET = 545;
			public const int VERTIUM_CHEST = 546;
			public const int VERTIUM_LEGS = 547;
			public const int VERTIUM_GLOVES = 548;
			public const int VERTIUM_BOOTS = 549;
			public const int COBALT_HELMET = 550;
			public const int COBALT_CHEST = 551;
			public const int COBALT_LEGS = 552;
			public const int COBALT_GLOVES = 553;
			public const int COBALT_BOOTS = 554;
			public const int DARKSTEEL_FRAGMENT = 555;
			public const int DARKSTEEL_INGOT = 556;
			public const int COBALT_AXE = 557;
			public const int COBALT_BATTLEAXE = 558;
			public const int COBALT_DAGGER = 559;
			public const int COBALT_GREATSWORD = 560;
			public const int COBALT_PICKAXE = 561;
			public const int VERTIUM_AXE = 562;
			public const int VERTIUM_BATTLEAXE = 563;
			public const int VERTIUM__DAGGER = 564;
			public const int VERTIUM_GREATSWORD = 565;
			public const int VERTIUM_PICKAXE = 566;
			public const int VERTIUM_SWORD = 568;
			public const int DARKSTEEL_AXE = 569;
			public const int DARKSTEEL_BATTLEAXE = 570;
			public const int DARKSTEEL_DAGGER = 571;
			public const int DARKSTEEL_GREATSWORD = 572;
			public const int DARKSTEEL_PICKAXE = 573;
			public const int DARKSTEEL_SWORD = 574;
			public const int WOLFBANE_HOOD = 575;
			public const int WOLFBANE_CHEST = 576;
			public const int WOLFBANE_LEGS = 577;
			public const int WOLFBANE_GLOVES = 578;
			public const int WOLFBANE_BOOTS = 579;
			public const int WOLFBANE_CAPE = 580;
			public const int DIAMOND_AMULET = 581;
			public const int DIAMOND_RING = 582;

		}

	}
}
