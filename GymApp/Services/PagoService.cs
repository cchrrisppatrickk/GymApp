using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepo;
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IUsuarioRepository _usuarioRepo; // Necesitamos buscar user por DNI

        public PagoService(IPagoRepository pagoRepo, IMembresiaRepository membresiaRepo, IUsuarioRepository usuarioRepo)
        {
            _pagoRepo = pagoRepo;
            _membresiaRepo = membresiaRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<DeudaInfoDTO> BuscarMembresiaPorDniAsync(string dni)
        {
            var usuario = await _usuarioRepo.ObtenerPorDNIAsync(dni);
            if (usuario == null) throw new Exception("Usuario no encontrado");

            // Buscamos la última membresía registrada del usuario
            // Nota: Aquí podrías filtrar por 'Activa' si solo permites pagar deudas actuales
            var membresias = await _membresiaRepo.ObtenerTodasConDetallesAsync();
            var ultimaMembresia = membresias
                                    .Where(m => m.UserId == usuario.UserId)
                                    .OrderByDescending(m => m.MembresiaId)
                                    .FirstOrDefault();

            if (ultimaMembresia == null) throw new Exception("El usuario no tiene membresías registradas");

            return new DeudaInfoDTO
            {
                MembresiaId = ultimaMembresia.MembresiaId,
                Cliente = usuario.NombreCompleto,
                Plan = ultimaMembresia.Plan.Nombre,
                Estado = ultimaMembresia.Estado
            };
        }

        public async Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId)
        {
            // 1. Validar Membresía (reutilizamos lógica o buscamos directo)
            var info = await BuscarMembresiaPorDniAsync(dto.DniCliente);

            // 2. Crear Entidad
            var nuevoPago = new PagosMembresium
            {
                MembresiaId = info.MembresiaId,
                UsuarioEmpleadoId = empleadoId, // Auditoría
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                FechaPago = DateTime.Now,
                Comprobante = Guid.NewGuid().ToString().Substring(0, 8).ToUpper() // Generamos un código simple
            };

            await _pagoRepo.InsertAsync(nuevoPago);
            await _pagoRepo.SaveAsync();

            return nuevoPago.PagoId;
        }

        public async Task<IEnumerable<PagoListDTO>> ListarPagosAsync()
        {
            var pagos = await _pagoRepo.ObtenerHistorialCompletoAsync();
            return pagos.Select(p => new PagoListDTO
            {
                PagoId = p.PagoId,
                NombreCliente = p.Membresia.User.NombreCompleto,
                NombrePlan = p.Membresia.Plan.Nombre,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                FechaPago = p.FechaPago?.ToString("g"), // Formato general fecha+hora
                NombreEmpleado = p.UsuarioEmpleado.NombreCompleto
            });
        }
    }
}