using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class CongelamientoService : ICongelamientoService
    {
        private readonly ICongelamientoRepository _congelamientoRepo;
        private readonly IMembresiaRepository _membresiaRepo;

        public CongelamientoService(
            ICongelamientoRepository congelamientoRepo,
            IMembresiaRepository membresiaRepo)
        {
            _congelamientoRepo = congelamientoRepo;
            _membresiaRepo = membresiaRepo;
        }

        public async Task CongelarAsync(CongelarMembresiaDTO dto)
        {
            // 1. Obtener Membresía con Plan incluido
            var membresia = await _membresiaRepo.GetByIdAsync(dto.MembresiaId);
            if (membresia == null) throw new Exception("La membresía no existe.");

            // Validar que el plan permita congelar
            if (membresia.Plan == null) 
            {
                // Refrescamos para obtener el plan si no está cargado
                var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
                membresia = todas.FirstOrDefault(m => m.MembresiaId == dto.MembresiaId);
            }

            if (!(membresia.Plan.PermiteCongelar ?? false))
                throw new Exception($"El plan '{membresia.Plan.Nombre}' no permite congelamientos.");

            // 2. Validar que no esté ya vencida
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            if (membresia.FechaVencimiento < hoy)
                throw new Exception("No puedes congelar una membresía ya vencida.");

            // 3. Validar que no esté ya congelada
            if (membresia.Estado == "Congelada")
                throw new Exception("La membresía ya se encuentra en estado Congelada.");

            // 4. Calcular duración del congelamiento
            var inicio = DateOnly.FromDateTime(dto.FechaInicio);
            var fin = DateOnly.FromDateTime(dto.FechaFin);

            if (fin <= inicio)
                throw new Exception("La fecha de fin debe ser posterior a la de inicio.");

            var diasCongelados = fin.DayNumber - inicio.DayNumber;

            // 5. Crear el Registro de Congelamiento
            var nuevoCongelamiento = new Congelamiento
            {
                MembresiaId = dto.MembresiaId,
                UsuarioEmpleadoId = dto.UsuarioEmpleadoId,
                FechaInicio = inicio,
                FechaFin = fin,
                Motivo = dto.Motivo,
                FechaRegistro = DateTime.Now
            };

            await _congelamientoRepo.InsertAsync(nuevoCongelamiento);

            // 6. ACTUALIZAR MEMBRESÍA
            // - Cambiar Estado
            membresia.Estado = "Congelada";
            // - Extender la fecha de vencimiento por los días pausados
            membresia.FechaVencimiento = membresia.FechaVencimiento.AddDays(diasCongelados);

            await _membresiaRepo.UpdateAsync(membresia);

            // 7. Guardar Cambios
            await _congelamientoRepo.SaveAsync();
            await _membresiaRepo.SaveAsync();
        }

        public async Task<IEnumerable<Congelamiento>> ListarHistorialAsync(int membresiaId)
        {
            return await _congelamientoRepo.GetHistorialPorMembresiaAsync(membresiaId);
        }

        public async Task DescongelarManualAsync(int membresiaId)
        {
            var membresia = await _membresiaRepo.GetByIdAsync(membresiaId);
            if (membresia == null) throw new Exception("Membresía no encontrada.");

            if (membresia.Estado != "Congelada")
                throw new Exception("La membresía no está congelada.");

            // Solo cambiamos el estado a activa
            // Los días de extensión ya se sumaron al inicio del proceso de congelación.
            membresia.Estado = "Activa";

            await _membresiaRepo.UpdateAsync(membresia);
            await _membresiaRepo.SaveAsync();
        }
    }
}
