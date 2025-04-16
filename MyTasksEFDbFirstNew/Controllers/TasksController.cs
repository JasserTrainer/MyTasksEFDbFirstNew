using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTasksEFDbFirst.DTOs.Tasks.Request;
using MyTasksEFDbFirst.Models;
using static MyTasksEFDbFirst.Helpers.Enums.MyTasksEnums;

namespace MyTasksEFDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TestdbContext _testdbContext;
        public TasksController(TestdbContext testdbContext)
        {
            _testdbContext = testdbContext;
        }

        [HttpPost]
        [Route("add-task")]
        public async Task<IActionResult> Create([FromBody] CreateTaskInputDTO input)
        {
            var message = "0";
            try
            {
                //check if user id exisit 
                if (!_testdbContext.Users.Any(x => x.Id == input.UserId))
                    throw new Exception("User Dose not Exisit");
                //end date grater than start date 
                if (input.StartDate >= input.EndDate)
                    throw new Exception("End Date Should be After Start Date");
                //title not empty and at least 5 character 
                if (string.IsNullOrEmpty(input.Title) || input.Title.Length < 5)
                    throw new Exception("Title is Required and Should be Containts at least 5 Character");
                Models.Task task = new Models.Task();
                task.Title = input.Title;
                task.Description = input.Description;
                task.Status = MyTaskStatus.New.ToString();
                task.Start = input.StartDate;
                task.End = input.EndDate;
                task.UserId = input.UserId;
                await _testdbContext.AddAsync(task);
                await _testdbContext.SaveChangesAsync();
                return StatusCode(200, message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update-task")]
        public async Task<IActionResult> Update([FromBody] UpdateTaskInputDTO input)
        {
            var message = "0";
            try
            {
                var task = _testdbContext.Tasks.Where(x=>x.Id == input.Id).SingleOrDefault();
                if(task != null)
                {
                    if (!string.IsNullOrEmpty(input.Title))
                    {
                        if (string.IsNullOrEmpty(input.Title) || input.Title.Length < 5)
                            throw new Exception("Title is Required and Should be Containts at least 5 Character");
                        else
                            task.Title = input.Title;
                    }
                    if (!string.IsNullOrEmpty(input.Description))
                    {
                        task.Description = input.Description;   
                    }
                    if (!string.IsNullOrEmpty(input.Status))
                    {
                        if (!Enum.TryParse(input.Status, false, out MyTaskStatus status))
                            throw new Exception("Invaliad Status Value ");
                        task.Status = input.Status; 
                    }
                    if(input.StartDate != null)
                    {
                        if(input.EndDate != null)
                        {
                            if (input.StartDate >= input.EndDate)
                                throw new Exception("End Date Should be After Start Date");
                        }
                        if (input.StartDate >= task.End)
                            throw new Exception("End Date Should be After Start Date");
                        task.Start = input.StartDate;
                    }
                    if (input.EndDate != null)
                    {
                        if (task.Start >= input.EndDate)
                            throw new Exception("End Date Should be After Start Date");
                        task.End = input.EndDate;
                    }
                    _testdbContext.Update(task);
                    _testdbContext.SaveChanges();
                }
                throw new Exception($"No Task With The Given Id {input.Id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
