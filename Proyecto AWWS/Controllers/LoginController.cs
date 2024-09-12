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
            // inicia el cliente de MongoDB con la cadena de conexión
            var client = new MongoClient("mongodb+srv://admin:zNG8KfdyNPLA44XZ@angeldior.53t301e.mongodb.net/");

            // se obtiene la base de datos
            var database = client.GetDatabase("AWWS");

            // se obtienen las colecciones (tablas) de la base de datos
            usuarios = database.GetCollection<Usuarios>("Usuarios");
            mecanicos = database.GetCollection<Mecanicos>("Mecanicos");
            clientes = database.GetCollection<Clientes>("Clientes");
        }

        // accion que muestra la vista de login
        public ActionResult Login()
        {
            return View();
        }

        // accion que maneja el inicio de sesión del usuario
        [HttpPost]
        public ActionResult Login(string idUsuario, string Contraseña)
        {
            // busca al usuario en la colección de usuarios
            var usuario = usuarios.Find(u => u.idUsuario.ToString() == idUsuario && u.Contraseña == Contraseña).FirstOrDefault();

            if (usuario != null)
            {
                // si el usuario es encontrado, guarda su nombre en la sesion y envia a la vista de inicio del usuario
                Session["Nombre"] = usuario.Nombre.Split()[0];
                return RedirectToAction("Inicio", "Inicio");
            }

            // si no se encuentra el usuario, busca en la coleccion de mecanicos
            var mecanico = mecanicos.Find(m => m.NumeroDocumento.ToString() == idUsuario && m.Contraseña == Contraseña).FirstOrDefault();

            if (mecanico != null)
            {
                // si el mecanico es encontrado, guarda su numero de documento en la sesion y envia a la vista de inicio del mecanico
                Session["NumeroDocumento"] = mecanico.NumeroDocumento;
                return RedirectToAction("Inicio", "Mecanico");
            }

            // si no se encuentra el mecanico, busca en la coleccion de clientes
            var cliente = clientes.Find(c => c.IdCliente.ToString() == idUsuario && c.Contraseña == Contraseña).FirstOrDefault();

            if (cliente != null)
            {
                // si el cliente es encontrado, guarda su nombre e ID en la sesion y envia a la vista de inicio del cliente
                Session["Nombre"] = cliente.Nombre.Split()[0];
                Session["IdCliente"] = cliente.IdCliente;
                return RedirectToAction("Inicio", "Cliente");
            }

            // si no se encuentran credenciales validas, muestra un mensaje de error y envia al login
            TempData["Error"] = "Credenciales inválidas.";
            return RedirectToAction("Login");
        }

        // accion que muestra la vista de registro
        public ActionResult Registro()
        {
            return View();
        }

        // accion que maneja el registro de un nuevo cliente
        [HttpPost]
        public async Task<ActionResult> Registrar(string idCliente, string nombre, string contraseña, string numeroTelefono, string direccion)
        {
            // verifica si el ID del cliente ya existe en la base de datos
            var clienteExistente = await clientes.Find(c => c.IdCliente == Convert.ToInt32(idCliente)).FirstOrDefaultAsync();

            if (clienteExistente != null)
            {
                // si el cliente ya existe, muestra un mensaje de error
                TempData["ErrorMessage"] = "El ID del cliente ya está registrado.";
                return RedirectToAction("Registro", "Login");
            }

            // crea un nuevo cliente y lo inserta en la base de datos
            var nuevoCliente = new Clientes
            {
                IdCliente = Convert.ToInt32(idCliente),
                Nombre = nombre,
                Contraseña = contraseña,
                NumeroTelefono = Convert.ToInt64(numeroTelefono),
                Direccion = direccion
            };

            await clientes.InsertOneAsync(nuevoCliente);

            // muestra un mensaje de exito y redirige al login
            TempData["SuccessMessage"] = "Registro exitoso. Por favor inicie sesión.";
            return RedirectToAction("Login", "Login");
        }

        // accion que muestra la vista para restablecer la contraseña
        public ActionResult ReestablecerContraseña()
        {
            return View();
        }

        // accion que maneja el restablecimiento de la contraseña del cliente
        [HttpPost]
        public async Task<ActionResult> ReestablecerContraseña(string idCliente, string contraseña)
        {
            // Busca el cliente por su ID
            var cliente = await clientes.Find(c => c.IdCliente == Convert.ToInt32(idCliente)).FirstOrDefaultAsync();

            if (cliente != null)
            {
                // Si el cliente es encontrado, actualiza la contraseña
                var update = Builders<Clientes>.Update.Set("Contraseña", contraseña);
                await clientes.UpdateOneAsync(c => c.IdCliente == cliente.IdCliente, update);

                // Muestra un mensaje de éxito
                TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
            }
            else
            {
                // Si el cliente no es encontrado, muestra un mensaje de error
                TempData["ErrorMessage"] = "El ID ingresado es incorrecto.";
            }

            return RedirectToAction("ReestablecerContraseña", "Login");
        }
    }
}
