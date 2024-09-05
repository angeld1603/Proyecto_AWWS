using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CapaEntidad
{
    public class Asistencia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int NumeroDocumento { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaEntrada { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? FechaSalida { get; set; }
    }
}
