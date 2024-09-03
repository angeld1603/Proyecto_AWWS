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

        public string FechaIngreso { get; set; }

        public string FechaEntregaPrevista { get; set; }

        public string Estado { get; set; }

        public int IdCliente { get; set; }  // Relacionado con la clase Cliente para obtener el id del cliente
        public string placa { get; set; }  // Relacionado con la clase Vehiculo para obtener la placa del vehiculo
        public int NumeroDocumento { get; set; }  // Relacionado con la clase Mecanico para obtener el id del mecanico
    }
}