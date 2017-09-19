using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

namespace LibHitomi.Database
{
    public class DatabaseWriter
    {
        private string connectionString;
        public DatabaseWriter(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public void CreateTables(bool ifNotExists = true)
        {
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            using (OdbcTransaction transaction = connection.BeginTransaction())
            {
                OdbcCommand comm = new OdbcCommand("CREATE TABLE (Id int PRIMARY KEY, Name varchar(1024), Language varchar(1024), CrawlMethod varchar(1024), Type varchar(1024) NOT NULL, VideoFilename varchar(1024), VideoGalleryId varchar(1024))" + (ifNotExists ? " IF NOT EXISTS" : ""), connection, transaction);
                comm.ExecuteNonQuery();
                foreach (string tableName in new string[] { "Galleries", "Artists", "Characters", "Groups", "Parodies", "Tags" })
                {
                    OdbcCommand subcomm = new OdbcCommand("CREATE TABLE (Id int, Value varchar(1024))" + (ifNotExists ? " IF NOT EXISTS" : ""), connection, transaction);
                    subcomm.ExecuteNonQuery();
                }
            }
        }
        public void WriteToDatabases(IEnumerable<Gallery> galleries, bool dropAll = true)
        {
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            using (OdbcTransaction transaction = connection.BeginTransaction())
            {
                if(dropAll)
                    foreach (string tableName in new string[] { "Galleries", "Artists", "Characters", "Groups", "Parodies", "Tags"})
                        DeleteAllFromTable(tableName, connection, transaction);
                foreach (Gallery gallery in galleries)
                {
                    // Id, Language, Name, CrawlMethod, Type
                    // VideoFilename, VideoGalleryId
                    OdbcCommand comm = new OdbcCommand("INSERT INTO Galleries (Id, Language, Name, CrawlMethod, Type, VideoFilename, VideoGalleryId) VALUES (@Id, @Language, @Name, @CrawlMethod, @Type, @VideoFilename, @VideoGalleryId)", connection, transaction);
                    comm.Parameters.Add(new OdbcParameter("@Id", gallery.Id));
                    comm.Parameters.Add(new OdbcParameter("@Language", gallery.Language));
                    comm.Parameters.Add(new OdbcParameter("@Name", gallery.Name));
                    comm.Parameters.Add(new OdbcParameter("@CrawlMethod", gallery.GalleryCrawlMethod));
                    comm.Parameters.Add(new OdbcParameter("@Type", gallery.Type));
                    comm.Parameters.Add(new OdbcParameter("@VideoFilename", gallery.VideoFilename));
                    comm.Parameters.Add(new OdbcParameter("@VideoGalleryId", gallery.VideoGalleryId));
                    comm.ExecuteNonQuery();
                    // Artist, Character, Group, Parodies, Tags
                    InsertSubItmes(gallery.id, "Artists", gallery.Artists, connection, transaction);
                    InsertSubItmes(gallery.id, "Characters", gallery.Characters, connection, transaction);
                    InsertSubItmes(gallery.id, "Groups", gallery.Groups, connection, transaction);
                    InsertSubItmes(gallery.id, "Parodies", gallery.Parodies, connection, transaction);
                    InsertSubItmes(gallery.id, "Tags", gallery.Tags, connection, transaction);
                }
            }
        }
        private void InsertSubItmes(int galleryId, string tableName, IEnumerable<string> items, OdbcConnection connection, OdbcTransaction transaction)
        {
            if (items.Count() == 0)
                return;
            foreach (string value in items)
            {
                OdbcCommand comm = new OdbcCommand("INSERT INTO " + tableName + "(Id, Value) VALUES (@Id, @Value)", connection, transaction);
                comm.Parameters.Add(new OdbcParameter("@Id", galleryId));
                comm.Parameters.Add(new OdbcParameter("@Value", value));
                comm.ExecuteNonQuery();
            }
        }
        private void DeleteAllFromTable(string tableName, OdbcConnection connection, OdbcTransaction transaction)
        {
            OdbcCommand comm = new OdbcCommand($"DELETE * FROM {tableName}", connection, transaction);
            comm.ExecuteNonQuery();
        }
    }
}
