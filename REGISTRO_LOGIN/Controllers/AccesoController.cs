using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using REGISTRO_LOGIN.Models;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using System.Web.Services.Description;
using REGISTRO_LOGIN.Permisos;

namespace REGISTRO_LOGIN.Controllers
{
    
    public class AccesoController : Controller
    {
        static string cadena = "Data Source=DESKTOP-61VL0KL; Initial Catalog=DB_Acceso; Integrated Security=true";

        // GET: Acceso
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Registrar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registrar(Usuario oUsuario)
        {
            bool registrado;
            string mensaje;

            if (oUsuario.Clave == oUsuario.ConfirmarClave)
            {
                oUsuario.Clave = ConvertirSha256(oUsuario.Clave);
            }
            else
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_registrarUsuario",cn) ;
                cmd.Parameters.AddWithValue("correo",oUsuario.Correo);
                cmd.Parameters.AddWithValue("clave",oUsuario.Clave);

                cmd.Parameters.Add("registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("mensaje", SqlDbType.VarChar,100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                cmd.ExecuteNonQuery();

                registrado = Convert.ToBoolean(cmd.Parameters["registrado"].Value);
                mensaje = cmd.Parameters["mensaje"].Value.ToString();
            }

            ViewData["Mensaje"] = mensaje;
            if (registrado)
            {
                return RedirectToAction("Login", "Acceso");
            }
            else
            {
                return View();
            }
            
        }
        [HttpPost]
        public ActionResult Login(Usuario oUsuario)
        {
            oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_validarUsuario", cn);
                cmd.Parameters.AddWithValue("correo", oUsuario.Correo);
                cmd.Parameters.AddWithValue("clave", oUsuario.Clave);
                cmd.CommandType = CommandType.StoredProcedure;

                
                cn.Open();

                oUsuario.idUsuario = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                
            }
            if (oUsuario.idUsuario != 0)
            {
                Session["usuario"] = oUsuario;
                return RedirectToAction("Index", "Home");

            }
            else
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }

            
        }
        public static string ConvertirSha256(string texto)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

    }
}