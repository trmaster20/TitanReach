using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TRShared;

namespace TitanReach.Server
{
    public class DataLoader
    {

        public static void Load()
        {
            DataManager.IS_SERVER = true;
            TRShared.Logger.OnSharedLog += (sender, text) => TitanReach_Server.Server.Log(text);
            DataManager.OnTable += DataManager_OnTable;
            DataManager.UpdateDBEvent += Update;
            TRShared.DataManager.Load();
        }


        public static Dictionary<int, string> NpcBehaviours = new Dictionary<int, string>();

        public static System.Data.DataTable DataManager_OnTable(string def, string query)
        {
            try
            {
                string db = "Assets/Databases/Definitions/" + def + ".db";
                var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = db;
                var dbcon = new Microsoft.Data.Sqlite.SqliteConnection(connectionStringBuilder.ConnectionString);
                dbcon.Open();
                IDbCommand cmnd_read = dbcon.CreateCommand();
                cmnd_read.CommandText = query;
                DataTable myTable = new DataTable();

               

                IDataReader read = cmnd_read.ExecuteReader();


                myTable.Load(read, LoadOption.Upsert, FillErrorHandler);
                dbcon.Close();
                return myTable;
            }
            catch (Exception e)
            {
                TRShared.Logger.Log("Error OnTable ", e);
                return null;
            }
        }

      

        public static void Update(string query, string db)
        {
            var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = db;
            var dbcon = new Microsoft.Data.Sqlite.SqliteConnection(connectionStringBuilder.ConnectionString);
            dbcon.Open();

            IDbCommand cmnd_read = dbcon.CreateCommand();
            cmnd_read.CommandText = query;
            cmnd_read.ExecuteNonQuery();

        }









        static void FillErrorHandler(object sender, FillErrorEventArgs e)
        {
            // You can use the e.Errors value to determine exactly what
            // went wrong.
            Console.WriteLine(e.Errors.Message);
            if (e.Errors.GetType() == typeof(System.FormatException))
            {
                Console.WriteLine("Error when attempting to update the value: {0}",
                    e.Values[0]);
            }

            // Setting e.Continue to True tells the Load
            // method to continue trying. Setting it to False
            // indicates that an error has occurred, and the 
            // Load method raises the exception that got 
            // you here.
            e.Continue = false;
        }
    }
}
