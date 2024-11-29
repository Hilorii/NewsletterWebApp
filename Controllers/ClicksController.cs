using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using NewsletterWebAppBackend.Helpers;

namespace NewsletterWebAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClicksController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public ClicksController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Pobieranie kliknięć bez użycia modelu Click
        [HttpGet]
        public async Task<IActionResult> GetClicks()
        {
            try
            {
                var clicks = new List<object>();

                using (var connection = _dbHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Email_Log_Id, Clicked_at FROM Clicks";
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                clicks.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Email_Log_Id = reader.GetInt32(1),
                                    Clicked_at = reader.GetDateTime(2)
                                });
                            }
                        }
                    }
                }

                return Ok(clicks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}