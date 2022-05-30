using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Enums;

namespace TRShared.Data.Definitions
{
    public class ObjectDef
    {

        public int ID = -1;
        public string Name = "";
        public string Description = "";
        public string Path = "";
        public bool Attackable = false;
        public bool Interactable = false;
        public float InteractableRadius = 0;
        public bool Stoppable = false;
        public string StopText = null;
        public string InteractText = null;
        public int RespawnTime = 5000;

        public MiningDef MiningDef;
        public PickableDef PickableDef;
        public WoodcuttingDef WoodcuttingDef;
        public FishingDef FishDef;


        [Definition("ObjectDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.ObjectDefinitions = new ObjectDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ObjectDef def = new ObjectDef()
                {
                    ID = dt.Int(0),
                    Name = dt.String(1),
                    Description = dt.String(2),
                    Path = dt.String(3),
                    Attackable = dt.Int(4) == 1,
                    Interactable = dt.Int(5) == 1,
                    InteractableRadius = dt.Int(6),
                    StopText = dt.String(7),
                    Stoppable = dt.String(7) != null ? true : false,
                    InteractText = ((dt.Int(5) == 1) && dt.String(8) != null) ? dt.String(8) : default,

                };
                if (dt.String(9) != null)
                    def.LoadAttributes(dt.String(9));
                DataManager.ObjectDefinitions[def.ID] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ObjectDefinitions.Length + " ObjectDefs");
    }


    private bool[] AttributeArray;
        public void LoadAttributes(string str)
        {
            string[] attributes = str.Split('|');
            if (attributes != null && attributes.Length > 0)
            {
                string[] names = Enum.GetNames(typeof(ObjectAttribute));
                for (int i = 0; i < names.Length; i++)
                    names[i] = names[i].ToLower();

                AttributeArray = new bool[names.Length];
                foreach (string att in attributes)
                {
                    string attribute = att.ToLower();
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (names[i] == attribute)
                        {
                            AttributeArray[i] = true;
                            break;
                        }
                    }

                }
            }
        }

        public bool HasAttribute(ObjectAttribute attr) => AttributeArray == null ? false : AttributeArray[(int)attr];

        public bool HasAnyAttribute(params ObjectAttribute[] attr)
        {
            if (AttributeArray == null)
                return false;

            foreach (ObjectAttribute a in attr)
                if (HasAttribute(a))
                    return true;

            return false;
        }


    }
}
