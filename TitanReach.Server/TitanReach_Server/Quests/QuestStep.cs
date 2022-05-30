namespace TitanReach_Server.Quests {
    public class QuestStep {
        public int ID;
        public string Text;
        public bool complete;

        public QuestStep(string text) {
            ID = 0;
            Text = text;
            complete = false;
        }

        public QuestStep(int id, string text, bool complete) : this(text) {
            ID = id;
            this.complete = complete;
        }
    }
}