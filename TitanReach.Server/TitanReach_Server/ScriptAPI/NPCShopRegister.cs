using System;
using TitanReach_Server.Model;

namespace TitanReach_Server {

    public class NPCShopRegister {

        public int ID;
        public Action<Player> act;

        public NPCShopRegister(int iD, Action<Player> act) {
            ID = iD;
            this.act = act;
        }

        public override bool Equals(object obj) {
            return obj is NPCShopRegister register &&
                   ID == register.ID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID);
        }
    }
}
