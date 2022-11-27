using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
  [Route("api/todoitems")]
  [ApiController]
  public class TodoItemsController : ControllerBase
  {
    private readonly TodoContext _context;

    public TodoItemsController(TodoContext context)
    {
      _context = context;
    }

    // GET: api/todoitems
    [HttpGet]
    public async Task<ActionResult<IList<TodoItem>>> GetTodoItems()
    {
      if (_context.TodoItem == null)
      {
        return NotFound();
      }

      return Ok(await _context.TodoItem.ToListAsync());
    }

    // GET: api/todoitems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
    {
      if (_context.TodoItem == null)
      {
        return NotFound();
      }

      var todoItem = await _context.TodoItem.FindAsync(id);

      if (todoItem == null)
      {
        return NotFound();
      }

      return Ok(todoItem);
    }

    // PUT: api/todoitems/5
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<ActionResult> PutTodoItem(long id, TodoItem todoItem)
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

    // POST: api/todoitems
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
      if (_context.TodoItem == null)
      {
        return Problem("Entity set 'TodoContext.TodoItem'  is null.");
      }
      _context.TodoItem.Add(todoItem);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
    }

    // DELETE: api/todoitems/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodoItem(long id)
    {
      if (_context.TodoItem == null)
      {
        return NotFound();
      }
      var todoItem = await _context.TodoItem.FindAsync(id);
      if (todoItem == null)
      {
        return NotFound();
      }

      _context.TodoItem.Remove(todoItem);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool TodoItemExists(long id)
    {
      return (_context.TodoItem?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
