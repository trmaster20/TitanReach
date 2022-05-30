using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanReach_Server.Model;

namespace TitanReach_Server {

    public class NPCQuestRegister {

        public int ID;
        public Func<Npc, Player, int, CancellationToken, Task<int>> func;

        public NPCQuestRegister(int iD, Func<Npc, Player, int, CancellationToken, Task<int>> func) {
            ID = iD;
            this.func = func;
        }

        public override bool Equals(object obj) {
            return obj is NPCQuestRegister register &&
                   ID == register.ID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID);
        }
    }
}
