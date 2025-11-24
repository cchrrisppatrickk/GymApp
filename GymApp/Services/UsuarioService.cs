using GymApp.Models;
using GymApp.Repositories;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net; // Asegúrate de tener instalado BCrypt.Net-Next

namespace GymApp.Services
{
    public class UsuarioService : IUsuarioService
    {
        // Inyectamos el Repositorio, no el DbContext directamente.
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task<Usuario> ObtenerPorIdAsync(int id)
        {
            return await _usuarioRepository.ObtenerConDetallesAsync(id);
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario, string passwordRaw)
        {
            // 1. Regla de Negocio: Validar que el DNI no exista
            if (await _usuarioRepository.ExisteDniAsync(usuario.Dni))
            {
                throw new Exception("El DNI ya está registrado en el sistema.");
            }

            // 2. Regla de Seguridad: Hashear contraseña
            // Nunca guardamos passwordRaw. Usamos BCrypt con un WorkFactor de 11.
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordRaw);

            // 3. Regla de Negocio: Generar Token QR único
            usuario.CodigoQr = Guid.NewGuid();

            // 4. Datos de Auditoría
            usuario.FechaRegistro = DateTime.Now;
            usuario.Estado = true; // Activo por defecto

            // 5. Persistencia
            await _usuarioRepository.InsertAsync(usuario);
            await _usuarioRepository.SaveAsync();

            return usuario;
        }

        public async Task ActualizarUsuarioAsync(Usuario usuario)
        {
            // Aquí podrías agregar validaciones extra antes de guardar
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveAsync();
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            // Soft Delete: No borramos el registro, solo lo desactivamos (opcional)
            // Pero según tu repositorio genérico, tenemos Delete físico.
            // Vamos a implementarlo físico por ahora según el repo.
            await _usuarioRepository.DeleteAsync(id);
            await _usuarioRepository.SaveAsync();
            return true;
        }

        public async Task<Usuario> ValidarLoginAsync(string dni, string password)
        {
            var usuario = await _usuarioRepository.ObtenerPorDNIAsync(dni);

            if (usuario == null) return null;
            if (usuario.Estado == false) throw new Exception("Usuario inactivo.");

            // Verificamos si la contraseña coincide con el Hash
            bool esValido = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            return esValido ? usuario : null;
        }

        public byte[] GenerarImagenQR(Guid codigoQR)
        {
            // Usamos la librería QRCoder
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                // Creamos la data del QR basada en el GUID convertido a string
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(codigoQR.ToString(), QRCodeGenerator.ECCLevel.Q);

                // Renderizamos a Bytes (Formato PNG)
                // Usamos PngByteQRCodeHelper para compatibilidad cruzada (Linux/Windows)
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20); // 20 es el tamaño de píxel (resolución)

                return qrCodeImage;
            }
        }
    }
}