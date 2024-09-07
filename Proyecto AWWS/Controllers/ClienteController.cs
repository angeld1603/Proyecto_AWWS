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
        private readonly IMongoCollection<Citas> citasCollection;

        private readonly IMongoCollection<Notificaciones> notificacionesCollection;

        public ClienteController()
        {
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            var database = client.GetDatabase("AWWS");

            citasCollection = database.GetCollection<Citas>("Citas");

            notificacionesCollection = database.GetCollection<Notificaciones>("Notificaciones");
        }

        public ActionResult Inicio()
        {
            return View();
        }

        public ActionResult Citas()
        {
            var idCliente = Session["IdCliente"]?.ToString();

            if (string.IsNullOrEmpty(idCliente))
            {
                TempData["ErrorMessage"] = "No se pudo obtener el cliente. Inicie sesión de nuevo.";
                return RedirectToAction("Login", "Login");
            }

            // Obtener las citas confirmadas para el cliente
            var estados = new[] { "Pendiente", "Confirmada" };
            var citaConfirmadas = citasCollection.Find(c => estados.Contains(c.Estado)).ToList();

            return View(citaConfirmadas);
        }

        public ActionResult AgendarCita()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SolicitarCita(Citas citas)
        {
            // Verificar si el cliente está en la sesión
            if (Session["IdCliente"] != null)
            {
                // Asigna el estado inicial de la cita
                citas.Estado = "Pendiente";

                // Obtener el IdCliente desde la sesión y asignarlo a la cita
                citas.IdCliente = Session["IdCliente"].ToString();

                // Guardar la cita en la base de datos
                citasCollection.InsertOne(citas);

                // Notificar al administrador (puede ser por correo o notificación interna)
                EnviarNotificacionAdministrador(citas);

                // Establecer el mensaje de éxito en TempData
                TempData["SuccessMessage"] = "La cita ha sido solicitada exitosamente.";

                // Redirigir a la vista de inicio del cliente (o cualquier vista donde desees mostrar el mensaje)
                return RedirectToAction("Citas", "Cliente");
            }

            // Si no hay cliente en la sesión, redirigir al login
            TempData["Error"] = "Debe iniciar sesión para agendar una cita.";
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
                Mensaje = $"Nueva cita solicitada:\nCliente: {cita.IdCliente}\nFecha y Hora: {cita.FechaHora}\nDescripción: {cita.Descripción}",
                Fecha = DateTime.Now
            };

            notificacionesCollection.InsertOne(notificacion);
        }
    }
}