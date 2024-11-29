using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using NewsletterWebAppBackend.Helpers;

namespace NewsletterWebAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailLogUsersController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public EmailLogUsersController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Pobieranie relacji EmailLogUsers bez użycia modelu EmailLogUser
        [HttpGet]
        public async Task<IActionResult> GetEmailLogUsers()
        {
            try
            {
                var emailLogUsers = new List<object>();

                using (var connection = _dbHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SELECT Id, Email_Log_Id, User_Id FROM Email_Log_Users", connection))
                    {
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                emailLogUsers.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Email_Log_Id = reader.GetInt32(1),
                                    User_Id = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                }

                return Ok(emailLogUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}