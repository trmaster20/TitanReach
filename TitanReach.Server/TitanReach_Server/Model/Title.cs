
namespace TitanReach_Server.Model {
    public class Title {
        public int ID { get; private set; }
        public string Display { get; private set; }
        public TitleType Type { get; private set; }

        public Title(int id, string display, TitleType type) {
            ID = id;
            Display = display;
            Type = type;
        }
    }

    public enum TitleType {
        HIDDEN = 0,
        NORMAL = 1,
        ELITE = 10
    }
}
