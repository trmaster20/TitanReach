using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class DTWrapper
    {
        public DataTable table;
        public int Row;

        public DTWrapper(DataTable table)
        {
            this.table = table;
        }

        public string String(int index) { 
            string str = table.Rows[Row][index].ToString();
            if (string.IsNullOrEmpty(str))
                return null;
            if(str != null)
            {
                if (str.Length > 0)
                    return str;
            }
            return null;
        }

        public int Count => table.Rows.Count;

        public int Int(int index)
        {
            string str = String(index);
            if (str != null)
                return int.Parse(str);
            else return -1;
        }

        public ushort Ushort(int index)
        {
            string str = String(index);
            if (str != null)
                return ushort.Parse(str);
            else return 0;
        }


        public float Float(int index)
        {
            string str = String(index);
            if (str != null)
                return float.Parse(str);
            else return 0f;
        }

        public void Destroy()
        {
            table.Clear();
            table = null;
        }

    }
}
