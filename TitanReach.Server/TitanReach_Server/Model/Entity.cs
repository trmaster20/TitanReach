using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TitanReach_Server.Model
{
    public class Entity
    {

        public Entity()
        {

        }

        public string Name;

        private int max_hp;
        public int MaxHP
        {
            get
            {
                if (GetType() ==  typeof(Npc))
                {
                    // return NPC definition max health;
                }
                return max_hp;
            }
            set
            {
                max_hp = value;
            }
        }
        public uint UID = 0;


    }
}
