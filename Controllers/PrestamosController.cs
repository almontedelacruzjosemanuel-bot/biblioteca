
using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrestamosController : ControllerBase
    {
        private readonly DbConnection _db;

        public PrestamosController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = _db.CreateConnection();

            var prestamos = await connection.QueryAsync<Prestamo>(
                "SELECT * FROM Prestamos");

            return Ok(prestamos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = _db.CreateConnection();

            var prestamo = await connection.QueryFirstOrDefaultAsync<Prestamo>(
                "SELECT * FROM Prestamos WHERE Prestamoid = @id",
                new { id });

            if (prestamo == null)
                return NotFound("Préstamo no encontrado");

            return Ok(prestamo);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Prestamo prestamo)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                int disponibles = await connection.ExecuteScalarAsync<int>(
                    "SELECT Cantidaddisponible FROM Libros WHERE Libroid = @Libroid",
                    new { prestamo.Libroid },
                    transaction);

                if (disponibles <= 0)
                {
                    transaction.Rollback();
                    return BadRequest("No hay libros disponibles");
                }

                var sql = @"INSERT INTO Prestamos
                            (Estudianteid, Libroid, Fechaprestamo, Fechavencimiento)
                            VALUES
                            (@Estudianteid, @Libroid, @Fechaprestamo, @Fechavencimiento)";

                await connection.ExecuteAsync(sql, prestamo, transaction);

                await connection.ExecuteAsync(
                    "UPDATE Libros SET Cantidaddisponible = Cantidaddisponible - 1 WHERE Libroid = @Libroid",
                    new { prestamo.Libroid },
                    transaction);

                transaction.Commit();

                return Ok("Préstamo registrado correctamente");
            }
            catch
            {
                transaction.Rollback();
                return StatusCode(500, "Error al registrar el préstamo");
            }
        }
    }
}
