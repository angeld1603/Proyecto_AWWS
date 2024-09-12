using CapaEntidad;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_AWWS.Controllers
{
    public class ClienteController : Controller
    {
        // Colecciones para manejar los datos de cada una de ellas
        private readonly IMongoCollection<Citas> citasCollection;
        private readonly IMongoCollection<Notificaciones> notificacionesCollection;

        public ClienteController()
        {
            //inicializa el cliente de MongoDB con la cadena de conexion
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            // obtiene la base de datos del cliente MongoDB
            var database = client.GetDatabase("AWWS");

            // se obtienen las colecciones (tablas) de la base de datos
            citasCollection = database.GetCollection<Citas>("Citas");
            notificacionesCollection = database.GetCollection<Notificaciones>("Notificaciones");
        }

        public ActionResult Inicio()
        {
            return View();
        }

        public ActionResult Citas()
        {
            // Obtener el IdCliente desde la sesión
            var idCliente = Session["IdCliente"]?.ToString();

            // Si no hay un cliente autenticado, redirigir al login
            if (string.IsNullOrEmpty(idCliente))
            {
                TempData["ErrorMessage"] = "No se pudo obtener el cliente. Inicie sesión de nuevo.";
                return RedirectToAction("Login", "Login");
            }

            // Filtrar las citas por el IdCliente y el estado (Pendiente o Confirmada)
            var estados = new[] { "Pendiente", "Confirmada" };
            var citasCliente = citasCollection.Find(c => c.IdCliente == idCliente && estados.Contains(c.Estado)).ToList();

            // Pasar las citas filtradas a la vista
            return View(citasCliente);
        }


        public ActionResult AgendarCita()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SolicitarCita(Citas citas)
        {
            if (Session["IdCliente"] != null)
            {
                // Validar que la hora de la cita esté en el rango permitido (7:00 AM - 5:00 PM)
                TimeSpan startTime = new TimeSpan(7, 0, 0);  // 7:00 AM
                TimeSpan endTime = new TimeSpan(17, 0, 0);   // 5:00 PM
                TimeSpan citaHora = citas.FechaHora.TimeOfDay;

                if (citaHora < startTime || citaHora > endTime)
                {
                    TempData["ErrorMessage"] = "La hora de la cita debe estar entre 7:00 AM y 5:00 PM.";
                    return RedirectToAction("AgendarCita");
                }

                // Asigna el estado inicial de la cita
                citas.Estado = "Pendiente";
                citas.IdCliente = Session["IdCliente"].ToString();
                citasCollection.InsertOne(citas);
                EnviarNotificacionAdministrador(citas);

                TempData["SuccessMessage"] = "La cita ha sido solicitada exitosamente.";
                return RedirectToAction("Citas", "Cliente");
            }

            TempData["ErrorMessage"] = "Debe iniciar sesión para agendar una cita.";
            return RedirectToAction("Login", "Login");
        }



        public ActionResult CitaConfirmada()
        {
            return View();
        }

        private void EnviarNotificacionAdministrador(Citas cita)
        {
            var notificacion = new Notificaciones
            {
                Mensaje = $@"
            <div>
                <p><strong>Cliente:</strong> {cita.IdCliente}</p>
                <p><strong>Fecha y Hora:</strong> {cita.FechaHora.ToString("dd/MM/yyyy HH:mm")}</p>
                <p><strong>Descripción:</strong> {cita.Descripción}</p>
            </div>",
                Fecha = DateTime.Now
            };

            notificacionesCollection.InsertOne(notificacion);
        }

    }
}