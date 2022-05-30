using System;
using System.Collections.Generic;
using System.Text;
using TitanReach_Server.Model;
using System.Linq;

namespace TitanReach_Server.Skills
{
    class Slayer
    {
        private static Random _random;


        private static int[] CatergoryChicken = new int[3] { 1, 2, 3};
        private static int[] CatergoryCow = new int[5] { 4, 5, 6, 7, 8};
        private static int[] CatergoryWolf = new int[4] {9, 10, 11, 12};
        private static int[] CatergoryBear = new int[4] { 13, 14, 15, 16 };
        private static int[] CatergoryStag = new int[3] { 17, 18, 19 };
        private static int[] CatergoryDoe = new int[3] { 20, 21, 22};
        private static int[] CatergoryRabbit = new int[3] { 23, 24, 25};
        private static int[] CatergorySpider = new int[4] { 26, 27, 28, 29};
        private static int[] CatergoryTaurboro = new int[3] { 30, 31, 32 };
        private static int[] CatergoryScorpion = new int[6] { 33,34,35,36,37,38 };
        private static int[] CatergoryGargoyl = new int[4] { 39,40,41,42};
        private static int[] CatergoryBat = new int[4] { 43, 44, 45, 46 };
        private static int[] CatergoryZimpala = new int[4] { 51,52,53,54 };
        private static int[] CatergoryCrab = new int[4] { 55, 56, 57, 58 };
        private static int[] CatergorySkeleton = new int[8] { 59, 60, 61, 62, 63, 64, 65, 66};
        private static int[] CatergoryZombie = new int[20] { 67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86};
        private static int[] CatergoryRat = new int[4] { 87, 88, 89, 90};


        private static Dictionary<int, int[]> NpcCategories = new Dictionary<int, int[]>()
        {
            {0, CatergoryChicken },
            {1, CatergoryCow },
            {2, CatergoryWolf },
            {3, CatergoryBear },
            {4, CatergoryStag },
            {5, CatergoryDoe },
            {6, CatergoryRabbit },
            {7, CatergorySpider },
            {8, CatergoryTaurboro },
            {9, CatergoryScorpion },
            {10, CatergoryGargoyl },
            {11, CatergoryBat },
            {12, CatergoryZimpala },
            {13, CatergoryCrab },
            {14, CatergorySkeleton },
            {15, CatergoryZombie },
            {16, CatergoryRat },

        };

        //int npcCategory, int minCount, int maxCount, int xpPerKill, int xpForCompletion, int levelReq)
        public static readonly SlayerTaskTemplate[] SlayerTaskTemplates = {
            new SlayerTaskTemplate(0, 4, 8, 30, 100, 1, "Chickens"), 
            new SlayerTaskTemplate(6, 5, 10, 30, 100, 2, "Rabbits"), 
            new SlayerTaskTemplate(1, 5, 10, 40, 100, 4, "Cows"), 
            new SlayerTaskTemplate(16, 10, 15, 30, 100, 8, "Rats"), 
            new SlayerTaskTemplate(5, 4, 9, 40, 100, 12, "Deer"), //doe
            new SlayerTaskTemplate(2, 10, 20, 50, 200, 16, "Wolves"), //wolf
            new SlayerTaskTemplate(4, 5, 15, 75, 100, 20, "Stags"), //stag
            new SlayerTaskTemplate(13, 10, 12, 50, 200, 20, "Crabs"), //crab
            new SlayerTaskTemplate(7, 5, 20, 60, 400, 25, "Spiders"), //spider
            new SlayerTaskTemplate(9, 8, 13, 60, 450, 30, "Scorpions"), //Scorpion
            new SlayerTaskTemplate(15, 6, 20, 50, 200, 30, "Zombies"), //zombie
            new SlayerTaskTemplate(14, 6, 20, 50, 200, 30, "Skleteons"), //skeleton
            new SlayerTaskTemplate(3, 10, 20, 75, 560, 35, "Bears"), //bear
            new SlayerTaskTemplate(10, 10, 20, 100, 1000, 45, "Gargoyls"), //gargoyl
            //new SlayerTaskTemplate(8, 10, 20, 30, 100, 45 "Taurboro"), //Taurboro
            //new SlayerTaskTemplate(12, 10, 20, 30, 100, 30 "Zimpala"), //zimpala
            //new SlayerTaskTemplate(11, 10, 20, 30, 100, 11, "Bats"), //bat

        };

        public class SlayerTask
        {
            public int _npcCategory;
            public int _totalAmount;
            public int _currentAmount;
            public int _xpPerKill;
            public int _xpForCompletion;
            public string _name;

            public SlayerTask(SlayerTaskTemplate template)
            {
                _npcCategory = template._npcCategory;
                _totalAmount = GenerateTaskAmount(template);
                _currentAmount = _totalAmount;
                _xpPerKill = template._xpPerKill;
                _xpPerKill = template._xpForCompletion;
                _name = template._name;
            }

            public SlayerTask(SlayerTaskTemplate template, int killsRequired, int killsComplete)
            {
                _npcCategory = template._npcCategory;
                _totalAmount = killsRequired;
                _currentAmount = _totalAmount - killsComplete;
                _xpPerKill = template._xpPerKill;
                _xpPerKill = template._xpForCompletion;
                _name = template._name;
            }
            private int GenerateTaskAmount(SlayerTaskTemplate slayerTask)
            {
                return ((int) (_random.NextDouble() * (slayerTask._maxCount - slayerTask._minCount + 1))) + slayerTask._minCount;
            }

            public string Warning()
            {
                string returnString = "Please note this current implementation of slayer is a fraction of what we plan it to be!";
                return returnString;
            }

            public string CurrentTaskInfo()
            {
                string returnString = "You have killed " + (_totalAmount-_currentAmount) + " of " + _totalAmount + " " + _name;
                return returnString;
            }

            public string NewTaskInfo()
            {
                string returnString = "Your new task is to kill " + _totalAmount + " " + _name;
                return returnString;
            }

            public string CompleteTaskInfo()
            {
                string returnString = "Congratulations, you have completed your slayer task";
                return returnString;
            }

            public static string NoTaskInfo()
            {
                string returnString = "You do not currently have a task!";
                return returnString;
            }

            public static string AlreadyTaskInfor()
            {
                string returnString = "You already have a task!";
                return returnString;
            }

            public static string CancelTaskInfo()
            {
                string returnString = "You have canceled your task!";
                return returnString;
            }

            public void EnemyKilledTrigger(Object sender, int npcId)
            {
                Player player = (Player)sender;
                if(NpcCategories[_npcCategory].Contains(npcId))
                {
                    player.Skills.AddExp((int)Stats.SKILLS.Slayer, _xpPerKill);
                    _currentAmount -= 1;
                    player.TriggerSlayerDatabaseUpdate();
                    if(_currentAmount == 0)
                    {
                        player.Skills.AddExp((int)Stats.SKILLS.Slayer, _xpForCompletion);
                        player.CompleteSlayerTask();
                    }
                }
            }
        }

        public struct SlayerTaskTemplate
        {
            public int _npcCategory;
            public int _minCount;
            public int _maxCount;
            public int _xpPerKill;
            public int _xpForCompletion;
            public int _levelReq;
            public string _name;

            public SlayerTaskTemplate(int npcCategory, int minCount, int maxCount, int xpPerKill, int xpForCompletion, int levelReq, string name)
            {
                _npcCategory = npcCategory;
                _minCount = minCount;
                _maxCount = maxCount;
                _xpPerKill = xpPerKill;
                _xpForCompletion = xpForCompletion;
                _levelReq = levelReq;
                _name = name;
            }
        }

        static Slayer()
        {
            _random = new Random();
        }

        public static SlayerTask GenerateSlayerTask(Player player)
        {
            List<SlayerTaskTemplate> avaliableSlayerTasks = GenerateAvaliableSlayerTasks(player);

            int randomSelection = (int) (_random.NextDouble() * avaliableSlayerTasks.Count);
            SlayerTaskTemplate slayerTaskTemplate = avaliableSlayerTasks[randomSelection];

            return new SlayerTask(slayerTaskTemplate);
        }
        
        public static SlayerTask GenerateCurrentSlayerTask(int category, int killsRequired, int killscomplete)
        {
            SlayerTaskTemplate slayerTemplate = SlayerTaskTemplates[0];

            foreach(SlayerTaskTemplate template in SlayerTaskTemplates)
            {
                if(template._npcCategory == category)
                {
                    slayerTemplate = template;
                }
            }

            return new SlayerTask(slayerTemplate, killsRequired, killscomplete);
        }

        private static List<SlayerTaskTemplate> GenerateAvaliableSlayerTasks(Player player)
        {
            List<SlayerTaskTemplate> avaliableSlayerTasks = new List<SlayerTaskTemplate>();

            foreach (SlayerTaskTemplate task in SlayerTaskTemplates)
            {
                if (task._levelReq <= player.Skills.GetCurLevel((int)Stats.SKILLS.Slayer))
                {
                    avaliableSlayerTasks.Add(task);
                }
            }
            return avaliableSlayerTasks;
        }
    }
}
