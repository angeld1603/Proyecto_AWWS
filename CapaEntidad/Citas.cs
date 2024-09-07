using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Citas
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // Identificador único para la cita
        public string IdCliente { get; set; } // Identificador del cliente que solicita la cita
        public string NumeroDocumento { get; set; } // Identificador del mecánico asignado (si aplica)
        public DateTime FechaHora { get; set; } // Fecha y hora de la cita
        public string Descripción { get; set; } // Descripción de la cita
        public string Estado { get; set; } // Estado de la cita (ej. "Pendiente", "Confirmada", "Cancelada")
    }

}
