using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;


namespace CapaEntidad
{
    public class Mecanicos
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Int32 NumeroDocumento { get; set; }
        public string Nombre { get; set; }
        public string Contraseña { get; set; }
        public bool Estado { get; set; } // true = activo, false = no activo

        public string ImagenUrl { get; set; }  // Nueva propiedad
    }
}