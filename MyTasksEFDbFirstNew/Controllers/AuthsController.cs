using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTasks.DTOs.Signups.Request;
using MyTasksEFDbFirst.DTOs.SignIns.Request;
using MyTasksEFDbFirst.DTOs.SignIns.Response;
using MyTasksEFDbFirst.Models;
using System.Data;

namespace MyTasksEFDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly TestdbContext context; // holder 
        public AuthsController(TestdbContext testdbContext)
        {
            context = testdbContext;
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Registration([FromBody] SignUpInputDTO input)
        {
            try
            {
                //validate input 
                if (!string.IsNullOrEmpty(input.Password) && !string.IsNullOrEmpty(input.Email) &&
                    !string.IsNullOrEmpty(input.Name))
                {
                    if (context.Users.Any(x => x.Email == input.Email))
                        throw new Exception("Email Is Already Exisit");
                    User user = new User(); //empty object 
                    user.Email = input.Email;
                    user.Name = input.Name;
                    user.Password = input.Password;
                    context.Add(user);
                    context.SaveChanges();
                    //Validation Using Query 
                    //1- Search 
                    var users1 = context.Users.Where(x => x.Email == user.Email).ToList();
                    //2-Get One 
                    var users2 = context.Users.Where(x => x.Email == user.Email).FirstOrDefault();
                    var users3 = context.Users.Where(x => x.Email == user.Email).SingleOrDefault();
                    var isUserInserted = context.Users.Where(x => x.Email == user.Email).ToList().Count() == 1;
                    //Check If Exisit 
                    if (context.Users.Any(x => x.Email == user.Email))
                        return StatusCode(201, " Account Created");
                    else
                        return StatusCode(400, "Failed To Create Account ");
                    // Validation using the same object
                    if (user.Id > 0)
                        return StatusCode(201, " Account Created");
                    else
                        return StatusCode(400, "Failed To Create Account ");
                }
                return StatusCode(400, "Failed To Create Account ");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An Error Was Occured {ex.Message}");
            }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn(SignInInputDTO input)
        {
            var response = new SignInOutputDTO();
            try
            {
                if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
                    throw new Exception("Email and Password are required ");

                var query = context.Users.Where(x => x.Email.Equals(input.Email)
                && x.Password.Equals(input.Password)).SingleOrDefault();
                //Mapping Extract 
                if (query == null)
                    throw new Exception("Invalid Email / Password");

                response.Id = query.Id;
                response.Name = query.Name;
                return Ok(response);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An Error Was Occured {ex.Message}");
            }
        }
    }
}
