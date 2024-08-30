using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CapaEntidad
{
    
    public class Clientes
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Contraseña { get; set; }
        public Int64 NumeroTelefono { get; set; }
        public string Direccion { get; set; }
    }
}
