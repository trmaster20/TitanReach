using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using TitanReach_Server.Quests;
using TRShared;
using static TitanReach_Server.Database;

namespace TitanReach_Server
{
    public class QuestManager {

        public Player player;
        public int ID;

        public QuestManager(Player p) {
            for (int i = 0; i < questStates.Length; i++)
                questStates[i] = new QuestState();

            player = p;
        }

        private const int QUEST_COUNT = 200;

        public static Quest [] quests = new Quest [QUEST_COUNT];

        public QuestState[] questStates = new QuestState[QUEST_COUNT];

        public string IndicatorString = "";
        public string QuestInfoString = "";

        public static void AddQuest(int questID, string name, int completedState, Dictionary<int, QuestInfo> questInfo) {
            quests[questID] = new Quest(questID, name, questInfo, completedState);
        }

        public void LoadQuestProgress(List<QuestData> questDatas) {
            foreach(QuestData questData in questDatas) {

                QuestState questState = questStates[questData.questId];
                QuestProgressData progressData = questData.progress[0];
                if (progressData.questProgressId == null)
                    return;

                ID = (int) progressData.questProgressId;
                questState.ForceState((int) progressData.currentState);
                questState.LoadSteps(progressData.steps);
            }
        }

        public int GetQuestState(int questID) {
            return questStates[questID].Number;
        }

        public string GetQuestName(int questID) {
            return quests[questID].Name;
        }

        public void AddStep(int questID, int nextStep, string text) {
            if(nextStep <= questStates[questID].Number) {
                return;
            }
            questStates[questID].AddStep(nextStep, text);
            UpdateQuestInfo(questID, questStates[questID]);
        }

        public void CompleteStep(int questID, int newState, int step) {
            QuestState state = questStates[questID];

            if (!state.Options.ContainsKey(newState)) {
                Server.Error("Quest Error, invalid state: " + player.Name + ": step [" + step + "] state [" + newState + "]");
                return;
            }

            state.CompleteStep(step, newState, player);

            if (Completed(questID)) {
                player.NetworkActions.PlaySound("quest_completed");
                player.NetworkActions.CompleteQuest(questID);
            }

            UpdateQuestInfo(questID, state);
        }

        public void UncompleteStep(int questID, int nextState, int step) {
            questStates[questID].UnCompleteStep(nextState, step);
        }

        public bool IsStarted(int questID) {
            return questStates[questID].Number > 0;
        }

        public bool Completed(int questID) {
            return questStates[questID].Number >= quests[questID].CompletedState;
        }

        private void UpdateQuestInfo(int questID, QuestState state) {
            player.NetworkActions.UpdateQuestInfo(quests[questID], GetQuestInfo(questID), state);
        }

        public void InitialiseQuestInfo(Quest q) {
            int questID = q.ID;
            QuestState state = questStates[questID];
            player.NetworkActions.UpdateQuest(q, GetQuestInfo(questID), state, Completed(questID));
        }

        public void InitialiseAllQuestInfo() {
            foreach(Quest q in quests) {
                if (q != null) {
                    InitialiseQuestInfo(q);
                }
            }
        }

        public QuestInfo GetQuestInfo(int questID) {
            return GetQuestInfo(questID, questStates[questID].Number);
        }

        public QuestInfo GetQuestInfo(int questID, int state) {
            if (quests[questID] != null) {
                return quests[questID].GetQuestInfo(state);
            }

            return null;
        }

        public static string Npc(int id, string extension) {
            return "<color=yellow>" + DataManager.NpcDefinitions[id].Name + extension + "</color>";
        }

        public static string Item(int id, string extension) {
            return "<color=#08E1EC>[" + DataManager.ItemDefinitions[id].ItemName + extension + "]</color>";
        }

        #region oldCode
        /*
        public int[][] QuestStates = new int[1000][];

        public List<Indicator> Indicators = new List<Indicator>();

        public Dictionary<int, QuestInfo[]> QuestInformation = new Dictionary<int, QuestInfo[]>();

        public Quest[] Quests = new Quest[100];

        public static int STAGE = 1;
        public static int STEP = 2;

        public void StartQuest(int id, string name)
        {
            Quest q = new Quest();
            q.Name = name;
            q.QuestID = id;
            q.Completed = false;
            Quests[id] = q;
            player.NetworkActions.UpdateQuestName(true, q);
        }

        public int GetQuestState(int questID, int stateID)
        {
            if (QuestStates[questID] == null)
                return -1;
            if (QuestStates[questID][stateID] != -1)
            {
                return QuestStates[questID][stateID];
            }
            return -1;
        }

        public bool HasStartedQuest(int questID)
        {
            return QuestStates[questID] != null;
        }

        public void SetQuestState(int questID, int stateID, int val)
        {
            if (!HasStartedQuest(questID))
            {
                QuestStates[questID] = new int[100];
            }
            QuestStates[questID][stateID] = val;
        }

        public void RestoreIndicator(string res)
        {
            var arr = res.Split('|');
            if (arr.Length > 1)
            {
                if (arr[0] == "NPC")
                {
                    int npc = int.Parse(arr[1]);
                    Server.Log("sending indicator");
                    SetIndicator(TitanReach_Server.Quests.THE_FORGOTTEN.QUEST_ID, IndicatorIcon.DOWN_ARROW, IndicatorType.NPC, npc, false);

                }
                else if (arr[0] == "POS")
                {
                    float x = float.Parse(arr[1]);
                    float y = float.Parse(arr[2]);
                    float z = float.Parse(arr[3]);
                    SetIndicator(TitanReach_Server.Quests.THE_FORGOTTEN.QUEST_ID, IndicatorIcon.DOWN_ARROW, IndicatorType.POSITION, x, y, z, false);
                }
            }
        }

        public void SetIndicator(int QuestID, IndicatorIcon icon, IndicatorType type, int ID, bool removeLast = false)
        {

            if (player.LastIndicator != null && removeLast)
            {
                if (player.LastIndicator.Type == IndicatorType.NPC)
                {
                    RemoveIndicator(QuestID, player.LastIndicator.IconType, player.LastIndicator.Type, player.LastIndicator.ObjOrNpcID);
                }
                else if (player.LastIndicator.Type == IndicatorType.POSITION)
                {
                    RemoveIndicator(QuestID, player.LastIndicator.IconType, player.LastIndicator.Type, player.LastIndicator.x, player.LastIndicator.y, player.LastIndicator.z);
                }
            }
            IndicatorString = "NPC|" + ID;
            Indicator ourIndi = new Indicator();
            ourIndi.IconType = icon;
            ourIndi.Type = type;
            ourIndi.ObjOrNpcID = ID;
            ourIndi.QuestID = QuestID;
            Indicators.Add(ourIndi);
            player.LastIndicator = ourIndi;
            player.NetworkActions.UpdateIndicator(true, ourIndi);

        }
        public string IndicatorString = "";
        public string ObjectiveString = "";
        public void SetIndicator(int QuestID, IndicatorIcon icon, IndicatorType type, float x, float y, float z, bool removeLast = false)
        {

            if (player.LastIndicator != null && removeLast)
            {
                if (player.LastIndicator.Type == IndicatorType.NPC)
                {
                    RemoveIndicator(QuestID, player.LastIndicator.IconType, player.LastIndicator.Type, player.LastIndicator.ObjOrNpcID);
                }
                else if (player.LastIndicator.Type == IndicatorType.POSITION)
                {
                    RemoveIndicator(QuestID, player.LastIndicator.IconType, player.LastIndicator.Type, player.LastIndicator.x, player.LastIndicator.y, player.LastIndicator.z);
                }
            }
            IndicatorString = "POS|" + x + "|" + y + "|" + z;
            Indicator ourIndi = new Indicator();
            ourIndi.IconType = icon;
            ourIndi.Type = type;
            ourIndi.QuestID = QuestID;
            ourIndi.x = x;
            ourIndi.y = y;
            ourIndi.z = z;
            Indicators.Add(ourIndi);
            player.LastIndicator = ourIndi;
            player.NetworkActions.UpdateIndicator(true, ourIndi);

        }

        public int tempStep = -1;
        public int tempStage = -1;

        public void RemoveIndicator(int QuestID, IndicatorIcon icon, IndicatorType type, float x, float y, float z)
        {

            IndicatorString = "";
            Indicator toRemove = null;
            foreach (Indicator ind in Indicators)
                if (ind.QuestID == QuestID && ind.Type == type && ind.x == x && ind.y == y && ind.z == z)
                {
                    toRemove = ind;
                    break;
                }
            if (toRemove != null)
            {
                Server.Log("removing indiactor");
                player.NetworkActions.UpdateIndicator(false, toRemove);
                Indicators.Remove(toRemove);
            }

        }

        public void RemoveIndicator(int QuestID, IndicatorIcon icon, IndicatorType type, int ID)
        {
            IndicatorString = "";
            Indicator toRemove = null;
            foreach (Indicator ind in Indicators)
                if (ind.QuestID == QuestID && ind.Type == type && ind.ObjOrNpcID == ID)
                {
                    toRemove = ind;
                    break;
                }
            if (toRemove != null)
            {
                Server.Log("removing indiactor");
                player.NetworkActions.UpdateIndicator(false, toRemove);
                Indicators.Remove(toRemove);
            }

            // update client
        }


        public void AddQuestInfo(int QuestID, int InfoID, string info)
        {
            if (!QuestInformation.ContainsKey(QuestID))
            {
                QuestInformation[QuestID] = new QuestInfo[100];
            }
            if (QuestInformation[QuestID][InfoID] != null)
                RemoveQuestInfo(QuestID, InfoID);

            QuestInfo qi;
            if (QuestInformation[QuestID][InfoID] != null)
                qi = QuestInformation[QuestID][InfoID];
            else qi = new QuestInfo();
            qi.InfoID = InfoID;
            qi.info = info;
            qi.QuestID = QuestID;
            QuestInformation[QuestID][InfoID] = qi;
            player.NetworkActions.UpdateQuestInfo(true, qi);

        }

        public void RestoreQuestInfo(string info)
        {
            var arr = info.Split('|');
            foreach (var str in arr)
            {
                string[] spl = str.Split('~');
                if (spl.Length > 1)
                {
                    int num = int.Parse(spl[0]);
                    bool finished = spl[1] == "C";
                    string txt = spl[2];
                    AddQuestInfo(THE_FORGOTTEN.QUEST_ID, num, txt);
                    if (finished)
                        RemoveQuestInfo(THE_FORGOTTEN.QUEST_ID, num, true);
                }
            }

        }

        public string GetQuestInfoString()
        {
            string line = "";
            int count = 0;
            if (QuestInformation != null)
            {
                if (QuestInformation.ContainsKey(THE_FORGOTTEN.QUEST_ID))
                    foreach (var qi in QuestInformation[THE_FORGOTTEN.QUEST_ID])
                    {
                        if (qi != null)
                        {
                            if (count > 0)
                                line += "|";
                            line += qi.InfoID + "~";
                            if (qi.finished) // completed / striked out
                            {
                                line += "C~";
                            }
                            else
                            {
                                line += "A~";
                            }
                            line += qi.info;
                            count++;
                        }
                    }
            }

            return line;
        }

        public void QuestComplete()
        {
            player.NetworkActions.CompleteQuest();
        }

        public void RemoveQuestInfo(int QuestID, int InfoID, bool completed = false)
        {
            if (completed)
                player.NetworkActions.PlaySound("quest_updated");
            var dict = QuestInformation[QuestID];
            if (dict != null)
            {
                if (QuestInformation[QuestID][InfoID] != null)
                {

                    player.NetworkActions.UpdateQuestInfo(false, QuestInformation[QuestID][InfoID], completed);
                    if (!completed)
                        QuestInformation[QuestID][InfoID] = null;
                    else
                        QuestInformation[QuestID][InfoID].finished = true;
                }
            }
        }

        public void ClearQuestInfo(int QuestID)
        {
            if (!QuestInformation.ContainsKey(QuestID))
                return;
            var dict = QuestInformation[QuestID];
            if (dict != null)
            {
                for (int i = 0; i < QuestInformation[QuestID].Length; i++)
                {
                    if (QuestInformation[QuestID][i] != null)
                        RemoveQuestInfo(QuestID, i);
                }
            }
        }


        public string Npc(int id)
        {
            return "<color=yellow>" + DataManager.NpcDefinitions[id].Name + "</color>";
        }

        public string Item(int id)
        {
            return "<color=#08E1EC>[" + DataManager.ItemDefinitions[id].ItemName + "]</color>";
        }

        public enum IndicatorType { NPC, OBJECT, POSITION };
        public enum IndicatorIcon { DOWN_ARROW };

        public class Indicator
        {
            public IndicatorIcon IconType;
            public IndicatorType Type;
            public float x;
            public float y;
            public float z;
            public int ObjOrNpcID;
            public int QuestID;
        }

        public class QuestInfo
        {
            public string info;
            public int QuestID;
            public int InfoID;
            public bool finished = false;
        }
        */
        #endregion
    }
}
