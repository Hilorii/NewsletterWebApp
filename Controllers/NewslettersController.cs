using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using NewsletterWebAppBackend.Helpers;

namespace NewsletterWebAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewslettersController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public NewslettersController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Pobieranie newsletterów bez użycia modelu Newsletter
        [HttpGet]
        public async Task<IActionResult> GetNewsletters()
        {
            try
            {
                var newsletters = new List<object>();

                using (var connection = _dbHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Title, Content, Created_at, Updated_at, Image_url, Thumbnail_url FROM Newsletters";
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                newsletters.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Content = reader.GetString(2),
                                    Created_at = reader.GetDateTime(3),
                                    Updated_at = reader.GetDateTime(4),
                                    Image_url = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    Thumbnail_url = reader.IsDBNull(6) ? null : reader.GetString(6)
                                });
                            }
                        }
                    }
                }

                return Ok(newsletters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
