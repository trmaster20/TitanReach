using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Structs
{
    public struct Ingredient
    {
        public int ItemID;
        public int Amount;

        public static Ingredient[] Prepare(string str)
        {
            int ingred = 0;
            string[] arr = str.Split(',');
            Ingredient[] ingredients  = new Ingredient[arr.Length];
            foreach (string s in arr)
            {
                Ingredient ing = new Ingredient();
                ing.ItemID = int.Parse(s.Split('=')[0]);
                ing.Amount = int.Parse(s.Split('=')[1]);
                ingredients[ingred] = ing;
                ingred++;
            }
            return ingredients;
        }
    }


}
