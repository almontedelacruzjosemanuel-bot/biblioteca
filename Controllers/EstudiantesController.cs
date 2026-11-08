
using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstudiantesController : ControllerBase
    {
        private readonly DbConnection _db;

        public EstudiantesController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = _db.CreateConnection();

            var estudiantes = await connection.QueryAsync<Estudiante>(
                "SELECT * FROM Estudiantes");

            return Ok(estudiantes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = _db.CreateConnection();

            var estudiante = await connection.QueryFirstOrDefaultAsync<Estudiante>(
                "SELECT * FROM Estudiantes WHERE Estudianteid = @id",
                new { id });

            if (estudiante == null)
                return NotFound("Estudiante no encontrado");

            return Ok(estudiante);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Estudiante estudiante)
        {
            using var connection = _db.CreateConnection();

            var sql = @"INSERT INTO Estudiantes
                        (Nombre, Apellido, Matricula, Celular, Email)
                        VALUES
                        (@Nombre, @Apellido, @Matricula, @Celular, @Email)";

            await connection.ExecuteAsync(sql, estudiante);

            return Ok("Estudiante registrado correctamente");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Estudiante estudiante)
        {
            using var connection = _db.CreateConnection();

            var sql = @"UPDATE Estudiantes
                        SET Nombre = @Nombre,
                            Apellido = @Apellido,
                            Matricula = @Matricula,
                            Celular = @Celular,
                            Email = @Email
                        WHERE Estudianteid = @Estudianteid";

            estudiante.Estudianteid = id;

            int filas = await connection.ExecuteAsync(sql, estudiante);

            if (filas == 0)
                return NotFound("Estudiante no encontrado");

            return Ok("Estudiante actualizado correctamente");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = _db.CreateConnection();

            int filas = await connection.ExecuteAsync(
                "DELETE FROM Estudiantes WHERE Estudianteid = @id",
                new { id });

            if (filas == 0)
                return NotFound("Estudiante no encontrado");

            return Ok("Estudiante eliminado correctamente");
        }
    }
}
