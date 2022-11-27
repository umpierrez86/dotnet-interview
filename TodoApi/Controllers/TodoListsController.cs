using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
  [Route("api/todolists")]
  [ApiController]
  public class TodoListsController : ControllerBase
  {
    private readonly TodoContext _context;

    public TodoListsController(TodoContext context)
    {
      _context = context;
    }

    // GET: api/todolists
    [HttpGet]
    public async Task<ActionResult<IList<TodoList>>> GetTodoItems()
    {
      if (_context.TodoList == null)
      {
        return NotFound();
      }

      return Ok(await _context.TodoList.ToListAsync());
    }

    // GET: api/todolists/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoList>> GetTodoItem(long id)
    {
      if (_context.TodoList == null)
      {
        return NotFound();
      }

      var todoItem = await _context.TodoList.FindAsync(id);

      if (todoItem == null)
      {
        return NotFound();
      }

      return Ok(todoItem);
    }

    // PUT: api/todolists/5
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<ActionResult> PutTodoItem(long id, TodoList todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest();
      }

      _context.Entry(todoItem).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!TodoItemExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    // POST: api/todolists
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TodoList>> PostTodoItem(TodoList todoItem)
    {
      if (_context.TodoList == null)
      {
        return Problem("Entity set 'TodoContext.TodoItem'  is null.");
      }
      _context.TodoList.Add(todoItem);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
    }

    // DELETE: api/todolists/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodoItem(long id)
    {
      if (_context.TodoList == null)
      {
        return NotFound();
      }
      var todoItem = await _context.TodoList.FindAsync(id);
      if (todoItem == null)
      {
        return NotFound();
      }

      _context.TodoList.Remove(todoItem);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool TodoItemExists(long id)
    {
      return (_context.TodoList?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
