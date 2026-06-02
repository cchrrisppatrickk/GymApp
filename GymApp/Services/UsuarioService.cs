using GymApp.Constants;
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

        /// <summary>Solo personal: Admin + Empleado, con sus permisos cargados.</summary>
        public async Task<List<Usuario>> ObtenerPersonalAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Role)
                .Include(u => u.UsuarioPermisos)
                .Where(u => u.Role.Nombre == AppRoles.Admin || u.Role.Nombre == "Empleado")
                .OrderByDescending(u => u.UserId)
                .ToListAsync();
        }

        /// <summary>Solo socios (rol Cliente), paginados.</summary>
        public async Task<PagedResult<UsuarioViewModel>> ObtenerSociosPaginadosAsync(string? buscar, int pagina, int? mes = null, int? anio = null, int tamanoPagina = 20)
        {
            var query = _context.Usuarios
                .Include(u => u.Role)
                .Where(u => u.Role.Nombre == "Cliente")
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(u => u.NombreCompleto.Contains(buscar) || (u.Dni != null && u.Dni.Contains(buscar)));
            else if (mes.HasValue && anio.HasValue)
                query = query.Where(u => u.FechaRegistro.HasValue && u.FechaRegistro.Value.Month == mes.Value && u.FechaRegistro.Value.Year == anio.Value);

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
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    NombreRol = u.Role.Nombre,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado ?? false,
                    NombreUsuario = u.NombreUsuario,
                    FotoBase64 = u.FotoUrl,
                    WhatsApp = u.WhatsApp,
                    Origen = u.Origen,
                    Genero = u.Genero,
                    PinAcceso = u.PinAcceso,
                    ModificadoPorNombre = _context.Usuarios
                        .Where(ua => ua.UserId == u.ModificadoPorId)
                        .Select(ua => ua.NombreCompleto)
                        .FirstOrDefault() ?? "SISTEMA"
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
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    NombreRol = u.Role.Nombre,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado ?? false,
                    NombreUsuario = u.NombreUsuario,
                    FotoBase64 = u.FotoUrl,
                    WhatsApp = u.WhatsApp,
                    Origen = u.Origen,
                    Genero = u.Genero,
                    PinAcceso = u.PinAcceso,
                    ModificadoPorNombre = _context.Usuarios
                        .Where(ua => ua.UserId == u.ModificadoPorId)
                        .Select(ua => ua.NombreCompleto)
                        .FirstOrDefault() ?? "SISTEMA"
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

        public async Task<UsuarioDetailsDTO> ObtenerDetallesCrmAsync(int id)
        {
            var u = await _context.Usuarios
                .Include(u => u.Role)
                .Include(u => u.Restricciones)
                .Include(u => u.Membresia)
                    .ThenInclude(m => m.Plan)
                .Include(u => u.Membresia)
                    .ThenInclude(m => m.Turno)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (u == null) return null;

            // Obtener nombres de usuarios aplicadores manualmente para evitar problemas de proyección compleja
            var aplicadorIds = u.Restricciones.Select(r => r.UsuarioAplicadorId).Distinct().ToList();
            var nombresAplicadores = await _context.Usuarios
                .Where(ua => aplicadorIds.Contains(ua.UserId))
                .ToDictionaryAsync(ua => ua.UserId, ua => ua.NombreCompleto);

            var modificadoPorNombre = u.ModificadoPorId.HasValue 
                ? (await _context.Usuarios.Where(ua => ua.UserId == u.ModificadoPorId.Value).Select(ua => ua.NombreCompleto).FirstOrDefaultAsync())
                : null;

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            return new UsuarioDetailsDTO
            {
                UserId = u.UserId,
                NombreCompleto = u.NombreCompleto,
                ApellidoPaterno = u.ApellidoPaterno,
                ApellidoMaterno = u.ApellidoMaterno,
                NombreRol = u.Role.Nombre,
                Dni = u.Dni,
                Email = u.Email,
                WhatsApp = u.WhatsApp,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                FotoUrl = u.FotoUrl,
                FechaNacimiento = u.FechaNacimiento,
                Genero = u.Genero,
                EstadoCivil = u.EstadoCivil,
                Origen = u.Origen,
                Ocupacion = u.Ocupacion,
                Nota = u.Nota,
                PinAcceso = u.PinAcceso,
                Estado = u.Estado ?? false,
                FechaRegistro = u.FechaRegistro,
                FechaUltimaModificacion = u.FechaUltimaModificacion,
                ModificadoPorNombre = modificadoPorNombre,
                Restricciones = u.Restricciones.Select(r => new RestriccionDTO
                {
                    Id = r.Id,
                    TipoRestriccion = r.TipoRestriccion,
                    Descripcion = r.Descripcion,
                    FechaAplicacion = r.FechaAplicacion,
                    EstadoActiva = r.EstadoActiva,
                    UsuarioAplicadorNombre = nombresAplicadores.ContainsKey(r.UsuarioAplicadorId) ? nombresAplicadores[r.UsuarioAplicadorId] : "Sistema"
                }).OrderByDescending(r => r.FechaAplicacion).ToList(),
                Membresias = u.Membresia.Select(m => new MembresiaListDTO
                {
                    MembresiaId = m.MembresiaId,
                    NombrePlan = m.Plan?.Nombre ?? "Plan Eliminado",
                    NombreTurno = m.Turno?.Nombre ?? "Sin Turno",
                    FechaInicio = m.FechaInicio.ToString("dd/MM/yyyy"),
                    FechaVencimiento = m.FechaVencimiento.ToString("dd/MM/yyyy"),
                    Estado = m.FechaVencimiento < hoy ? "Vencida" : "Activa",
                    DiasRestantes = Math.Max(0, m.FechaVencimiento.DayNumber - hoy.DayNumber),
                    DiasVencidos = Math.Max(0, hoy.DayNumber - m.FechaVencimiento.DayNumber)
                }).OrderByDescending(m => m.FechaVencimiento).ToList()
            };
        }

        public async Task<Usuario> CrearUsuarioCrmAsync(UsuarioCreateDTO dto)
        {
            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                ApellidoPaterno = dto.ApellidoPaterno,
                ApellidoMaterno = dto.ApellidoMaterno,
                RoleId = dto.RoleId,
                Dni = dto.Dni,
                Email = dto.Email,
                WhatsApp = dto.WhatsApp,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                FechaNacimiento = dto.FechaNacimiento,
                Genero = dto.Genero,
                EstadoCivil = dto.EstadoCivil,
                Origen = dto.Origen,
                Ocupacion = dto.Ocupacion,
                Nota = dto.Nota,
                PinAcceso = await GenerarPinAccesoAsync(), // Siempre generado automáticamente
                NombreUsuario = dto.NombreUsuario,
                Estado = true
            };

            return await CrearUsuarioAsync(usuario, dto.Password, dto.FotoArchivo);
        }

        public async Task ActualizarUsuarioCrmAsync(UsuarioEditDTO dto, int modificadoPorId)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UserId);
            if (usuario == null) throw new Exception("Usuario no encontrado");

            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.ApellidoPaterno = dto.ApellidoPaterno;
            usuario.ApellidoMaterno = dto.ApellidoMaterno;
            usuario.RoleId = dto.RoleId;
            usuario.Dni = dto.Dni;
            usuario.Email = dto.Email;
            usuario.WhatsApp = dto.WhatsApp;
            usuario.Telefono = dto.Telefono;
            usuario.Direccion = dto.Direccion;
            usuario.FechaNacimiento = dto.FechaNacimiento;
            usuario.Genero = dto.Genero;
            usuario.EstadoCivil = dto.EstadoCivil;
            usuario.Origen = dto.Origen;
            usuario.Ocupacion = dto.Ocupacion;
            usuario.Nota = dto.Nota;
            // usuario.PinAcceso = dto.PinAcceso; // Se elimina la actualización manual del PIN
            usuario.Estado = dto.Estado;
            usuario.NombreUsuario = dto.NombreUsuario;

            // Auditoría
            usuario.FechaUltimaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            // Password (solo si se provee)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await ActualizarUsuarioAsync(usuario, dto.FotoArchivo);
        }

        public async Task<string> GenerarPinAccesoAsync()
        {
            var random = new Random();
            string pin;
            bool existe;
            do
            {
                pin = random.Next(1000, 999999).ToString("D4"); // Entre 4 y 6 dígitos
                existe = await _context.Usuarios.AnyAsync(u => u.PinAcceso == pin);
            } while (existe);

            return pin;
        }

        public async Task<string> RegenerarPinAsync(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) throw new Exception("Usuario no encontrado");

            usuario.PinAcceso = await GenerarPinAccesoAsync();
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveAsync();

            return usuario.PinAcceso;
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario, string? passwordRaw, IFormFile? fotoArchivo = null, string? fotoBase64 = null)
        {
            // Normalizar DNI: convertir a null si está vacío o solo tiene espacios
            usuario.Dni = string.IsNullOrWhiteSpace(usuario.Dni) ? null : usuario.Dni.Trim();

            // --- NUEVO: GUARDAR FOTO ---
            if (fotoArchivo != null || !string.IsNullOrEmpty(fotoBase64))
            {
                // Usamos el DNI si existe, si no, el NombreUsuario (que se generará abajo si es nulo), 
                // pero necesitamos el identificador ya.
                string identifier = !string.IsNullOrWhiteSpace(usuario.Dni) ? usuario.Dni : (!string.IsNullOrWhiteSpace(usuario.NombreUsuario) ? usuario.NombreUsuario : "user_" + Guid.NewGuid().ToString().Substring(0, 8));
                
                if (fotoArchivo != null)
                {
                    usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoArchivo, identifier);
                }
                else
                {
                    usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoBase64, identifier);
                }
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

            // C. Generar PIN si no existe
            if (string.IsNullOrWhiteSpace(usuario.PinAcceso))
            {
                usuario.PinAcceso = await GenerarPinAccesoAsync();
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

        public async Task ActualizarUsuarioAsync(Usuario usuario, IFormFile? fotoArchivo = null, string? fotoBase64 = null)
        {
            // Normalizar DNI: convertir a null si está vacío o solo tiene espacios
            usuario.Dni = string.IsNullOrWhiteSpace(usuario.Dni) ? null : usuario.Dni.Trim();

            // --- NUEVO: GUARDAR FOTO ---
            if (fotoArchivo != null || !string.IsNullOrEmpty(fotoBase64))
            {
                // Limpieza de foto anterior
                if (!string.IsNullOrEmpty(usuario.FotoUrl))
                {
                    string oldFileName = Path.GetFileName(usuario.FotoUrl);
                    string oldPath = Path.Combine(_env.WebRootPath, "uploads", "fotos", oldFileName);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                // Si no hay DNI, usar el NombreUsuario como identificador para procesar la foto
                string identifier = !string.IsNullOrWhiteSpace(usuario.Dni) ? usuario.Dni : usuario.NombreUsuario;
                
                if (fotoArchivo != null)
                {
                    usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoArchivo, identifier);
                }
                else
                {
                    usuario.FotoUrl = await ProcesarFotoPerfilAsync(fotoBase64, identifier);
                }
            }

            // Validar que el DNI no esté duplicado por otro usuario (excluyendo al usuario actual)
            if (!string.IsNullOrWhiteSpace(usuario.Dni))
            {
                var usuarioConMismoDni = await _usuarioRepository.ObtenerPorDNIAsync(usuario.Dni);
                if (usuarioConMismoDni != null && usuarioConMismoDni.UserId != usuario.UserId)
                {
                    throw new Exception("El DNI ya está registrado por otro usuario.");
                }
            }

            // Validar que el NombreUsuario no esté duplicado por otro usuario (excluyendo al usuario actual)
            if (!string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            {
                var usuarioConMismoNombre = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(usuario.NombreUsuario);
                if (usuarioConMismoNombre != null && usuarioConMismoNombre.UserId != usuario.UserId)
                {
                    throw new Exception($"El usuario '{usuario.NombreUsuario}' ya está en uso.");
                }
            }

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

        public async Task<List<string>> ObtenerPermisosUsuarioAsync(int userId)
        {
            return await _context.UsuarioPermisos
                .Where(up => up.UserId == userId)
                .Select(up => up.PermisoId)
                .ToListAsync();
        }

        public async Task ActualizarPermisosUsuarioAsync(int userId, string[] permisos)
        {
            var permisosActuales = await _context.UsuarioPermisos.Where(up => up.UserId == userId).ToListAsync();
            _context.UsuarioPermisos.RemoveRange(permisosActuales);
            
            if (permisos != null && permisos.Any())
            {
                var nuevosPermisos = permisos.Select(p => new UsuarioPermiso { UserId = userId, PermisoId = p });
                await _context.UsuarioPermisos.AddRangeAsync(nuevosPermisos);
            }
            
            await _context.SaveChangesAsync();
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

        private async Task<string?> ProcesarFotoPerfilAsync(string? fotoBase64, string dniUsuario)
        {
            if (string.IsNullOrEmpty(fotoBase64)) return null;

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "fotos");

                // Crear directorio si no existe
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string base64Data = fotoBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                byte[] fileBytes = Convert.FromBase64String(base64Data);
                string identifier = !string.IsNullOrWhiteSpace(dniUsuario) ? dniUsuario : "user_" + Guid.NewGuid().ToString().Substring(0, 8);
                string uniqueFileName = $"perfil_{identifier}_{DateTime.Now.Ticks}.jpg";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                return $"/uploads/fotos/{uniqueFileName}";
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
