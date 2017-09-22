using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

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
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            using (OdbcTransaction transaction = connection.BeginTransaction())
            {
                OdbcCommand comm = new OdbcCommand("CREATE TABLE (Id int PRIMARY KEY, Name varchar(1024), Language varchar(1024), CrawlMethod int, Type varchar(1024) NOT NULL, VideoFilename varchar(1024), VideoGalleryId int)" + (ifNotExists ? " IF NOT EXISTS" : ""), connection, transaction);
                comm.ExecuteNonQuery();
                foreach (string tableName in new string[] { "Galleries", "Artists", "Characters", "Groups", "Parodies", "Tags" })
                {
                    OdbcCommand subcomm = new OdbcCommand("CREATE TABLE (Id int, Value varchar(1024))" + (ifNotExists ? " IF NOT EXISTS" : ""), connection, transaction);
                    subcomm.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 데이터베이스에 갤러리들을 기록합니다.
        /// </summary>
        /// <param name="galleries">기록할 갤러리들입니다.</param>
        /// <param name="dropAll">기록 전 모든 테이블에서 데이터를 삭제할 지의 여부입니다.</param>
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
