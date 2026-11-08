using Biblioteca.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly DbConnection _db;

        public TestDbController(DbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                using var connection = _db.CreateConnection();
                await connection.OpenAsync();

                return Ok("Conexión exitosa con BibliotecaDB");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}