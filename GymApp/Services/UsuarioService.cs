using GymApp.Models;
using GymApp.Repositories;
using QRCoder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net;
using GymApp.Data;
using GymApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymApp.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IWebHostEnvironment _env;
        private readonly GymDbContext _context;

        public UsuarioService(IUsuarioRepository usuarioRepository, IWebHostEnvironment env, GymDbContext context)
        {
            _usuarioRepository = usuarioRepository;
            _env = env;
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task<PagedResult<UsuarioViewModel>> ObtenerUsuariosPaginadosAsync(string? buscar, int pagina, int tamanoPagina = 10)
        {
            var query = _context.Usuarios.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(u => u.NombreCompleto.Contains(buscar) || (u.Dni != null && u.Dni.Contains(buscar)));
            }

            int count = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(count / (double)tamanoPagina);

            var items = await query
                .OrderByDescending(u => u.UserId)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(u => new UsuarioViewModel
                {
                    UserId = u.UserId,
                    NombreCompleto = u.NombreCompleto,
                    Dni = u.Dni,
                    NombreRol = u.Role.Nombre,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado ?? false,
                    NombreUsuario = u.NombreUsuario
                })
                .ToListAsync();

            return new PagedResult<UsuarioViewModel>
            {
                Items = items,
                TotalPages = totalPages,
                CurrentPage = pagina,
                SearchTerm = buscar
            };
        }


        public async Task<Usuario> ObtenerPorIdAsync(int id)
        {
            return await _usuarioRepository.ObtenerConDetallesAsync(id);
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario, string? passwordRaw, IFormFile? fotoArchivo = null)
        {
            // --- NUEVO: GUARDAR FOTO ---
            if (fotoArchivo != null)
            {
                usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoArchivo, usuario.Dni);
            }

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

        public async Task ActualizarUsuarioAsync(Usuario usuario, IFormFile? fotoArchivo = null)
        {
            // --- NUEVO: GUARDAR FOTO ---
            if (fotoArchivo != null)
            {
                // Limpieza de foto anterior
                if (!string.IsNullOrEmpty(usuario.FotoUrl))
                {
                    string oldPath = Path.Combine(_env.WebRootPath, "uploads", "fotos", usuario.FotoUrl);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoArchivo, usuario.Dni);
            }

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
        }        // --- MÉTODO PRIVADO PARA GESTIONAR UPLOADS ---
        private async Task<string> ProcesarFotoPerfilAsync(IFormFile fotoArchivo, string dniUsuario)
        {
            if (fotoArchivo == null || fotoArchivo.Length == 0) return null;

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "fotos");

                // Crear directorio si no existe
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generar nombre único: {dni}_{ticks}.{ext}
                string extension = Path.GetExtension(fotoArchivo.FileName);
                string uniqueFileName = $"{dniUsuario}_{DateTime.Now.Ticks}{extension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fotoArchivo.CopyToAsync(fileStream);
                }

                return uniqueFileName;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
