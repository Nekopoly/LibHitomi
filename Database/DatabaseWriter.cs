using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace LibHitomi.Database
{
    /// <summary>
    /// MongoDB에 Hitomi.la 갤러리 목록을 기록합니다.
    /// </summary>
    public class DatabaseWriter : LibHitomi.ListDownloaderBase
    {
        MongoClient mongoClient;
        public DatabaseWriter(string connectionString)
        {
            mongoClient = new MongoClient(connectionString);
        }
        public DatabaseWriter(MongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
        }
        /// <summary>
        /// 크롤링 방법이 Normal인 모든 갤러리를 데이터베이스에서 삭제하고 Hitomi.la에서 전체 갤러리 목록을 가져와 데이터베이스에 기록합니다. 
        /// </summary>
        /// <param name="databaseName">MongoDB 데이터베이스 이름</param>
        /// <param name="collectionName">MongoDB 콜렉션 이름</param>
        /// <returns></returns>
        public async Task WriteAsync(string databaseName, string collectionName)
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);
            FilterDefinitionBuilder<BsonDocument> filterBuilder = new FilterDefinitionBuilder<BsonDocument>();
            UpdateDefinitionBuilder<BsonDocument> updateDefBuilder = new UpdateDefinitionBuilder<BsonDocument>();
            int chunkCount = getJsonCount();
            List<WriteModel<BsonDocument>> actions = new List<WriteModel<BsonDocument>>();
            actions.Add(new DeleteManyModel<BsonDocument>(filterBuilder.Eq("crawlMethod", GalleryCrawlMethod.Normal)));
            for(int i = 0; i < chunkCount; i++)
            {
                HttpWebRequest request = RequestHelper.CreateRequest(DownloadOptions.JsonSubdomain, $"/galleries{i}.json");
                using (WebResponse wres = await request.GetResponseAsync())
                using (Stream resstr = wres.GetResponseStream())
                using (StreamReader sre = new StreamReader(resstr))
                {
                    foreach (BsonDocument doc in BsonSerializer.Deserialize<BsonArray>(sre).Select(p => p.AsBsonDocument))
                        actions.Add(new InsertOneModel<BsonDocument>(doc));
                }
                Console.WriteLine($"Added Actions from {i + 1}/{chunkCount}st gallery chunk");
            }
            actions.Add(new UpdateManyModel<BsonDocument>(filterBuilder.Exists("crawlMethod", false), updateDefBuilder.Set("crawlMethod", GalleryCrawlMethod.Normal)));
            await collection.BulkWriteAsync(actions);
        }
        /// <summary>
        /// 제공받은 갤러리들을 데이터베이스에 기록합니다.
        /// </summary>
        /// <param name="galleries">기록할 갤러리</param>
        /// <param name="databaseName">MongoDB 데이터베이스 이름</param>
        /// <param name="collectionName">MongoDB 콜렉션 이름<</param>
        /// <returns></returns>
        public async Task WriteAsync(IEnumerable<Gallery> galleries, string databaseName, string collectionName)
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            IMongoCollection<Gallery> collection = database.GetCollection<Gallery>(collectionName);
            FilterDefinitionBuilder<Gallery> filterBuilder = new FilterDefinitionBuilder<Gallery>();
            await collection.InsertManyAsync(galleries);
        }
    }
}
