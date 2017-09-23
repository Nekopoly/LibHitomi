using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LibHitomi.Search;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace LibHitomi.Database
{
    /// <summary>
    /// 데이터베이스에서 작품들을 검색합니다. ADO.NET Connector의 설치가 필요할 수 있습니다.
    /// </summary>
    class DatabaseSearcher
    {
        private string connectionString;
        /// <summary>
        /// 클래스를 초기화합니다.
        /// </summary>
        /// <param name="connectionString">연결 문자열입니다.</param>
        public DatabaseSearcher(string connectionString)
        {
            this.connectionString = connectionString;
        }
        /// <summary>
        /// 갤러리가 존재하는 지의 여부를 확인합니다.
        /// </summary>
        /// <param name="id">갤러리 ID</param>
        /// <returns></returns>
        public bool HasGalleryId(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand comm = new MySqlCommand("SELECT COUNT(*) FROM Galleries WHERE Id = @Id", conn))
                {
                    comm.CommandType = CommandType.Text;
                    comm.Parameters.Add(new MySqlParameter("@Id", id));
                    return (int)comm.ExecuteScalar() > 0;
                }
            }
        }
        /// <summary>
        /// ID들을 입력받아 갤러리 정보를 가져옵니다.
        /// </summary>
        /// <param name="ids">갤러리 ID들</param>
        /// <returns></returns>
        public IEnumerable<Gallery> GetGalleriesById(IEnumerable<int> ids)
        {
            string sqlCondition = string.Join(" OR ", ids.Select(v => "Id = " + v));
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand comm = new MySqlCommand("SELECT * FROM Galleries WHERE " + sqlCondition))
                {
                    // get Gallery info
                    DataSet dataSet = new DataSet();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(comm);
                    adapter.Fill(dataSet, "Galleries");
                    foreach (string i in new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" })
                    {
                        MySqlCommand subComm = new MySqlCommand("SELECT * FROM " + i + " WHERE " + sqlCondition);
                        MySqlDataAdapter subAdapter = new MySqlDataAdapter(subComm);
                        subAdapter.Fill(dataSet, i);
                    }
                    // make Object
                    foreach (int id in sqlCondition)
                    {
                        foreach (var i in from j in dataSet.Tables["Galleries"].AsEnumerable() where j.Field<int>("id") == id select j)
                        {
                            //(Id, Language, Name, CrawlMethod, Type, VideoFilename, VideoGalleryId)
                            Gallery gallery = new Gallery((GalleryCrawlMethod)i.Field<int>("CrawlMethod"));
                            gallery.id = i.Field<int>("Id");
                            gallery.language = i.Field<string>("Language");
                            gallery.name = i.Field<string>("Name");
                            gallery.type = i.Field<string>("Type");
                            gallery.videoFilename = i.Field<string>("VideoFilename");
                            gallery.videoGalleryId = i.Field<int>("VideoGalleryId");
                            Type galleryType = gallery.GetType();
                            foreach (string j in new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" })
                            {
                                List<int> temp = new List<int>();
                                foreach (var k in from g in dataSet.Tables[j].AsEnumerable() where g.Field<int>("id") == id select g)
                                {
                                    temp.Add(k.Field<int>("Value"));
                                }
                                galleryType.GetField(j.ToLower(), System.Reflection.BindingFlags.NonPublic).SetValue(gallery, temp.ToArray());
                            }
                            gallery.UnNull();
                            yield return gallery;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 데이터베이스를 검색합니다.
        /// </summary>
        /// <param name="query">쿼리 객체들</param>
        /// <returns></returns>
        public IEnumerable<int> Search(IEnumerable<QueryEntry> query)
        {
            List<int> result = new List<int>();
            int? limit, offset;
            bool firstEntry = true;
            using(MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                foreach (QueryEntry i in query)
                {
                    List<int> entryResult = new List<int>();
                    switch (i.Namespace)
                    {
                        // Tag Tables
                        case TagNamespace.Artist:
                        case TagNamespace.Character:
                        case TagNamespace.Group:
                        case TagNamespace.Series:
                        case TagNamespace.Tag:
                            string tableName = Enum.GetName(typeof(TagNamespace), TagNamespace.Tag);
                            if (tableName == "Series")
                                tableName = "Parodies";
                            else
                                tableName += "s";
                            string sqlQuery;
                            switch (i.QueryType)
                            {
                                case QueryMatchType.Equals:
                                    sqlQuery = "SELECT id FROM " + tableName + " WHERE Value = @match";
                                    break;
                                case QueryMatchType.Contains:
                                    sqlQuery = "SELECT id FROM " + tableName + " WHERE Value = '%' + @match + '%'";
                                    break;
                                case QueryMatchType.NA:
                                    sqlQuery = "SELECT id FROM Galleries Not IN (SELECT id FROM " + tableName + ")";
                                    break;
                                default:
                                    throw new Exception("QueryType not specified!");
                            }
                            using (MySqlCommand comm = new MySqlCommand(sqlQuery, conn))
                            {
                                comm.Parameters.Add(new MySqlParameter("match", i.Query));
                                using (MySqlDataAdapter adapter = new MySqlDataAdapter(comm))
                                using (DataSet ds = new DataSet())
                                {
                                    adapter.Fill(ds, tableName);
                                    entryResult = new List<int>(ds.Tables[tableName]
                                        .AsEnumerable()
                                        .Select(j => j.Field<int>("Value")));
                                }
                            }
                            break;
                        // Unsupported special queries
                        case TagNamespace.LibHitomi_Debug:
                        case TagNamespace.LibHitomi_From:
                        case TagNamespace.LibHitomi_Id:
                            continue;
                        case TagNamespace.LibHitomi_Limit:
                            if(int.TryParse(i.Query, out int newLimit))
                            {
                                limit = newLimit;
                            }
                            break;
                        case TagNamespace.LibHitomi_Offset:
                            if (int.TryParse(i.Query, out int newOffset))
                            {
                                offset = newOffset;
                            }
                            break;
                        // Gallery Info Table
                        case TagNamespace.Name:
                        case TagNamespace.Type:
                        case TagNamespace.Language:
                            string propName = Enum.GetName(typeof(TagNamespace), TagNamespace.Tag);
                            string giSqlQuery;
                            switch (i.QueryType)
                            {
                                case QueryMatchType.Equals:
                                    giSqlQuery = "SELECT id FROM Galleries WHERE " + propName + " = @match";
                                    break;
                                case QueryMatchType.Contains:
                                    giSqlQuery = "SELECT id FROM Galleries WHERE " + propName + " = '%' + @match + '%'";
                                    break;
                                case QueryMatchType.NA:
                                    giSqlQuery = "SELECT id FROM Galleries WHERE " + propName + " =  NULL OR " + propName + " = ''";
                                    break;
                                default:
                                    throw new Exception("QueryType not specified!");
                            }
                            using (MySqlCommand comm = new MySqlCommand(giSqlQuery, conn))
                            {
                                comm.Parameters.Add(new MySqlParameter("match", i.Query));
                                using (MySqlDataAdapter adapter = new MySqlDataAdapter(comm))
                                using (DataSet ds = new DataSet())
                                {
                                    adapter.Fill(ds, "Galleries");
                                    entryResult = new List<int>(ds.Tables["Galleries"]
                                        .AsEnumerable()
                                        .Select(j => j.Field<int>("Value")));
                                }
                            }
                            break;
                    }
                    if (firstEntry)
                    {
                        result = new List<int>(entryResult);
                        firstEntry = false;
                    }
                    else if (i.isExclusion)
                    {
                        result = new List<int>(result.Except(entryResult));
                    } else
                    {
                        result = new List<int>(result.Intersect(entryResult));
                    }
                }
                return result;
            }
        }
    }
}
