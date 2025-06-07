using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.ApplicationCore.Interfaces;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly ITodoListsService _listService;

        public TodoListsController(ITodoListsService listService)
        {
            _listService = listService;
        }

        // GET: api/todolists
        [HttpGet]
        public async Task<ActionResult<IList<TodoList>>> GetTodoLists([FromQuery] string? name)
        {
            var result = await _listService.Get(name);
            return Ok(result);
        }

        // GET: api/todolists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoList>> GetTodoList(long id)
        {
            try
            {
                var result = await _listService.GetById(id);
                return Ok(result);
            }
            catch(ArgumentException)
            {
                return NotFound();
            }
        }

        // PUT: api/todolists/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoList(long id, UpdateTodoList payload)
        {
            try
            {
                var result = await _listService.Update(id, payload);
                return Ok(result);
            }
            catch(ArgumentException)
            {
                return NotFound();
            }
        }

        // POST: api/todolists
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList(CreateTodoList payload)
        {
            try
            {
                var result = await _listService.Create(payload);
                return CreatedAtAction("GetTodoList", new { id = result.Id }, result);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            try
            {
                await _listService.Delete(id);
                return NoContent();
            }
            catch(ArgumentException)
            {
                return NotFound();
            }
        }

        private async Task<bool> TodoListExists(long id)
        {
            return await _listService.Exists(id);
        }
    }
}
