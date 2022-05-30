
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server.Model
{
    public class Item
    {

        public static int CURRENT_ITEMS = 0;

        public int UID;
        public ushort ID;
        public int Amount = 1;

        public Item(ushort id)
        {
            UID = CURRENT_ITEMS;
            CURRENT_ITEMS++;
            ID = id;
        }

        public Item(ushort id, int amount)
        {
            UID = CURRENT_ITEMS;
            CURRENT_ITEMS++;
            ID = id;
            Amount = amount;
        }

        public ItemDefinition Definition
        {
            get
            {
                return DataManager.ItemDefinitions[ID];
            }
        }

    }
}
