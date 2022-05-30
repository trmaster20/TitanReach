using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class Definition : System.Attribute
    {
        public string Name { get; private set; }
        public Definition(string Name)
        {
            this.Name = Name;
        }
    }

}
