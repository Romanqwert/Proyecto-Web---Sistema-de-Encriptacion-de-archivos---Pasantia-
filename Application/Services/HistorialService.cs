using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Core.Entities;
using EncriptacionApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Application.Services
{
    /// Implementación del servicio de registro de historial.
    public class HistorialService : IHistorialService
    {
        private readonly AppDbContext _context;

        public HistorialService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Historial>> GetUserHistoryAsync(int userId)
        {
            return await _context.Historial.Where(h => h.IdUsuario == userId).ToListAsync<Historial>();
        }

        /// Registra una nueva acción en la base de datos.
        public async Task RegistrarAccion(int idUsuario, int? idAlgoritmo, string accion, string resultado, string ipOrigen)
        {
            // Si no se proporciona un id de algoritmo, buscamos el de 'AES-256' por defecto.
            int idAlgoritmoFinal = idAlgoritmo ??
                                   (await _context.Algoritmo
                                        .FirstOrDefaultAsync(a => a.NombreAlgoritmo == "AES-256"))?.IdAlgoritmo ?? 1;

            var historial = new Historial
            {
                IdUsuario = idUsuario,
                IdAlgoritmo = idAlgoritmoFinal,
                Accion = accion,
                Resultado = resultado,
                IpOrigen = ipOrigen,
                FechaAccion = DateTime.UtcNow
            };

            await _context.Historial.AddAsync(historial);
            await _context.SaveChangesAsync();
        }
    }
}


