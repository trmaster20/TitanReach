using System;
using System.Collections.Generic;
using System.Text;

namespace TitanReach_Server {
    public class Quest {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set;}

        private Dictionary<int, QuestInfo> questInfo;
        public int CompletedState {get; private set; }

        public Quest(int iD, string name, Dictionary<int, QuestInfo> questInfo, int completedState) {
            ID = iD;
            Name = name;
            this.questInfo = questInfo;
            CompletedState = completedState;
        }

        public QuestInfo GetQuestInfo(int state) {
            if (questInfo.ContainsKey(state)) {
                return questInfo[state];
            }
            return null;
        }

        public override bool Equals(object obj) {
            return obj is Quest quest &&
                   ID == quest.ID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID);
        }
    }

    public class QuestInfo {
        public List<string> HintText { get; private set; }

        public string JournalText { get; private set; }

        public QuestInfo(List<string> hintText, string journalText) {
            HintText = new List<string>();

            HintText = hintText;
            JournalText = journalText;
        }
    }
}
