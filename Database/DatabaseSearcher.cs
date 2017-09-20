using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;
using LibHitomi.Search;
namespace LibHitomi.Database
{
    class DatabaseSearcher
    {
        private string connectionString;
        public DatabaseSearcher(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public bool HasGalleryId(int id)
        {
            using (OdbcConnection conn = new OdbcConnection(connectionString))
            using (OdbcCommand comm = new OdbcCommand("SELECT COUNT(*) FROM Galleries WHERE Id = @Id", conn))
            {
                comm.CommandType = CommandType.Text;
                comm.Parameters.Add(new OdbcParameter("@Id", id));
                return (int)comm.ExecuteScalar() > 0;
            }
        }
        public IEnumerable<Gallery> GetGalleriesById(IEnumerable<int> ids)
        {
            string sqlCondition = string.Join(" OR ", ids.Select(v => "Id = " + v));
            using (OdbcConnection conn = new OdbcConnection(connectionString))
            using (OdbcCommand comm = new OdbcCommand("SELECT * FROM Galleries WHERE " + sqlCondition))
            {
                // get Gallery info
                DataSet dataSet = new DataSet();
                OdbcDataAdapter adapter = new OdbcDataAdapter(comm);
                adapter.Fill(dataSet, "Galleries");
                foreach (string i in new string[] { "Artists", "Characters", "Groups", "Parodies", "Tags" })
                {
                    OdbcCommand subComm = new OdbcCommand("SELECT * FROM " + i + " WHERE " + sqlCondition);
                    OdbcDataAdapter subAdapter = new OdbcDataAdapter(subComm);
                    subAdapter.Fill(dataSet, i);
                }
                // make Object
                foreach(int id in sqlCondition)
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
        public IEnumerable<int> Search(IEnumerable<QueryEntry> query)
        {
            throw new NotImplementedException();
        }
    }
}
