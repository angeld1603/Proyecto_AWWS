using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;



namespace CapaEntidad
{
    public class Usuarios
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int idUsuario { get; set; }
        public string Nombre { get; set; }
        public string Contraseña { get; set; }
        public int Edad { get; set; }

    }
}