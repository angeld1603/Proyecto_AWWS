using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using CapaEntidad;
using System.Threading.Tasks;

namespace Proyecto_AWWS.Controllers
{
    public class LoginController : Controller
    {   
        private readonly IMongoCollection<Usuarios> usuarios;
        private readonly IMongoCollection<Mecanicos> mecanicos;
        private readonly IMongoCollection<Clientes> clientes;

        public LoginController()
        {
            //Cliente mongo con la cadena de conexion
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");    

            //Obtener base de datos
            var database = client.GetDatabase("AWWS");

            //Obtener la colección con la informacion de los usuarios
            usuarios = database.GetCollection<Usuarios>("Usuarios");

            mecanicos = database.GetCollection<Mecanicos>("Mecanicos");

            clientes = database.GetCollection<Clientes>("Clientes");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string idUsuario, string Contraseña)
        {
            // Primero buscar en la tabla de usuarios
            var usuario = usuarios.Find(u => u.idUsuario.ToString() == idUsuario && u.Contraseña == Contraseña).FirstOrDefault();

            if (usuario != null)
            {
                Session["Nombre"] = usuario.Nombre.Split()[0];
                //Session["Rol"] = "Usuario"; // Opcional, para diferenciar el rol
                return RedirectToAction("Inicio", "Inicio");
            }

            // Buscar en la tabla de Mecánicos si no se encontró en la tabla de usuarios
            var mecanico = mecanicos.Find(m => m.NumeroDocumento.ToString() == idUsuario && m.Contraseña == Contraseña).FirstOrDefault();

            if (mecanico != null)
            {
                Session["NumeroDocumento"] = mecanico.NumeroDocumento;
                //Session["Rol"] = "Mecanico"; // Opcional, para diferenciar el rol
                return RedirectToAction("Inicio", "Mecanico");
            }

            // Buscar en la tabla de Clientes si no se encontró en la tabla de usuarios ni en la de Mecánicos
            var cliente = clientes.Find(c => c.IdCliente.ToString() == idUsuario && c.Contraseña == Contraseña).FirstOrDefault();

            if (cliente != null)
            {
                Session["Nombre"] = cliente.Nombre.Split()[0];
                Session["IdCliente"] = cliente.IdCliente;
                return RedirectToAction("Inicio", "Cliente");
            }

            // Si las credenciales no son válidas, mostrar un mensaje de error
            TempData["Error"] = "Credenciales inválidas.";
            return RedirectToAction("Login");
        }

        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(string idCliente, string nombre, string contraseña, string numeroTelefono, string direccion)
        {
            // Verifica si el ID del cliente ya existe en la base de datos
            var clienteExistente = await clientes.Find(c => c.IdCliente == Convert.ToInt32(idCliente)).FirstOrDefaultAsync();

            if (clienteExistente != null)
            {
                // Si el cliente ya existe, guarda el mensaje de error en TempData
                TempData["ErrorMessage"] = "El ID del cliente ya está registrado.";
                return RedirectToAction("Registro", "Login"); // Redirige a la vista de registro
            }

            // Crea un nuevo cliente y lo inserta en la base de datos
            var nuevoCliente = new Clientes
            {
                IdCliente = Convert.ToInt32(idCliente),
                Nombre = nombre,
                Contraseña = contraseña,
                NumeroTelefono = Convert.ToInt64(numeroTelefono),
                Direccion = direccion
            };

            await clientes.InsertOneAsync(nuevoCliente);

            // Mensaje de éxito
            return RedirectToAction("Login", "Login"); // Redirige al login
        }


        public ActionResult ReestablecerContraseña()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ReestablecerContraseña(string idCliente, string contraseña)
        {
            // Busca el cliente por su ID
            var cliente = await clientes.Find(c => c.IdCliente == Convert.ToInt32(idCliente)).FirstOrDefaultAsync();

            if (cliente != null)
            {
                // Actualiza la contraseña
                var update = Builders<Clientes>.Update.Set("Contraseña", contraseña);
                await clientes.UpdateOneAsync(c => c.IdCliente == cliente.IdCliente, update);

                // Mensaje de éxito
                TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
            }
            else
            {
                // Mensaje de error
                TempData["ErrorMessage"] = "El ID ingresado es incorrecto.";
            }

            return RedirectToAction("ReestablecerContraseña", "Login");
        }
    }
}