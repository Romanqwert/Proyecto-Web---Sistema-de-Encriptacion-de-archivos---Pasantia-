using EncriptacionApi.Core.Entities;
using EncriptacionApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Application.Interfaces
{
    /// Define las operaciones para registrar el historial de acciones.
    public interface IHistorialService
    {
        /// Registra una nueva entrada en el historial.
        // <param name="idUsuario">ID del usuario que realiza la acción.</param>
        // <param name="idAlgoritmo">ID del algoritmo usado (si aplica).</param>
        // <param name="accion">Descripción de la acción (ej. "ENCRYPT_FILE").</param>
        // <param name="resultado">Resultado de la acción (ej. "SUCCESS").</param>
        // <param name="ipOrigen">Dirección IP desde donde se realizó la acción.</param>
        Task RegistrarAccion(int idUsuario, int? idAlgoritmo, string accion, string resultado, string ipOrigen);
        Task<List<Historial>> GetUserHistoryAsync(int userId);
    }

    public class HistorialService : IHistorialService
    {
        private readonly AppDbContext _context;

        public HistorialService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Historial>> GetUserHistoryAsync(int userId)
        {
            return await _context.Historial
                .Where(h => h.IdUsuario == userId)
                .OrderByDescending(h => h.IdHistorial)
                .ToListAsync<Historial>();
        }

        public async Task RegistrarAccion(int idUsuario, int? idAlgoritmo, string accion, string resultado, string ipOrigen)
        {
            var historial = new Historial
            {
                IdUsuario = idUsuario,
                Accion = accion,
                Resultado = resultado,
                IpOrigen = ipOrigen,
                FechaAccion = DateTime.UtcNow
            };

            // Solo asignamos IdAlgoritmo si tiene valor
            if (idAlgoritmo.HasValue)
            {
                historial.IdAlgoritmo = idAlgoritmo.Value;
            }

            _context.Historial.Add(historial);
            await _context.SaveChangesAsync();
        }
    }
}
