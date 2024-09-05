using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using CapaEntidad;
using Microsoft.Ajax.Utilities;
using QRCoder;

namespace Proyecto_AWWS.Controllers
{
    public class InicioController : Controller
    {
        private readonly IMongoCollection<Mecanicos> mecanicoCollection;
        private readonly IMongoCollection<Vehiculos> vehiculoCollection;
        private readonly IMongoCollection<Reparacion> reparacionCollection;
        private readonly IMongoCollection<Clientes> clientesCollection;


        public InicioController()
        {
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            var database = client.GetDatabase("AWWS");

            mecanicoCollection = database.GetCollection<Mecanicos>("Mecanicos");

            vehiculoCollection = database.GetCollection<Vehiculos>("Vehiculos");

            reparacionCollection = database.GetCollection<Reparacion>("Reparacion");

            clientesCollection = database.GetCollection<Clientes>("Clientes");
        }

        //Vistas
        public ActionResult Inicio()
        {
            return View();
        }

        // Acción para mostrar la lista de mecánicos 
        public ActionResult GestionarMecanicos()
        {
            var mecanicos = mecanicoCollection.Find(m => true).ToList();
            return View(mecanicos);
        }

        // Acción para mostrar el formulario de registro de mecanicos
        public ActionResult Registrar()
        {
            return View();
        }

        // Acción para mostrar el formulario de edición de mecanicos
        public ActionResult Editar(int id)
        {
            var mecanico = mecanicoCollection.Find(m => m.NumeroDocumento == id).FirstOrDefault();
            if (mecanico == null)
            {
                return HttpNotFound();
            }
            return View(mecanico);
        }

        [HttpPost]
        public ActionResult SubirFoto(HttpPostedFileBase archivo, int numeroDocumento)
        {
            if (archivo != null && archivo.ContentLength > 0)
            {
                // Genera el nombre de archivo
                var fileName = $"{numeroDocumento}.jpg";
                var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                // Guarda el archivo en el servidor
                archivo.SaveAs(path);

                // Actualiza la URL en la base de datos
                var mecanico = mecanicoCollection.Find(m => m.NumeroDocumento == numeroDocumento).FirstOrDefault();
                if (mecanico != null)
                {
                    mecanico.ImagenUrl = $"/Content/Images/{fileName}";
                    var filter = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, numeroDocumento);
                    var update = Builders<Mecanicos>.Update.Set(m => m.ImagenUrl, mecanico.ImagenUrl);
                    mecanicoCollection.UpdateOne(filter, update);
                }
            }

            return RedirectToAction("GestionarMecanicos");
        }

        public ActionResult RegistrarVehiculos()
        {
            return View();
        }

        // Acción para mostrar el formulario de edicion de vehiculos
        public ActionResult EditarV(string placa)
        {
            // Buscar el vehículo por su placa
            var vehiculos = vehiculoCollection.Find(v => v.placa == placa).FirstOrDefault();

            // Verificar si el vehículo no se encontró
            if (vehiculos == null)
            {
                return HttpNotFound();
            }

            // Devolver la vista con el vehículo encontrado
            return View(vehiculos);
        }

        [HttpPost]
        public ActionResult EditarVehiculos(Vehiculos vehiculos)
        {
            if (ModelState.IsValid)
            {
                // Crear un filtro para encontrar el vehículo por su placa
                var filter = Builders<Vehiculos>.Filter.Eq(v => v.placa, vehiculos.placa);

                // Crear la actualización con los nuevos valores
                var update = Builders<Vehiculos>.Update
                    .Set(v => v.marca, vehiculos.marca)
                    .Set(v => v.modelo, vehiculos.modelo)
                    .Set(v => v.año, vehiculos.año);

                // Ejecutar la actualización
                var result = vehiculoCollection.UpdateOne(filter, update);

                // Verificar si la actualización tuvo éxito
                if (result.ModifiedCount > 0)
                {
                    return RedirectToAction("GestionarVehiculos");
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo actualizar el vehículo. Verifique los datos e intente nuevamente.");
                }
            }
            return View(vehiculos);
        }

        // Acción para eliminar un mecánico
        public ActionResult EliminarVehiculos(string placa)
        {
            vehiculoCollection.DeleteOne(v => v.placa == placa);
            return RedirectToAction("GestionarVehiculos");
        }

        // Acción para mostrar el formulario de gestion de reparaciones

        public ActionResult GestionarReparaciones()
        {
            var reparacion = reparacionCollection.Find(r => true).ToList();
            return View(reparacion);
        }

        public ActionResult RegistrarReparacion()
        {
            // Filtra mecánicos activos
            var mecanicosActivos = mecanicoCollection
                .Find(m => m.Estado == true) // Asegúrate de que el campo Estado es booleano
                .ToList();

            // Prepara la lista para el Dropdown de mecánicos activos
            ViewBag.Mecanicos = new SelectList(mecanicosActivos, "NumeroDocumento", "Nombre");
            ViewBag.Clientes = new SelectList(clientesCollection.Find(c => true).ToList(), "IdCliente", "Nombre");
            ViewBag.Vehiculos = new SelectList(vehiculoCollection.Find(v => true).ToList(), "placa", "placa");

            return View();
        }

        [HttpGet]
        public JsonResult BuscarIdClientePorPlaca(string placa)
        {
            if (string.IsNullOrEmpty(placa))
            {
                return Json(new { idCliente = (string)null }, JsonRequestBehavior.AllowGet);
            }

            // Supongamos que tienes las colecciones de vehículos y clientes configuradas
            var vehiculo = vehiculoCollection.Find(v => v.placa == placa).FirstOrDefault();
            if (vehiculo != null)
            {
                var cliente = clientesCollection.Find(c => c.IdCliente == vehiculo.IdCliente).FirstOrDefault();
                return Json(new { idCliente = cliente?.IdCliente.ToString() ?? "Cliente no encontrado" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { idCliente = "Cliente no encontrado" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarR(Reparacion reparacion)
        {
            if (ModelState.IsValid)
            {
                // Verifica si el cliente existe
                var cliente = clientesCollection.Find(c => c.IdCliente == reparacion.IdCliente).FirstOrDefault();
                if (cliente == null)
                {
                    ModelState.AddModelError("", "Cliente no encontrado.");
                    return View(reparacion);
                }

                // Verifica si el vehículo existe y pertenece al cliente
                var vehiculo = vehiculoCollection.Find(v => v.placa == reparacion.placa && v.IdCliente == reparacion.IdCliente).FirstOrDefault();
                if (vehiculo == null)
                {
                    ModelState.AddModelError("", "Vehículo no encontrado o no pertenece al cliente seleccionado.");
                    return View(reparacion);
                }

                // Verifica si el mecánico está activo
                var mecanico = mecanicoCollection.Find(m => m.NumeroDocumento == reparacion.NumeroDocumento && m.Estado).FirstOrDefault();
                if (mecanico == null)
                {
                    ModelState.AddModelError("", "Mecánico no activo o no encontrado.");
                    return View(reparacion);
                }

                // Insertar el nuevo documento
                reparacionCollection.InsertOne(reparacion);

                return RedirectToAction("GestionarReparaciones");
            }

            // Si el modelo no es válido, recargar los datos de vista
            ViewBag.Clientes = new SelectList(clientesCollection.Find(c => true).ToList(), "IdCliente", "Nombre", reparacion.IdCliente);
            ViewBag.Vehiculos = new SelectList(vehiculoCollection.Find(v => v.IdCliente == reparacion.IdCliente).ToList(), "placa", "placa", reparacion.placa);
            ViewBag.Mecanicos = new SelectList(mecanicoCollection.Find(m => m.Estado).ToList(), "NumeroDocumento", "Nombre", reparacion.NumeroDocumento);

            return View(reparacion);
        }


        public JsonResult ObtenerClientes(string filtro)
        {
            var clientes = clientesCollection.Find(c => c.Nombre.Contains(filtro)).ToList();
            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerVehiculosPorCliente(int clienteId)
        {
            var vehiculos = vehiculoCollection.Find(v => v.IdCliente == clienteId).ToList();
            return Json(vehiculos, JsonRequestBehavior.AllowGet);
        }


        // Acción para mostrar el formulario de gestion de citas
        public ActionResult GestionarCitas()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(Mecanicos mecanico)
        {
            if (ModelState.IsValid)
            {
                // Agregar el nuevo mecánico a la base de datos sin establecer el estado
                mecanico.Estado = false; // Asignar un valor predeterminado si es necesario
                mecanicoCollection.InsertOne(mecanico);

                // Redirigir o mostrar un mensaje de éxito
                ViewBag.RegistroExitoso = true;
                return RedirectToAction("GestionarMecanicos");
            }
            return View(mecanico);
        }

        // Acción para manejar la edición del mecánico
        [HttpPost]
        public ActionResult Editar(Mecanicos mecanico)
        {
            if (ModelState.IsValid)
            {
                var filter = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, mecanico.NumeroDocumento);
                var update = Builders<Mecanicos>.Update
                    .Set(m => m.Nombre, mecanico.Nombre)
                    .Set(m => m.Contraseña, mecanico.Contraseña)
                    .Set(m => m.Estado, mecanico.Estado)
                    .Set(m => m.Contraseña, mecanico.Contraseña);

                var result = mecanicoCollection.UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return RedirectToAction("GestionarMecanicos");
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo actualizar el mecánico. Verifique los datos e intente nuevamente.");
                }
            }
            return View(mecanico);
        }

        // Acción para eliminar un mecánico
        public ActionResult Eliminar(int id)
        {
            mecanicoCollection.DeleteOne(m => m.NumeroDocumento == id);
            return RedirectToAction("GestionarMecanicos");
        }

        public ActionResult ActualizarImagenes()
        {
            var mecanicos = mecanicoCollection.Find(m => true).ToList();

            foreach (var mecanico in mecanicos)
            {
                // Genera la URL de la imagen en función del NumeroDocumento
                mecanico.ImagenUrl = ObtenerImagenUrlParaMecanico(mecanico);

                // Actualiza el documento en la base de datos
                var filter = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, mecanico.NumeroDocumento);
                var update = Builders<Mecanicos>.Update.Set(m => m.ImagenUrl, mecanico.ImagenUrl);
                mecanicoCollection.UpdateOne(filter, update);
            }

            return RedirectToAction("GestionarMecanicos");
        }


        private string ObtenerImagenUrlParaMecanico(Mecanicos mecanico)
        {
            return Url.Content($"~/Content/Images/{mecanico.NumeroDocumento}.jpg");
        }

        public ActionResult GestionarVehiculos()
        {
            var vehiculos = vehiculoCollection.Find(v => true).ToList();
            return View(vehiculos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarVehiculos(Vehiculos vehiculos)
        {
            if (ModelState.IsValid)
            {
                vehiculoCollection.InsertOne(vehiculos);
                return RedirectToAction("GestionarVehiculos");
            }
            return View(vehiculos);
        }

        public JsonResult BuscarCliente(int idCliente)
        {
            // Ajusta el nombre de la colección y la búsqueda según tu implementación
            var cliente = clientesCollection.Find(c => c.IdCliente == idCliente).FirstOrDefault();
            if (cliente != null)
            {
                return Json(new { nombre = cliente.Nombre }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { nombre = "Cliente no encontrado" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AsistenciaMecanico()
        {
            return View();
        }

        public ActionResult Generar_QR()
        {
            string qrContent = "https://10.180.145.194:45455/Mecanico/RegistrarAsistencia";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(10);

            return File(qrCodeImage, "image/png");
        }
    }
}