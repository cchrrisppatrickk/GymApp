using GymApp.Models;
using GymApp.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class PlaneService : IPlaneService
    {
        private readonly IPlaneRepository _planeRepository;

        public PlaneService(IPlaneRepository planeRepository)
        {
            _planeRepository = planeRepository;
        }

        public async Task<IEnumerable<Plane>> ObtenerTodosAsync()
        {
            return await _planeRepository.GetAllAsync();
        }

        public async Task<Plane> ObtenerPorIdAsync(int id)
        {
            return await _planeRepository.GetByIdAsync(id);
        }

        public async Task CrearPlanAsync(Plane plan)
        {
            ValidarPlan(plan);
            await _planeRepository.InsertAsync(plan);
            await _planeRepository.SaveAsync();
        }

        public async Task ActualizarPlanAsync(Plane plan)
        {
            ValidarPlan(plan);
            await _planeRepository.UpdateAsync(plan);
            await _planeRepository.SaveAsync();
        }

        public async Task EliminarPlanAsync(int id)
        {
            // Opcional: Validar si existen Membresías activas con este plan antes de borrar
            await _planeRepository.DeleteAsync(id);
            await _planeRepository.SaveAsync();
        }

        // Método privado para no repetir lógica
        private void ValidarPlan(Plane plan)
        {
            if (plan.PrecioBase < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            if (plan.DuracionDias <= 0)
                throw new ArgumentException("La duración debe ser al menos de 1 día.");

            if (string.IsNullOrWhiteSpace(plan.Nombre))
                throw new ArgumentException("El nombre del plan es obligatorio.");
        }
    }
}