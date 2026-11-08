using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Biblioteca.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevolucionesController : ControllerBase
    {
        private readonly DbConnection _db;

        public DevolucionesController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = _db.CreateConnection();
            var devoluciones = await connection.QueryAsync<Devolucion>("SELECT * FROM Devoluciones");
            return Ok(devoluciones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = _db.CreateConnection();
            var devolucion = await connection.QueryFirstOrDefaultAsync<Devolucion>(
                "SELECT * FROM Devoluciones WHERE Devolucionid = @id",
                new { id });

            if (devolucion == null)
                return NotFound("Devolución no encontrada");

            return Ok(devolucion);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Devolucion devolucion)
        {
            SqlConnection? connection = null;
            SqlTransaction? transaction = null;

            try
            {
                // Aseguramos que la conexión sea de tipo SqlConnection para soportar OpenAsync
                connection = _db.CreateConnection() as SqlConnection;
                if (connection == null)
                {
                    return StatusCode(500, "Error de configuración: La conexión no es de tipo SQL Server.");
                }

                await connection.OpenAsync();
                transaction = connection.BeginTransaction();

                // 1. Obtener el Libroid asociado a ese Prestamoid
                var libroid = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT Libroid FROM Prestamos WHERE Prestamoid = @Prestamoid",
                    new { devolucion.Prestamoid },
                    transaction);

                if (libroid == null)
                {
                    transaction.Rollback();
                    return NotFound("El préstamo especificado no existe");
                }

                // 2. Verificar si el préstamo ya fue devuelto anteriormente
                int existe = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Devoluciones WHERE Prestamoid = @Prestamoid",
                    new { devolucion.Prestamoid },
                    transaction);

                if (existe > 0)
                {
                    transaction.Rollback();
                    return BadRequest("Este préstamo ya cuenta con una devolución registrada");
                }

                // 3. Verificar que el libro no supere su inventario total al sumar stock
                var libroInfo = await connection.QueryFirstOrDefaultAsync<(int Disponible, int Total)>(
                    "SELECT Cantidaddisponible, Cantidadtotal FROM Libros WHERE Libroid = @libroid",
                    new { libroid },
                    transaction);

                if (libroInfo.Disponible >= libroInfo.Total)
                {
                    transaction.Rollback();
                    return BadRequest("No se puede registrar la devolución porque el libro ya se encuentra con su inventario completo en stock.");
                }

                // 4. Insertar la devolución
                var sqlInsert = @"INSERT INTO Devoluciones
                                (Prestamoid, Fechadevolucion, Observaciones)
                                VALUES
                                (@Prestamoid, @Fechadevolucion, @Observaciones)";

                await connection.ExecuteAsync(sqlInsert, devolucion, transaction);

                // 5. Devolver la unidad al stock del libro (+1)
                var sqlUpdateLibro = @"UPDATE Libros 
                                       SET Cantidaddisponible = Cantidaddisponible + 1 
                                       WHERE Libroid = @libroid";

                await connection.ExecuteAsync(sqlUpdateLibro, new { libroid }, transaction);

                // 6. Confirmar transacción
                transaction.Commit();

                return Ok("Devolución registrada correctamente");
            }
            catch (Exception ex)
            {
                try
                {
                    transaction?.Rollback();
                }
                catch
                {
                    // Evitar que falle el rollback si la conexión ya estaba cerrada
                }

                return StatusCode(500, $"Error: {ex.Message} | Detalle: {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = _db.CreateConnection();

            int filas = await connection.ExecuteAsync(
                "DELETE FROM Devoluciones WHERE Devolucionid = @id",
                new { id });

            if (filas == 0)
                return NotFound("Devolución no encontrada");

            return Ok("Devolución eliminada correctamente");
        }
    }
}