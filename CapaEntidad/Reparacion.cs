using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Reparacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime FechaEntregaPrevista { get; set; }

        public string Estado { get; set; }

        public string VehiculoId { get; set; } // Para identificar el vehículo

        public string MecanicoId { get; set; } // Para identificar al mecánico asignado

        public List<string> Repuestos { get; set; } // Lista de repuestos asignados
    }
}
