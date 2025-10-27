using EncriptacionApi.Core.Entities;

namespace EncriptacionApi.Application.Interfaces
{
    /// Repositorio para las operaciones CRUD de la entidad Usuario.
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);        // Obtiene un usuario por su correo electrónico.
        Task<Usuario?> GetByUsernameAsync(string username);        // Obtiene un usuario por su nombre de usuario.
        Task AddAsync(Usuario usuario);        // Añade un nuevo usuario a la base de datos.
        Task<bool> UserExistsAsync(string username);        // Verifica si ya existe un usuario con ese nombre de usuario.
    }
}
