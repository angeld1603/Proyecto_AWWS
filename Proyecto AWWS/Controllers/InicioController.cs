using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using CapaEntidad;
using QRCoder;
using MongoDB.Bson;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;


namespace Proyecto_AWWS.Controllers
{
    public class InicioController : Controller
    {
        // Colecciones para manejar los datos de cada una de ellas
        private readonly IMongoCollection<Mecanicos> mecanicoCollection;
        private readonly IMongoCollection<Vehiculos> vehiculoCollection;
        private readonly IMongoCollection<Reparacion> reparacionCollection;
        private readonly IMongoCollection<Clientes> clientesCollection;
        private readonly IMongoCollection<Citas> citasCollection;
        private readonly IMongoCollection<Notificaciones> notificacionesCollection;
        private readonly IMongoCollection<Asistencia> asistenciaCollection;


        public InicioController()
        {
            //inicializa el cliente de MongoDB con la cadena de conexion
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            // obtiene la base de datos del cliente MongoDB
            var database = client.GetDatabase("AWWS");

            // se obtienen las colecciones (tablas) de la base de datos
            mecanicoCollection = database.GetCollection<Mecanicos>("Mecanicos");
            vehiculoCollection = database.GetCollection<Vehiculos>("Vehiculos"); 
            reparacionCollection = database.GetCollection<Reparacion>("Reparacion");
            clientesCollection = database.GetCollection<Clientes>("Clientes"); 
            citasCollection = database.GetCollection<Citas>("Citas"); 
            notificacionesCollection = database.GetCollection<Notificaciones>("Notificaciones"); 
            asistenciaCollection = database.GetCollection<Asistencia>("Asistencia"); 

            // configura el contexto de licencia para la biblioteca de ExcelPackage utilizada para generar los reportes en Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
        }

        // accion para mostrar el formulario de inicio
        public ActionResult Inicio()
        {
            return View();
        }

        // accion para mostrar la lista de mecánicos 
        public ActionResult GestionarMecanicos()
        {
            var mecanicos = mecanicoCollection.Find(m => true).ToList();
            return View(mecanicos);
        }

        // accion para mostrar el formulario de registro de mecanicos
        public ActionResult Registrar()
        {
            return View();
        }

        // accion para mostrar el formulario de edición de mecanicos
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
            // Verifica si el archivo no es nulo y tiene contenido
            if (archivo != null && archivo.ContentLength > 0)
            {
                var fileName = $"{numeroDocumento}.jpg";// Genera el nombre del archivo basado en el número de documento del mecánico
                var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);// Combina el nombre del archivo con la ruta de almacenamiento en el servidor

                archivo.SaveAs(path);

                // Busca el mecánico en la base de datos por número de documento
                var mecanico = mecanicoCollection.Find(m => m.NumeroDocumento == numeroDocumento).FirstOrDefault();
                if (mecanico != null)
                {
                    mecanico.ImagenUrl = $"/Content/Images/{fileName}"; // Actualiza la URL de la imagen del mecánico
                    var filter = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, numeroDocumento);// Crea un filtro para encontrar el mecánico específico en la base de datos
                    var update = Builders<Mecanicos>.Update.Set(m => m.ImagenUrl, mecanico.ImagenUrl);// Crea una actualización para establecer la nueva URL de la imagen
                    mecanicoCollection.UpdateOne(filter, update);
                }
            }

            return RedirectToAction("GestionarMecanicos");
        }

        // accion para mostrar el formulario de registro de vehiculos
        public ActionResult RegistrarVehiculos()
        {
            return View();
        }

        // accion para mostrar el formulario de edicion de vehiculos
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

        // accion para eliminar un mecánico
        public ActionResult EliminarVehiculos(string placa)
        {
            vehiculoCollection.DeleteOne(v => v.placa == placa);
            return RedirectToAction("GestionarVehiculos");
        }

        // accion para mostrar el formulario de gestion de reparaciones
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
            // Verifica si el parámetro placa es nulo o está vacío
            if (string.IsNullOrEmpty(placa))
            {
                // Devuelve un JSON con idCliente nulo si el parámetro placa no es válido
                return Json(new { idCliente = (string)null }, JsonRequestBehavior.AllowGet);
            }

            // Busca el vehículo en la base de datos usando la placa proporcionada
            var vehiculo = vehiculoCollection.Find(v => v.placa == placa).FirstOrDefault();
            if (vehiculo != null)
            {
                var cliente = clientesCollection.Find(c => c.IdCliente == vehiculo.IdCliente).FirstOrDefault(); // Si el vehículo es encontrado, busca el cliente asociado usando el IdCliente del vehículo

                return Json(new { idCliente = cliente?.IdCliente.ToString() ?? "Cliente no encontrado" }, JsonRequestBehavior.AllowGet); // Devuelve el idCliente del cliente encontrado o un mensaje si el cliente no es encontrado
            }

            // Si el vehículo no es encontrado, devuelve un mensaje indicando que el cliente no fue encontrado
            return Json(new { idCliente = "Cliente no encontrado" }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarR(Reparacion reparacion)
        {
            // Verifica si el modelo proporcionado es válido
            if (ModelState.IsValid)
            {
                // Busca al cliente asociado con el ID proporcionado en la reparación
                var cliente = clientesCollection.Find(c => c.IdCliente == reparacion.IdCliente).FirstOrDefault();
                if (cliente == null)
                {
                    // Si el cliente no se encuentra, agrega un error al modelo y vuelve a mostrar la vista
                    ModelState.AddModelError("", "Cliente no encontrado.");
                    return View(reparacion);
                }

                // Busca el vehículo asociado con la placa y el ID del cliente proporcionado
                var vehiculo = vehiculoCollection.Find(v => v.placa == reparacion.placa && v.IdCliente == reparacion.IdCliente).FirstOrDefault();
                if (vehiculo == null)
                {
                    ModelState.AddModelError("", "Vehículo no encontrado o no pertenece al cliente seleccionado.");
                    return View(reparacion);
                }

                // Busca al mecánico asociado con el número de documento proporcionado y verifica si está activo
                var mecanico = mecanicoCollection.Find(m => m.NumeroDocumento == reparacion.NumeroDocumento && m.Estado).FirstOrDefault();
                if (mecanico == null)
                {
                    ModelState.AddModelError("", "Mecánico no activo o no encontrado.");
                    return View(reparacion);
                }

                // Inserta el nuevo documento de reparación en la bd
                reparacionCollection.InsertOne(reparacion);

                // Establece TempData para indicar que el registro fue exitoso
                TempData["RegistroExitoso"] = true;

                return RedirectToAction("RegistrarReparacion");
            }

            // se obtienen los datos de la lista de clientes y la pasa a la vista para el dropdown
            ViewBag.Clientes = new SelectList(clientesCollection.Find(c => true).ToList(), "IdCliente", "Nombre", reparacion.IdCliente);
            ViewBag.Vehiculos = new SelectList(vehiculoCollection.Find(v => v.IdCliente == reparacion.IdCliente).ToList(), "placa", "placa", reparacion.placa);
            ViewBag.Mecanicos = new SelectList(mecanicoCollection.Find(m => m.Estado).ToList(), "NumeroDocumento", "Nombre", reparacion.NumeroDocumento);

            return View(reparacion);
        }


        public JsonResult ObtenerClientes(string filtro)
        {
            // Busca clientes cuyo nombre contenga el filtro proporcionado
            var clientes = clientesCollection.Find(c => c.Nombre.Contains(filtro)).ToList();

            // Devuelve los clientes encontrados en formato JSON
            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerVehiculosPorCliente(int clienteId)
        {
            // Busca vehículos que pertenezcan al cliente con el ID proporcionado
            var vehiculos = vehiculoCollection.Find(v => v.IdCliente == clienteId).ToList();

            // Devuelve los vehículos encontrados en formato JSON
            return Json(vehiculos, JsonRequestBehavior.AllowGet);
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

        // accion para manejar la edición del mecánico
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

        // accion para eliminar un mecánico
        public ActionResult Eliminar(int id)
        {
            mecanicoCollection.DeleteOne(m => m.NumeroDocumento == id);
            return RedirectToAction("GestionarMecanicos");
        }

        // accion para actualizar la URL de la imagen de todos los mecánicos en la base de datos
        public ActionResult ActualizarImagenes()
        {
            // Recupera todos los mecánicos de la colección
            var mecanicos = mecanicoCollection.Find(m => true).ToList();

            foreach (var mecanico in mecanicos)
            {
                // Genera la URL de la imagen en función del NumeroDocumento del mecánico
                mecanico.ImagenUrl = ObtenerImagenUrlParaMecanico(mecanico);

                // Crea un filtro para encontrar el documento del mecánico basado en su NumeroDocumento
                var filter = Builders<Mecanicos>.Filter.Eq(m => m.NumeroDocumento, mecanico.NumeroDocumento);

                // Define la actualización para establecer la nueva URL de la imagen
                var update = Builders<Mecanicos>.Update.Set(m => m.ImagenUrl, mecanico.ImagenUrl);

                // Aplica la actualización en la base de datos
                mecanicoCollection.UpdateOne(filter, update);
            }

            // Redirige a la vista de gestión de mecánicos después de la actualización
            return RedirectToAction("GestionarMecanicos");
        }


        private string ObtenerImagenUrlParaMecanico(Mecanicos mecanico)
        {
            // La URL se basa en la ruta local de las imágenes en el servidor
            return Url.Content($"~/Content/Images/{mecanico.NumeroDocumento}.jpg");
        }

        // accion del controlador para gestionar vehículos
        public ActionResult GestionarVehiculos()
        {
            var vehiculos = vehiculoCollection.Find(v => true).ToList();
            return View(vehiculos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarVehiculos(Vehiculos vehiculos)
        {
            // Verifica que el modelo recibido es válido
            if (ModelState.IsValid)
            {
                // Inserta el nuevo vehículo en la colección de vehículos
                vehiculoCollection.InsertOne(vehiculos);

                // Establece una bandera en TempData para indicar éxito en el registro
                TempData["RegistroExitoso"] = true;

                // Redirige a la acción de registro de vehículos para mostrar el mensaje
                return RedirectToAction("RegistrarVehiculos");
            }

            // Si el modelo no es válido, vuelve a mostrar el formulario con los datos ingresados
            return View(vehiculos);
        }

        public JsonResult BuscarCliente(int idCliente)
        {
            // Busca el cliente en la colección de clientes usando el ID proporcionado
            var cliente = clientesCollection.Find(c => c.IdCliente == idCliente).FirstOrDefault();

            // Si el cliente es encontrado, devuelve el nombre del cliente
            if (cliente != null)
            {
                return Json(new { nombre = cliente.Nombre }, JsonRequestBehavior.AllowGet);
            }
            // Si el cliente no es encontrado, devuelve un mensaje indicando esto
            else
            {
                return Json(new { nombre = "Cliente no encontrado" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AsistenciaMecanico()
        {
            return View();
        }

        // Acción del controlador para generar un código QR
        public ActionResult Generar_QR()
        {
            // Define el contenido del código QR, en este caso el url de la vista donde se encuentra el controlador
            string qrContent = "https://10.180.145.194:45455/Mecanico/RegistrarAsistencia";

            // Crea una instancia de QRCodeGenerator para generar datos de código QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            // Genera los datos del código QR con el contenido especificado 
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);

            // Crea una instancia de BitmapByteQRCode para convertir los datos en una imagen de código QR
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

            // Obtiene la imagen del código QR como un arreglo de bytes con un tamaño de 10 (tamaño de la imagen)
            byte[] qrCodeImage = qrCode.GetGraphic(10);

            // Devuelve la imagen del código QR como un archivo de tipo "image/png"
            return File(qrCodeImage, "image/png");
        }


        // Acción para mostrar el formulario de gestion de citas
        public ActionResult GestionarCitas()
        {
            // Obtener citas con estados "Pendiente" o "Confirmada"
            var estados = new[] { "Pendiente"};
            var citas = citasCollection.Find(c => estados.Contains(c.Estado)).ToList();

            return View(citas);
        }

       
        public ActionResult VerNotificaciones()
        {
            // Obtiene todas las notificaciones que no han sido leídas
            var notificaciones = notificacionesCollection.Find(n => !n.Leida).ToList();

            // Pasa la lista de notificaciones a la vista para su visualización
            return View(notificaciones);
        }

        [HttpPost]
        public ActionResult MarcarComoLeida(string id)
        {
            // Convertir el ID de string a ObjectId
            if (!ObjectId.TryParse(id, out var notificacionId))
            {
                TempData["ErrorMessage"] = "ID de notificación inválido.";
                return RedirectToAction("GestionarCitas"); // Ajustar según la vista que quieras mostrar después
            }

            // Crear el update para marcar la notificación como leída
            var update = Builders<Notificaciones>.Update.Set(n => n.Leida, true);

            // Actualizar la notificación en la base de datos
            var resultado = notificacionesCollection.UpdateOne(n => n.Id == notificacionId.ToString(), update);

            if (resultado.ModifiedCount > 0)
            {
                TempData["SuccessMessage"] = "Notificación marcada como leída.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo marcar la notificación como leída.";
            }

            return RedirectToAction("GestionarCitas"); // Ajustar según la vista que quieras mostrar después
        }

        [HttpPost]
        public ActionResult ConfirmarCita(string id)
        {
            // Convertir el ID de string a ObjectId
            if (!ObjectId.TryParse(id, out var citaId))
            {
                TempData["ErrorMessage"] = "ID de cita inválido.";
                return RedirectToAction("GestionarCitas");
            }

            // Crear el update para cambiar el estado de la cita a "Confirmada"
            var update = Builders<Citas>.Update.Set(c => c.Estado, "Confirmada");

            // Actualizar la cita en la base de datos
            var resultado = citasCollection.UpdateOne(c => c.Id == citaId.ToString(), update);

            if (resultado.ModifiedCount > 0)
            {
                TempData["SuccessMessage"] = "Cita confirmada exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo confirmar la cita.";
            }

            return RedirectToAction("GestionarCitas");
        }

        public JsonResult VerificarNuevasNotificaciones()
        {
            // Cuenta el número de notificaciones que no han sido leídas
            var notificacionesNoLeidas = notificacionesCollection.CountDocuments(n => !n.Leida);

            // Crea una respuesta que indica si hay nuevas notificaciones y su cantidad
            var response = new
            {
                hasNewNotifications = notificacionesNoLeidas > 0, // Indica si hay notificaciones nuevas
                count = notificacionesNoLeidas // Número de notificaciones no leídas
            };

            // Devuelve la respuesta en formato JSON
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GestionarReportes()
        {
            return View();
        }

        public JsonResult GenerarReporte(string tipo, DateTime? fecha)
        {
            if (tipo == "Asistencias")
            {
                // Crea un filtro vacío por defecto
                var filter = Builders<Asistencia>.Filter.Empty;

                // Si se ha proporcionado una fecha, ajusta el filtro para esa fecha
                if (fecha.HasValue)
                {
                    var startDate = fecha.Value.Date;
                    var endDate = startDate.AddDays(1); // Fin del día para la comparación

                    filter = Builders<Asistencia>.Filter.And(
                        Builders<Asistencia>.Filter.Gte(a => a.FechaEntrada, startDate),
                        Builders<Asistencia>.Filter.Lt(a => a.FechaEntrada, endDate)
                    );
                }

                // Obtiene las asistencias que coinciden con el filtro
                var asistencias = asistenciaCollection.Find(filter).ToList();

                // Borra los datos de las asistencias para la respuesta JSON
                var resultadoAsistencias = asistencias.Select(a => new
                {
                    a.NumeroDocumento,
                    a.Nombre,
                    FechaEntrada = a.FechaEntrada.ToString("yyyy-MM-dd HH:mm:ss"),
                    FechaSalida = a.FechaSalida.HasValue ? a.FechaSalida.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A"
                }).ToList();

                return Json(resultadoAsistencias, JsonRequestBehavior.AllowGet);
            }
            else if (tipo == "Reparaciones")
            {
                var filter = Builders<Reparacion>.Filter.Empty;

                if (fecha.HasValue)
                {
                    var startDate = fecha.Value.Date;
                    var endDate = startDate.AddDays(1); // Fin del día para la comparación

                    filter = Builders<Reparacion>.Filter.And(
                        Builders<Reparacion>.Filter.Gte(r => r.FechaIngreso, startDate),
                        Builders<Reparacion>.Filter.Lt(r => r.FechaIngreso, endDate)
                    );
                }

                // Obtiene las reparaciones que coinciden con el filtro
                var reparaciones = reparacionCollection.Find(filter).ToList();

                // Borra los datos de las reparaciones para la respuesta JSON
                var resultadoReparaciones = reparaciones.Select(r => new {
                    r.Descripcion,
                    FechaIngreso = r.FechaIngreso.ToString("yyyy-MM-dd"),
                    FechaEntregaPrevista = r.FechaEntregaPrevista.HasValue ? r.FechaEntregaPrevista.Value.ToString("yyyy-MM-dd") : "N/A",
                    r.Estado,
                    r.IdCliente,
                    r.placa,
                    r.NumeroDocumento
                }).ToList();

                return Json(resultadoReparaciones, JsonRequestBehavior.AllowGet);
            }

            // Si el tipo de reporte no es válido, devuelve un mensaje de error
            return Json(new { mensaje = "Tipo de reporte no válido" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarReporteExcel(string tipo, DateTime? fecha)
        {
            using (var package = new ExcelPackage()) // Crea una instancia de ExcelPackage para manejar el archivo Excel
            {
                ExcelWorksheet worksheet; // Variable para la hoja de cálculo
                string[] headers; // arreglo para los encabezados de las columnas

                // Verifica el tipo de reporte solicitado
                if (tipo == "Reparaciones")
                {
                    headers = new[] { "Placa", "Cliente", "Mecánico", "Fecha Ingreso", "Fecha Entrega Prevista", "Descripción" }; // Encabezados para el reporte de reparaciones
                    worksheet = package.Workbook.Worksheets.Add("Reparaciones"); // Añade una nueva hoja de cálculo para reparaciones

                    // Obtiene todos los documentos de reparaciones
                    var reparaciones = reparacionCollection.Find(r => true).ToList();

                    // Añade encabezados a la primera fila
                    for (int i = 0; i < headers.Length; i++)
                    {
                        // Estilo de las columnas en excel
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Añade los datos de reparaciones a las filas siguientes
                    int row = 2;
                    foreach (var reparacion in reparaciones)
                    {
                        worksheet.Cells[row, 1].Value = reparacion.placa;
                        worksheet.Cells[row, 2].Value = clientesCollection.Find(c => c.IdCliente == reparacion.IdCliente).FirstOrDefault()?.Nombre;
                        worksheet.Cells[row, 3].Value = mecanicoCollection.Find(m => m.NumeroDocumento == reparacion.NumeroDocumento).FirstOrDefault()?.Nombre;
                        worksheet.Cells[row, 4].Value = reparacion.FechaIngreso.ToShortDateString();
                        worksheet.Cells[row, 5].Value = reparacion.FechaEntregaPrevista?.ToShortDateString() ?? "N/A";
                        worksheet.Cells[row, 6].Value = reparacion.Descripcion;

                        // Añade borde a todas las celdas de la fila (estilo del documento)
                        for (int col = 1; col <= headers.Length; col++)
                        {
                            worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        row++;
                    }
                }


                else if (tipo == "Asistencias")
                {
                    headers = new[] { "Número Documento", "Nombre", "Fecha Entrada", "Fecha Salida" }; // Encabezado para el reporte de asistencias
                    worksheet = package.Workbook.Worksheets.Add("Asistencias");  // Añade una nueva hoja de cálculo para asistencias

                    // Obtiene todos los documentos de asistencias
                    var asistencias = asistenciaCollection.Find(a => true).ToList();

                    // Añade encabezados a la primera fila
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Añade los datos de asistencias a las filas siguientes
                    int row = 2;
                    foreach (var asistencia in asistencias)
                    {
                        worksheet.Cells[row, 1].Value = asistencia.NumeroDocumento;
                        worksheet.Cells[row, 2].Value = asistencia.Nombre;
                        worksheet.Cells[row, 3].Value = asistencia.FechaEntrada.ToShortDateString();
                        worksheet.Cells[row, 4].Value = asistencia.FechaSalida?.ToShortDateString() ?? "N/A";

                        // Añade borde a todas las celdas de la fila (estilo del documento)
                        for (int col = 1; col <= headers.Length; col++)
                        {
                            worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        row++;
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Tipo de reporte no válido.");
                }

                // Ajusta automáticamente el tamaño de las columnas
                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Genera el nombre del archivo con la fecha actual
                string fileName = $"{tipo}_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}