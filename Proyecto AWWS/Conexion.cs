using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MongoDB.Driver;

namespace Proyecto_AWWS
{
    public class Conexion
    {
    }

    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDBConnection"].ConnectionString;
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("AWWS");
        }

        public IMongoCollection<T> GetCollection<T>(string Usuarios)
        {
            return _database.GetCollection<T>(Usuarios);
        }
    }

}