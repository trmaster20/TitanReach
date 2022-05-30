using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Enums
{
    public struct ItemDrop
    {
        public ushort ItemID;
        public int Amount;
        public int Weight;   // The running weight
        public int DBWeight; //Not the running weight
        public int AmountMin;
        public int AmountMax;
    }
}
