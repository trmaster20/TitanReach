
using TRShared.Data.Enums;

namespace TitanReach_Server.Utilities {

    [System.AttributeUsage(
        System.AttributeTargets.Method,
        AllowMultiple = true)
        ]

    public class Command : System.Attribute {
        public string [] Args { get; private set; }
        public Rank RequiredRank {get; private set; }
        public string Description { get; private set; }
        public string ArgsList { get; private set; }
        public string [] CommandStrings { get; private set; }

        public Command(Rank requiredRank, string description, string [] args, params string [] commandStrings) {
            RequiredRank = requiredRank;
            Description = description;
            Args = args;
            CommandStrings = commandStrings;
        }

        public string GetExampleText() {
            string text = "/" + CommandStrings[0];
            foreach (string arg in Args) {
                text += " <" + arg + ">";
            }
            return text;
        }

        public string HelpText() {
            string text = "/";
            foreach(string commandString in CommandStrings) {
                text += "[" + commandString + "] ";
            }
            foreach (string arg in Args) {
                text += " <" + arg + ">";
            }
            text += " : " + Description;
            return text;
        }
    }
}
