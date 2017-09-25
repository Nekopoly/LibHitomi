using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace LibHitomi.Database
{
    /// <summary>
    /// 갤러리 객체들을 관계형 데이터베이스에 기록합니다. ADO.NET Connector의 설치가 필요할 수 있습니다.
    /// </summary>
    public class DatabaseWriter
    {
        private string connectionString;
        /// <summary>
        /// DatabaseWrtier 클래스를 초기화합니다.
        /// </summary>
        /// <param name="connectionString">연결 문자열</param>
        public DatabaseWriter(string connectionString)
        {
            this.connectionString = connectionString;
        }
        /// <summary>
        /// 테이블들을 생성합니다.
        /// </summary>
        /// <param name="ifNotExists">존재하지 않는 경우에만 생성할 지의 여부입니다.</param>
        public void CreateTables(bool ifNotExists = true)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    string query = "CREATE TABLE " + (ifNotExists ? "IF NOT EXISTS " : "") + "Galleries (Id int PRIMARY KEY, Name varchar(1024), Language varchar(1024), CrawlMethod int, Type varchar(1024) NOT NULL, VideoFilename varchar(1024), VideoGalleryId int)";
                    Console.WriteLine("SQL Query Executing : " + query);
                    MySqlCommand comm = new MySqlCommand(query, connection, transaction);
                    comm.ExecuteNonQuery();
                    foreach (string tableName in new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" })
                    {
                        Console.WriteLine("SQL Query Executing : " + query);
                        query = "CREATE TABLE " + (ifNotExists ? "IF NOT EXISTS " : "") + tableName + " (Id int, Value varchar(1024))";
                        MySqlCommand subcomm = new MySqlCommand(query, connection, transaction);
                        subcomm.ExecuteNonQuery();
                    }
                }
            }
        }
        /// <summary>
        /// 갤러리 정보들을 삽입하는 SQL 코드를 만들어 반환합니다.
        /// </summary>
        /// <param name="galleries">기록할 갤러리들입니다.</param>
        /// <param name="dropAll">기록 전 모든 테이블에서 데이터를 삭제할 지의 여부입니다.</param>
        /// <returns></returns>
        public string BuildSQL(IEnumerable<Gallery> galleries, bool dropAll = true)
        {
            StringBuilder builder = new StringBuilder();
            string[] subTables = new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" };
            if (dropAll)
            {
                foreach (string i in subTables.Concat(new string[] { "Galleries" }))
                {
                    builder.AppendLine($"TRUNCATE TABLE {i};");
                }
            }
            builder.Append(buildGalleryInsertQuery(galleries));
            builder.AppendLine(";");
            foreach (string i in subTables)
            {
                builder.Append(buildSubTableQuery(galleries, i));
                builder.AppendLine(";");
            }
            return builder.ToString();
        }
        /// <summary>
        /// 갤러리 정보들을 삽입하는 SQL 코드를 만들어 스트림에 기록합니다.
        /// </summary>
        /// <param name="galleries">기록할 갤러리들입니다.</param>
        /// <param name="dropAll">기록 전 모든 테이블에서 데이터를 삭제할 지의 여부입니다.</param>
        /// <param name="writer">SQL이 기록될 스트림입니다.</param>
        /// <returns></returns>
        public void BuildSQL(IEnumerable<Gallery> galleries, StreamWriter writer, bool dropAll = true)
        {
            string[] subTables = new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" };
            if (dropAll)
            {
                foreach (string i in subTables.Concat(new string[] { "Galleries" }))
                {
                    writer.WriteLine($"TRUNCATE TABLE {i};");
                }
            }
            writer.Write(buildGalleryInsertQuery(galleries));
            writer.WriteLine(";");
            foreach (string i in subTables)
            {
                writer.Write(buildSubTableQuery(galleries, i));
                writer.WriteLine(";");
            }
        }
        /// <summary>
        /// 데이터베이스에 갤러리들을 기록합니다.
        /// </summary>
        /// <param name="galleries">기록할 갤러리들입니다.</param>
        /// <param name="dropAll">기록 전 모든 테이블에서 데이터를 삭제할 지의 여부입니다.</param>
        public void WriteToDatabases(IEnumerable<Gallery> galleries, bool dropAll = true)
        {
            string[] subTables = new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" };
            string galleriesQuery = buildGalleryInsertQuery(galleries);
            Console.WriteLine("Built gallery insert query");
            Dictionary<string, string> subTableQueries = new Dictionary<string, string>();
            foreach(string i in subTables)
            {
                subTableQueries[i] = buildSubTableQuery(galleries, i);
                Console.WriteLine($"Built {i} insert query");
            }
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                Console.WriteLine("Openning connection");
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    if (dropAll)
                        foreach (string tableName in new string[] { "Galleries", "Artists", "Characters", "Groups", "Parodies", "Tags" })
                            DeleteAllFromTable(tableName, connection, transaction);
                    foreach (string i in new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" })
                    {
                        MySqlCommand loadComm = new MySqlCommand(subTableQueries[i], connection, transaction);
                        int sresultCount = loadComm.ExecuteNonQuery();
                        Console.WriteLine($"Inserted {sresultCount} rows into {i} Table");
                    }
                    MySqlCommand comm = new MySqlCommand(galleriesQuery, connection, transaction);
                    int gresultCount = comm.ExecuteNonQuery();
                    Console.WriteLine($"Inserted {gresultCount} rows into Galleries Table");
                    transaction.Commit();
                }
            }
            Console.WriteLine("Closed connection");
        }
        private string buildGalleryInsertQuery(IEnumerable<Gallery> galleries)
        {
            List<string> items = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("INSERT INTO Galleries (Id, Language, Name, CrawlMethod, Type, VideoFilename, VideoGalleryId) VALUES ");
            foreach(Gallery gallery in galleries)
            {
                items.Add(string.Format("({0},{1},{2},{3},{4},{5},{6})",
                    gallery.Id,
                    gallery.Language == null ? "NULL" : "'" + MySqlHelper.EscapeString(gallery.Language) + "'",
                    gallery.Name == null ? "NULL" : "'" + MySqlHelper.EscapeString(gallery.Name) + "'",
                    (int)gallery.GalleryCrawlMethod,
                    gallery.Type == null ? "NULL" : "'" + MySqlHelper.EscapeString(gallery.Type) + "'",
                    gallery.VideoFilename == null ? "NULL" : "'" + MySqlHelper.EscapeString(gallery.VideoFilename) + "'",
                    gallery.VideoGalleryId
                    ));
            }
            stringBuilder.Append(string.Join(",", items));
            return stringBuilder.ToString();
            }
        private string buildSubTableQuery(IEnumerable<Gallery> galleries, string tableName)
        {
            List<string> queryItems = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"INSERT INTO {tableName} (Id, Value) VALUES ");
            foreach (Gallery gallery in galleries)
            {
                int id = gallery.Id;
                object wrapped = gallery.GetType().GetProperty(tableName).GetValue(gallery);
                if (wrapped == null) continue;
                string[] items = (string[])wrapped;
                foreach(string i in items)
                {
                    queryItems.Add($"({gallery.Id},'{MySqlHelper.EscapeString(i)}')");
                }
            }
            stringBuilder.Append(string.Join(",", queryItems));
            return stringBuilder.ToString();
        }
        private void DeleteAllFromTable(string tableName, MySqlConnection connection, MySqlTransaction transaction)
        {
            MySqlCommand comm = new MySqlCommand($"TRUNCATE TABLE {tableName}", connection, transaction);
            comm.ExecuteNonQuery();
        }
    }
}
