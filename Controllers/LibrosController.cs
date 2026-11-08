using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibrosController : ControllerBase
    {
        private readonly DbConnection _db;

        public LibrosController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = _db.CreateConnection();
            var libros = await connection.QueryAsync<Libro>("SELECT * FROM Libros");
            return Ok(libros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = _db.CreateConnection();
            var libro = await connection.QueryFirstOrDefaultAsync<Libro>(
                "SELECT * FROM Libros WHERE Libroid = @id",
                new { id });

            if (libro == null)
                return NotFound("Libro no encontrado");

            return Ok(libro);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Libro libro)
        {
            if (libro.Cantidaddisponible > libro.Cantidadtotal || libro.Cantidaddisponible < 0)
            {
                return BadRequest("La cantidad disponible no puede ser mayor a la cantidad total ni menor a cero.");
            }

            using var connection = _db.CreateConnection();

            var sql = @"INSERT INTO Libros
                        (Titulo, Autor, ISBN, Cantidadtotal, Cantidaddisponible)
                        VALUES
                        (@Titulo, @Autor, @ISBN, @Cantidadtotal, @Cantidaddisponible)";

            await connection.ExecuteAsync(sql, libro);

            return Ok("Libro registrado correctamente");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Libro libro)
        {
            if (libro.Cantidaddisponible > libro.Cantidadtotal || libro.Cantidaddisponible < 0)
            {
                return BadRequest("La cantidad disponible no es válida respecto a la cantidad total.");
            }

            using var connection = _db.CreateConnection();
            libro.Libroid = id;

            var sql = @"UPDATE Libros
                        SET Titulo = @Titulo,
                            Autor = @Autor,
                            ISBN = @ISBN,
                            Cantidadtotal = @Cantidadtotal,
                            Cantidaddisponible = @Cantidaddisponible
                        WHERE Libroid = @Libroid";

            int filas = await connection.ExecuteAsync(sql, libro);

            if (filas == 0)
                return NotFound("Libro no encontrado");

            return Ok("Libro actualizado correctamente");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = _db.CreateConnection();

            int filas = await connection.ExecuteAsync(
                "DELETE FROM Libros WHERE Libroid = @id",
                new { id });

            if (filas == 0)
                return NotFound("Libro no encontrado");

            return Ok("Libro eliminado correctamente");
        }
    }
}