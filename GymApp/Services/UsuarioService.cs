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

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario, string? passwordRaw)
        {
            // 1. Validar DNI único (Se mantiene igual)
            if (await _usuarioRepository.ExisteDniAsync(usuario.Dni))
                throw new Exception("El DNI ya está registrado.");

            // --- LÓGICA DE AUTO-COMPLETADO (NUEVO) ---

            // A. Si no enviaron Nombre de Usuario, usamos el DNI
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            {
                usuario.NombreUsuario = usuario.Dni;
            }

            // B. Si no enviaron Password (registro rápido), usamos el DNI como contraseña
            string passwordFinal = passwordRaw;
            if (string.IsNullOrWhiteSpace(passwordFinal))
            {
                passwordFinal = usuario.Dni;
                // Opcional: Podrías usar una constante como "Gym2025!" si prefieres.
            }
            // ------------------------------------------

            // 2. Validar NombreUsuario único (Ahora validamos el autogenerado también)
            if (await _usuarioRepository.ExisteNombreUsuarioAsync(usuario.NombreUsuario))
                throw new Exception($"El usuario '{usuario.NombreUsuario}' ya está en uso.");

            // 3. Hashear Password (usamos la variable passwordFinal)
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordFinal);

            // 4. Datos automáticos
            usuario.CodigoQr = Guid.NewGuid();
            usuario.FechaRegistro = DateTime.Now;
            usuario.Estado = true;

            await _usuarioRepository.InsertAsync(usuario);
            await _usuarioRepository.SaveAsync();

            return usuario;
        }

        public async Task ActualizarUsuarioAsync(Usuario usuario)
        {
            // IMPORTANTE: Al editar, deberíamos validar que si cambió el nombre de usuario,
            // el nuevo no esté ocupado por otra persona. (Se puede refinar luego).
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

        public async Task<Usuario> ValidarLoginAsync(string inputLogin, string password)
        {
            Usuario usuario = null;

            // Intentamos buscar por DNI primero
            usuario = await _usuarioRepository.ObtenerPorDNIAsync(inputLogin);

            // Si no existe por DNI, buscamos por NombreUsuario
            if (usuario == null)
            {
                usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(inputLogin);
            }

            if (usuario == null) return null; // Usuario no existe
            if (usuario.Estado == false) throw new Exception("Tu cuenta está desactivada.");

            // Validar Password
            bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            return passwordCorrecto ? usuario : null;
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