using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using CapaEntidad;

namespace Proyecto_AWWS.Controllers
{
    public class InicioController : Controller
    {
        private readonly IMongoCollection<Mecanicos> mecanicoCollection;
        private readonly IMongoCollection<Vehiculos> vehiculoCollection;


        public InicioController()
        {
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            var database = client.GetDatabase("AWWS");

            mecanicoCollection = database.GetCollection<Mecanicos>("Mecanicos");

            vehiculoCollection = database.GetCollection<Vehiculos>("Vehiculos");
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
            return View();
        }

        // Acción para mostrar el formulario de gestion de citas
        public ActionResult GestionarCitas()
        {
            return View();
        }

        // Acción para manejar el registro del mecánico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(Mecanicos mecanicos)
        {
            if (ModelState.IsValid)
            {
                mecanicoCollection.InsertOne(mecanicos);
                return RedirectToAction("GestionarMecanicos");
            }
            return View(mecanicos);
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
                    .Set(m => m.Especialidad, mecanico.Especialidad)
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
    }
}