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
using GymApp.ViewModels.ApiAgent;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymApp.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IWebHostEnvironment _env;
        private readonly GymDbContext _context;
        private readonly IWebhookService _webhookService;
        private readonly IConfiguracionAlertaRepository _configRepo;

        public UsuarioService(IUsuarioRepository usuarioRepository, IWebHostEnvironment env, GymDbContext context, IWebhookService webhookService, IConfiguracionAlertaRepository configRepo)
        {
            _usuarioRepository = usuarioRepository;
            _env = env;
            _context = context;
            _webhookService = webhookService;
            _configRepo = configRepo;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task<PagedResult<UsuarioViewModel>> ObtenerUsuariosPaginadosAsync(string? buscar, int pagina, int? mes = null, int? anio = null, int tamanoPagina = 20)
        {
            var query = _context.Usuarios.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(u => u.NombreCompleto.Contains(buscar) || (u.Dni != null && u.Dni.Contains(buscar)));
            }
            else if (mes.HasValue && anio.HasValue)
            {
                query = query.Where(u => u.FechaRegistro.HasValue && u.FechaRegistro.Value.Month == mes.Value && u.FechaRegistro.Value.Year == anio.Value);
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
                // Usamos el DNI si existe, si no, el NombreUsuario (que se generará abajo si es nulo), 
                // pero necesitamos el identificador ya. Vamos a mover la lógica de indentificador arriba.
                string identifier = !string.IsNullOrWhiteSpace(usuario.Dni) ? usuario.Dni : (!string.IsNullOrWhiteSpace(usuario.NombreUsuario) ? usuario.NombreUsuario : "user_" + Guid.NewGuid().ToString().Substring(0, 8));
                usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoArchivo, identifier);
            }

            // 1. Validar DNI único (Solo si se proporcionó)
            if (!string.IsNullOrWhiteSpace(usuario.Dni) && await _usuarioRepository.ExisteDniAsync(usuario.Dni))
                throw new Exception("El DNI ya está registrado.");

            // --- LÓGICA DE AUTO-COMPLETADO ---

            // A. Si no enviaron Nombre de Usuario, usamos el DNI o generamos uno
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            {
                usuario.NombreUsuario = !string.IsNullOrWhiteSpace(usuario.Dni) 
                                        ? usuario.Dni 
                                        : "u_" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            // B. Si no enviaron Password, usamos el DNI o el NombreUsuario
            string passwordFinal = passwordRaw;
            if (string.IsNullOrWhiteSpace(passwordFinal))
            {
                passwordFinal = !string.IsNullOrWhiteSpace(usuario.Dni) 
                                ? usuario.Dni 
                                : usuario.NombreUsuario;
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

            // --- NOTIFICACIÓN EN TIEMPO REAL ---
            var configs = await _configRepo.GetAllAsync();
            foreach (var config in configs.Where(c => c.Activo && c.AvisarNuevoUsuario))
            {
                var payload = new 
                { 
                    ID = usuario.UserId, 
                    Nombre = usuario.NombreCompleto, 
                    Telefono = usuario.Telefono ?? "No registrado", 
                    DNI = usuario.Dni ?? "No registrado", 
                    Fecha = usuario.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") 
                };

                await _webhookService.EnviarAlertaInstantaneaAsync("NUEVO_USUARIO", payload, config.ChatIdDestino);
            }

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
        }

        // ── Dominio de Usuarios — Consultas granulares para el Agente IA ──────

        public async Task<IEnumerable<UsuarioAgenteDTO>> BuscarParaAgenteAsync(string termino)
        {
            return await _context.Usuarios
                .Where(u => u.NombreCompleto.Contains(termino) || u.Dni == termino)
                .Select(u => new UsuarioAgenteDTO
                {
                    Id             = u.UserId,
                    NombreCompleto = u.NombreCompleto,
                    DNI            = u.Dni,
                    Telefono       = u.Telefono,
                    FechaRegistro  = u.FechaRegistro ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UsuarioAgenteDTO>> ObtenerRecientesParaAgenteAsync(int dias)
        {
            var desde = DateTime.Now.AddDays(-dias);
            return await _context.Usuarios
                .Where(u => u.FechaRegistro >= desde)
                .OrderByDescending(u => u.FechaRegistro)
                .Select(u => new UsuarioAgenteDTO
                {
                    Id             = u.UserId,
                    NombreCompleto = u.NombreCompleto,
                    DNI            = u.Dni,
                    Telefono       = u.Telefono,
                    FechaRegistro  = u.FechaRegistro ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UsuarioAgenteDTO>> ObtenerPorFechaExactaParaAgenteAsync(DateTime fecha)
        {
            return await _context.Usuarios
                .Where(u => u.FechaRegistro.HasValue
                         && u.FechaRegistro.Value.Year  == fecha.Year
                         && u.FechaRegistro.Value.Month == fecha.Month
                         && u.FechaRegistro.Value.Day   == fecha.Day)
                .Select(u => new UsuarioAgenteDTO
                {
                    Id             = u.UserId,
                    NombreCompleto = u.NombreCompleto,
                    DNI            = u.Dni,
                    Telefono       = u.Telefono,
                    FechaRegistro  = u.FechaRegistro ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        // --- MÉTODO PRIVADO PARA GESTIONAR UPLOADS ---
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
