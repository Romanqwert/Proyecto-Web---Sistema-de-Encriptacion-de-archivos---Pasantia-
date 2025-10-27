using EncriptacionApi.Core.Entities;
using System.Linq.Expressions;

namespace EncriptacionApi.Application.Interfaces
{
    /// Repositorio para las operaciones CRUD de la entidad Archivo.
    public interface IArchivoRepository
    {
        Task<Archivo?> GetByIdAsync(int id);        // Obtiene un archivo por su ID.
        Task AddAsync(Archivo archivo);        // Añade un nuevo archivo a la base de datos.
        Task<IEnumerable<Archivo>> FindAsync(Expression<Func<Archivo, bool>> predicate);         // Busca archivos basándose en un predicado (filtro).

    }
}
