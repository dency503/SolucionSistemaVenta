﻿using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _fireBaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;
        public UsuarioService(
         IGenericRepository<Usuario> repositorio,
         IFireBaseService fireBaseService,
         IUtilidadesService utilidadesService,
         ICorreoService correoService
         )
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }

        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario
               == IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");
                if (usuario_encontrado.Clave !=
               _utilidadesService.ConvertirSHA256(ClaveActual))
                    throw new TaskCanceledException("La contraseña ingresada como actual no es correcta");

                usuario_encontrado.Clave = _utilidadesService.ConvertirSHA256(ClaveNueva);
                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo ==
 entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (usuario_existe != null)
                throw new TaskCanceledException("El corrreo ya existe");
            try
            {
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = clave_generada;
                entidad.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }
                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if (usuario_creado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");
                }
                if (UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", usuario_creado.Correo).Replace("[clave]", clave_generada);
                    string htmlCorreo = "";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;
                            if (response.CharacterSet == null)

                                readerStream = new StreamReader(dataStream);


                            else
                                readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                            htmlCorreo = readerStream.ReadToEnd();
                            response.Close(); readerStream.Close();
                        }

                    }
                    if (htmlCorreo != "")
                        await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta Creada", htmlCorreo);
                }
                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).First();
                return usuario_creado;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo ==
entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (usuario_existe != null)
                throw new TaskCanceledException("El corrreo ya existe");
            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u =>
               u.IdUsuario == entidad.IdUsuario);
                Usuario usuario_editar = queryUsuario.First();
                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;
                if (usuario_editar.NombreFoto == "")
                    usuario_editar.NombreFoto = NombreFoto;
                if (Foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto,
                   "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }
                bool respuesta = await _repositorio.Editar(entidad);
                if (respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario");
                Usuario usuario_editado = queryUsuario.Include(r =>
               r.IdRolNavigation).First();
                return usuario_editado;
            }
            catch
            {
                throw;
            }

        }

        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario ==
               IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");
                string nombreFoto = usuario_encontrado.NombreFoto;
                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);
                if (respuesta)
                    await _fireBaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                return true;
            }
            catch
            {
                throw;
            }

        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario ==
               entidad.IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");
                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;
                bool respuesta = await _repositorio.Editar(usuario_encontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSHA256(clave);
            Usuario usuario_encontrado = await _repositorio.Obtener(u =>
            u.Correo.Equals(correo) && u.Clave.Equals(clave_encriptada));
            return usuario_encontrado;
        }

        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario ==
 IdUsuario);
            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();
            return resultado;

        }

        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                // Find the user by email
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == Correo);

                if (usuario_encontrado == null)
                    throw new TaskCanceledException("No encontramos ningun usuario asociado al correo");

                // Generate a new random password
                string clave_generada = _utilidadesService.GenerarClave();
                usuario_encontrado.Clave = _utilidadesService.ConvertirSHA256(clave_generada);

                // Replace [clave] placeholder in the email template URL with the generated password
                string urlCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);

                // Fetch the email template content
                string htmlCorreo = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(urlCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;
                        if(response.CharacterSet == null)
                            readerStream = new StreamReader(dataStream);
                        else readerStream = new StreamReader(dataStream,Encoding.GetEncoding(response.CharacterSet));
                         
                        htmlCorreo = readerStream.ReadToEnd();
                        readerStream.Close();
                        response.Close();
                    }
                }
                bool correo_enviado = false;
                if (htmlCorreo != null)
                await _correoService.EnviarCorreo(usuario_encontrado.Correo, "Contraseña restablecida", htmlCorreo);
                if(correo_enviado)
                    throw new TaskCanceledException("Tenemos problemas porfavor intentelo mas tarde");
                

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
