using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackStatusSetter
{
    public class SqliteDataAccess
    {
        public static List<SlackProfile> LoadStatuses()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<SlackProfile>("select * from SlackStatus", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void SaveStatus(SlackProfile profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into SlackStatus (status_text, status_emoji) values (@status_text, @status_emoji)", profile);
            }
        }

        public static void DeleteStatus(SlackProfile profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from SlackStatus where status_text = @status_text and status_emoji = @status_emoji", profile);
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
