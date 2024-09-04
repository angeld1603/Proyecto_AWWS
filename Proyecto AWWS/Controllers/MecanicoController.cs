using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using CapaEntidad;

namespace Proyecto_AWWS.Controllers
{
    public class MecanicoController : Controller
    {
        private readonly IMongoCollection<Mecanicos> mecanicosCollection;
        private readonly IMongoCollection<Asistencia> asistenciaCollection;

        public MecanicoController()
        {
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");
            var database = client.GetDatabase("AWWS");

            mecanicosCollection = database.GetCollection<Mecanicos>("Mecanicos");
            asistenciaCollection = database.GetCollection<Asistencia>("Asistencia");
        }

        public ActionResult Inicio()
        {
            return View();
        }

        public ActionResult GestionarReparaciones()
        {
            return View();
        }

        public ActionResult Servicio()
        {
            return View();
        }

        [HttpGet]
        public JsonResult RegistrarAsistencia(string qrContent)
        {
            // Verificar que el contenido del QR sea el correcto
            if (qrContent != "https://10.180.145.194:45455/Mecanico/RegistrarAsistencia")
            {
                return Json(new { success = false, message = "QR incorrecto. Asegúrese de escanear el código correcto." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar que el mecánico esté autenticado
            if (Session["NumeroDocumento"] == null)
            {
                return Json(new { success = false, message = "No se pudo registrar la asistencia. Por favor, intente iniciar sesión de nuevo." }, JsonRequestBehavior.AllowGet);
            }

            // Obtener el ID del mecánico desde la sesión
            int numeroDocumento = Convert.ToInt32(Session["numeroDocumento"]);

            // Obtener la información del mecánico desde la colección
            var filtroMecanico = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, numeroDocumento);
            var mecanico = mecanicosCollection.Find(filtroMecanico).FirstOrDefault();

            if (mecanico == null)
            {
                return Json(new { success = false, message = "Mecánico no encontrado en la base de datos." }, JsonRequestBehavior.AllowGet);
            }

            // Obtener la zona horaria de Colombia
            TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Obtener la fecha y hora actual en Colombia
            DateTime fechaActualColombia = TimeZoneInfo.ConvertTime(DateTime.Now, colombiaTimeZone);

            // Convertir a UTC para almacenar en MongoDB
            DateTime fechaActualUTC = TimeZoneInfo.ConvertTimeToUtc(fechaActualColombia, colombiaTimeZone);

            // Verificar si ya existe un registro de entrada sin salida para este mecánico
            var filtroAsistencia = Builders<Asistencia>.Filter.And(
                Builders<Asistencia>.Filter.Eq(a => a.NumeroDocumento, numeroDocumento),
                Builders<Asistencia>.Filter.Eq(a => a.FechaSalida, null)
            );

            var registroExistente = asistenciaCollection.Find(filtroAsistencia).FirstOrDefault();

            if (registroExistente == null)
            {
                // Registrar nueva entrada
                var nuevaAsistencia = new Asistencia
                {
                    NumeroDocumento = numeroDocumento,
                    Nombre = mecanico.Nombre,
                    FechaEntrada = fechaActualUTC,
                    // Otros campos si es necesario
                };

                asistenciaCollection.InsertOne(nuevaAsistencia);

                return Json(new { success = true, message = "Entrada registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Actualizar el registro existente con la fecha de salida
                var update = Builders<Asistencia>.Update.Set(a => a.FechaSalida, fechaActualUTC);
                asistenciaCollection.UpdateOne(filtroAsistencia, update);

                return Json(new { success = true, message = "Salida registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}