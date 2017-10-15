using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using LibHitomi.Search;

namespace LibHitomi.Database
{
    class DatabaseReader
    {
        MongoClient mongoClient;
        string databaseName;
        string collectionName;
        public DatabaseReader(string connectionString, string databaseName, string collectionName)
        {
            this.databaseName = databaseName;
            this.collectionName = collectionName;
            mongoClient = new MongoClient(connectionString);
        }
        public DatabaseReader(MongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
        }
        private IMongoCollection<Gallery> getColleciton()
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            return database.GetCollection<Gallery>(collectionName);
        }
        public IEnumerable<Gallery> GetEveryGalleries()
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinitionBuilder<Gallery> filterBuilder = new FilterDefinitionBuilder<Gallery>();
            return collection.Find(filterBuilder.Empty).ToEnumerable();
        }
        public async Task<IEnumerable<Gallery>> GetEveryGalleriesAsync()
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinitionBuilder<Gallery> filterBuilder = new FilterDefinitionBuilder<Gallery>();
            return (await collection.FindAsync(filterBuilder.Empty)).ToEnumerable();
        }
        public long CountEveryGalleries()
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinitionBuilder<Gallery> filterBuilder = new FilterDefinitionBuilder<Gallery>();
            return collection.Count(filterBuilder.Empty);
        }
        public async Task<long> CountEveryGalleriesAsync()
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinitionBuilder<Gallery> filterBuilder = new FilterDefinitionBuilder<Gallery>();
            return await collection.CountAsync(filterBuilder.Empty);
        }
        private FilterDefinition<Gallery> CreateMongoDBFilter(IEnumerable<QueryEntry> query, out int limit, out int offset)
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinitionBuilder<Gallery> filterDefBuilder = new FilterDefinitionBuilder<Gallery>();
            FilterDefinition<Gallery> filterDef = filterDefBuilder.Empty;
            foreach (QueryEntry queryItem in query)
            {
                string namespaceName = Enum.GetName(typeof(TagNamespace), queryItem.Namespace);
                string propertyName;
                if (queryItem.Namespace == TagNamespace.LibHitomi_Limit)
                {
                    if (int.TryParse(queryItem.Query, out int value)) limit = value;
                    continue;
                }
                else if (queryItem.Namespace == TagNamespace.LibHitomi_Offset)
                {
                    if (int.TryParse(queryItem.Query, out int value)) offset = value;
                    continue;
                }
                else if (namespaceName.StartsWith("LibHitomi"))
                {
                    // Unsupported Query
                    continue;
                }
                switch (queryItem.Namespace)
                {
                    case TagNamespace.Tag:
                    case TagNamespace.Artist:
                    case TagNamespace.Group:
                    case TagNamespace.Character:
                        propertyName = namespaceName + "s";
                        break;
                    case TagNamespace.Language:
                    case TagNamespace.Name:
                    case TagNamespace.Type:
                        propertyName = namespaceName;
                        break;
                    case TagNamespace.Series:
                        propertyName = "Parodies";
                        break;
                    default:
                        continue;
                }
                FilterDefinition<Gallery> def;
                switch (queryItem.QueryType)
                {
                    case QueryMatchType.Equals:
                        if(queryItem.isForArrayNamespace)
                        {
                            def = filterDefBuilder.AnyEq(propertyName, queryItem.Query);
                        } else
                        {
                            def = filterDefBuilder.Eq(propertyName, queryItem.Query);
                        }
                        if (queryItem.isExclusion)
                            def = !def;
                        break;
                    case QueryMatchType.Contains:
                        if (queryItem.isForArrayNamespace)
                        {
                            def = filterDefBuilder.AnyEq(propertyName, new BsonRegularExpression(Regex.Escape(queryItem.Query)));
                        }
                        else
                        {
                            def = filterDefBuilder.Eq(propertyName, queryItem.Query);
                        }
                        if (queryItem.isExclusion)
                            def = !def;
                        break;
                    case QueryMatchType.NA:
                        if (queryItem.isForArrayNamespace)
                        {
                            def = filterDefBuilder.Exists(propertyName, false) | filterDefBuilder.Size(propertyName, 0);
                        }
                        else
                        {
                            def = filterDefBuilder.Exists(propertyName, false) | filterDefBuilder.Eq(propertyName, "");
                        }
                        if (queryItem.isExclusion)
                            def = !def;
                        break;
                    default:
                        throw new Exception("QueryMatchType not specified");
                }
                filterDef = filterDef & def;
            }
            limit = -1;
            offset = -1;
            return filterDef;
        }
        public IEnumerable<Gallery> SearchGalleries(IEnumerable<QueryEntry> query)
        {
            IMongoCollection<Gallery> collection = getColleciton();
            FilterDefinition<Gallery> filterDef = CreateMongoDBFilter(query, out int limit, out int offset);
            IFindFluent<Gallery, Gallery> fluent = collection.Find(filterDef);
            if (offset >= 0) fluent.Skip(offset);
            if (limit >= 0) fluent.Limit(limit);
            return fluent.ToEnumerable();
        }
    }
}
