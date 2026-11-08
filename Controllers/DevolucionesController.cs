
using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

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

            var devoluciones = await connection.QueryAsync<Devolucion>(
                "SELECT * FROM Devoluciones");

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
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var libroid = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT Libroid FROM Prestamos WHERE Prestamoid = @Prestamoid",
                    new { devolucion.Prestamoid },
                    transaction);

                if (libroid == null)
                {
                    transaction.Rollback();
                    return NotFound("Préstamo no encontrado");
                }

                int existe = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Devoluciones WHERE Prestamoid = @Prestamoid",
                    new { devolucion.Prestamoid },
                    transaction);

                if (existe > 0)
                {
                    transaction.Rollback();
                    return BadRequest("Este préstamo ya fue devuelto");
                }

                var sql = @"INSERT INTO Devoluciones
                            (Prestamoid, Fechadevolucion, Observaciones)
                            VALUES
                            (@Prestamoid, @Fechadevolucion, @Observaciones)";

                await connection.ExecuteAsync(sql, devolucion, transaction);

                await connection.ExecuteAsync(
                    "UPDATE Libros SET Cantidaddisponible = Cantidaddisponible + 1 WHERE Libroid = @libroid",
                    new { libroid },
                    transaction);

                transaction.Commit();

                return Ok("Devolución registrada correctamente");
            }
            catch
            {
                transaction.Rollback();
                return StatusCode(500, $"Error: {ex.Message} | Detalle: {ex.InnerException?.Message}");
            }
        }
    }
}
