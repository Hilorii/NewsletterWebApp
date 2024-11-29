using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using NewsletterWebAppBackend.Helpers;

namespace NewsletterWebAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailLogsController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public EmailLogsController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Pobieranie logów emaili bez użycia modelu EmailLog
        [HttpGet]
        public async Task<IActionResult> GetEmailLogs()
        {
            try
            {
                var emailLogs = new List<object>();

                using (var connection = _dbHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Newsletter_Id, Sent_At FROM Email_Logs";
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                emailLogs.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Newsletter_Id = reader.GetInt32(1),
                                    Sent_At = reader.GetDateTime(2)
                                });
                            }
                        }
                    }
                }

                return Ok(emailLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}