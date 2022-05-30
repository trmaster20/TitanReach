using TitanReach_Server.Model;

namespace TitanReach_Server
{

    public interface INpcInteract
    {

        int NpcID();

        void OnTalk(Npc n, Player p);


    }

}
