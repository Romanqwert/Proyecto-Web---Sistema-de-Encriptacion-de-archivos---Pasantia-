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
    }
}
