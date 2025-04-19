using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTasksEFDbFirst.DTOs.Tasks.Request;
using MyTasksEFDbFirst.Models;
using MyTasksEFDbFirstNew.DTOs.Tasks.Response;
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
                //chec   k if user id exisit 
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

        [HttpGet("get/{Id}")]
        public async Task<IActionResult> Get([FromRoute] int Id)
        {
            try
            {
                var task = _testdbContext.Tasks.Where(x => x.Id == Id).SingleOrDefault();
                if (task != null)
                {
                    return Ok(task);
                }
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
            
        }
        [HttpGet("tasks")]
        public async Task<IActionResult> ListAll()
        {
            try
            {
                var items = _testdbContext.Tasks.ToList();
                var list = new List<TaskDTO>();
                foreach (var item in items) {
                    list.Add(new TaskDTO
                    {
                        Id= item.Id,
                        Title = item.Title,
                        DeadLine = item.End.ToString(),
                        Status = item.Status
                    });
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("tasks-select")]
        public async Task<IActionResult> ListAll2()
        {
            try
            {
                var items = _testdbContext.Tasks.Select(x => new TaskDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    DeadLine = x.End.ToString(),
                    Status = x.Status
                }).ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("tasks-query")]
        public async Task<IActionResult> ListAll3()
        {
            try
            {
                //select Id ,Ttile, Status,End As  DeadLine From Tasks 
                var items = from t in _testdbContext.Tasks
                            select new TaskDTO
                            {
                                Id = t.Id,
                                Title = t.Title,
                                DeadLine = t.End.ToString(),
                                Status = t.Status
                            };
                return Ok(items.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("remove/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var task = _testdbContext.Tasks.Where(x => x.Id == Id).SingleOrDefault();
                if (task != null)
                {
                    _testdbContext.Remove(task);
                    _testdbContext.SaveChanges();
                    return Ok("removed");
                }
                throw new Exception("No Task Exisit With The Selected Id");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
