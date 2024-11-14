using ERPClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using TImeManagementSheet.Models;

namespace TImeManagementSheet.Controllers
{
    public class LoginController : Controller
    {
        IWebHostEnvironment Iwebhost;
        private string connectionString = "server=localhost;port=3306;uid=root;pwd=root;Database=timesheet";
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.LoginError = "Please enter both username and password";
                return View();
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // Parameterized query to prevent SQL injection
                string query = "SELECT Slno, UserName,Password, Email, MobileNo FROM users WHERE UserName = @UserName AND Password = @Password";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.UserName = reader["UserName"] == DBNull.Value ? null : Convert.ToString(reader["UserName"]);
                            user.Password = reader["Password"] == DBNull.Value ? null : Convert.ToString(reader["Password"]);
                            user.Email = reader["Email"] == DBNull.Value ? null : Convert.ToString(reader["Email"]);
                            user.MobileNo = reader["MobileNo"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MobileNo"]);

                            

                            // Update login date
                            reader.Close();
                            string updateQuery = "UPDATE users SET LoginDate = @LoginDate WHERE UserName = @UserName and Password = @Password";
                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@LoginDate", DateTime.Now);
                                updateCommand.Parameters.AddWithValue("@UserName", user.UserName);
                                updateCommand.Parameters.AddWithValue("@Password", user.Password);
                                updateCommand.ExecuteNonQuery();
                            }
                            // Store user details in session
                            user.LoginDate = DateTime.Now;
                            HttpContext.Session.SetSessionData("UserInfo", user);
                            return RedirectToAction("DashBoard", "Login");
                        }
                        else
                        {
                            ViewBag.LoginError = "Invalid Credentials";
                        }
                    }
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult DashBoard()
        {
            User user = (User) HttpContext.Session.GetSessionData<User>("UserInfo");
            ViewBag.UserName=user.UserName;
            List<TimeSheet> list = new List<TimeSheet>();
             
            return View(list); 
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> LoadData(int year, int month)
        {
            // DateTime dt=new DateTime(year,month,1);

            //List<TimeSheet> list = new List<TimeSheet>();
            //list.Add(new TimeSheet { Id = 1, Remarks = "1", WorkDate = DateTime.Now, Work = "Worked on Bugs" });
            //list.Add(new TimeSheet { Id = 2, Remarks = "2", WorkDate = DateTime.Now.AddDays(1), Work = "Worked on Bugs" });
            //list.Add(new TimeSheet { Id = 3, Remarks = "3", WorkDate = DateTime.Now.AddDays(2), Work = "Worked on Bugs" });
            //list.Add(new TimeSheet { Id = 4, Remarks = "4", WorkDate = DateTime.Now.AddDays(3), Work = "Worked on Bugs" });
            //list.Add(new TimeSheet { Id = 5, Remarks = "5", WorkDate = DateTime.Now.AddDays(4), Work = "Worked on Bugs" });
            //list.Add(new TimeSheet { Id = 6, Remarks = "6", WorkDate = DateTime.Now.AddDays(5), Work = "Worked on Bugs" });


            //return Json(list);


            var workEntries = new List<TimeSheet>(); 
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM WorkEntries WHERE YEAR(WorkDate) = @Year AND MONTH(WorkDate) = @Month", connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Month", month);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            workEntries.Add(new TimeSheet
                            {
                                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Id"]),
                                WorkDate = reader["WorkDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["WorkDate"]),
                                Work = reader["Work"] == DBNull.Value ? string.Empty : reader["Work"].ToString(),
                                Remarks = reader["Remarks"] == DBNull.Value ? string.Empty : reader["Remarks"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(workEntries);
        }

        [HttpPost]
       [Produces("application/json")]
        public async Task<IActionResult> AddEntry(DateTime workDate, string work, string remarks)
        {
            try
            {
                // Assuming you have a TimeSheet model with the necessary properties
                var newEntry = new TimeSheet
                {
                    WorkDate = workDate,
                    Work = work,
                    Remarks = remarks
                };

                // Save to database (use your method for saving data, e.g., Entity Framework or raw SQL)
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("INSERT INTO WorkEntries (WorkDate, Work, Remarks) VALUES (@WorkDate, @Work, @Remarks)", connection);
                    command.Parameters.AddWithValue("@WorkDate", workDate);
                    command.Parameters.AddWithValue("@Work", work);
                    command.Parameters.AddWithValue("@Remarks", remarks);

                    await command.ExecuteNonQueryAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> UpdateEntry(int id, DateTime workDate, string work, string remarks)
        {
            try
            {
                // Open a connection to the database
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL query to update the existing entry
                    var command = new MySqlCommand("UPDATE WorkEntries SET  Work = @Work, Remarks = @Remarks WHERE Id = @Id", connection); 
                    command.Parameters.AddWithValue("@Work", work);
                    command.Parameters.AddWithValue("@Remarks", remarks);
                    command.Parameters.AddWithValue("@Id", id);

                    // Execute the update command
                    var result = await command.ExecuteNonQueryAsync();

                    if (result > 0) // If rows are affected, update was successful
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, error = "No record found to update." });
                    }
                }
            }
            catch (Exception ex)
            {
                // Return error details if exception occurs
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> DeleteEntry(int id)
        {
            try
            {
                // Open a connection to the database
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL query to delete the entry
                    var command = new MySqlCommand("DELETE FROM WorkEntries WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);

                    // Execute the delete command
                    var result = await command.ExecuteNonQueryAsync();

                    if (result > 0) // If rows are affected, delete was successful
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, error = "No record found to delete." });
                    }
                }
            }
            catch (Exception ex)
            {
                // Return error details if exception occurs
                return Json(new { success = false, error = ex.Message });
            }
        }



    }
}
