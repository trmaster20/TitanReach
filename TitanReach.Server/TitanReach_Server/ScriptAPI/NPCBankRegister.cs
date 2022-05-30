using System;
using TitanReach_Server.Model;

namespace TitanReach_Server {

    public class NPCBankRegister {

        public Action<Player> act;

        public NPCBankRegister() {
            this.act = (player) => OpenBank(player);
        }

        private void OpenBank(Player player) {
            player.NetworkActions.SendBank();
        }
    }
}
