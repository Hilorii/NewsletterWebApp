using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using NewsletterWebAppBackend.Helpers;

namespace NewsletterWebAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public UsersController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Pobieranie użytkowników bez użycia modelu User
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = new List<object>();

                // Uzyskanie połączenia z bazą danych
                using (var connection = _dbHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    // Przygotowanie komendy SQL
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Email, Password, Subscribed, created_at FROM Users";
                        command.CommandType = CommandType.Text;

                        // Wykonanie zapytania i odczyt wyników
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Email = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    Subscribed = reader.GetBoolean(3),
                                    created_at = reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
