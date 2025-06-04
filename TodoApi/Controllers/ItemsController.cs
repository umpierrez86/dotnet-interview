using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("api/todoLists/{listId}/items")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly TodoContext _context;

    public ItemsController(TodoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IList<Item>>> Get(long listId, [FromQuery] string name)
    {
        var items = await _context.Items
            .Where(item => item.TodoListId == listId)
            .ToListAsync();

        if (!string.IsNullOrEmpty(name))
        {
            items = items.Where(item => item.Name == name).ToList();
        }
        
        return Ok(items);
    }

    [HttpGet("{itemId}")]
    public async Task<ActionResult<IList<Item>>> GetItem(long listId, long itemId)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TodoListId == listId);

        if (item == null)
        {
            return NotFound();
        }
        
        return Ok(item);
    }

    [HttpPut("{itemId}")]
    public async Task<ActionResult<Item>> PutItem(long listId, long itemId, UpdateItem updateItem)
    {
        var itemToUpdate = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TodoListId == listId);

        if (itemToUpdate == null)
        {
            return NotFound();
        }
        
        if (!string.IsNullOrEmpty(updateItem.Name) && updateItem.Name != itemToUpdate.Name)
        {
            itemToUpdate.Name = updateItem.Name;
        }

        if (!string.IsNullOrEmpty(updateItem.Description) && updateItem.Description != itemToUpdate.Description)
        {
            itemToUpdate.Description = updateItem.Description;
        }

        if (!itemToUpdate.IsComplete && updateItem.IsComplete)
        {
            itemToUpdate.IsComplete = updateItem.IsComplete;
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(itemToUpdate);
    }

    [HttpPost]
    public async Task<ActionResult<Item>> PostItem(long listId, CreateItem createItem)
    {
        Console.WriteLine(listId);
        var listExists = await _context.TodoList.AnyAsync(list => list.Id == listId);

        if (!listExists)
        {
            return NotFound();
        }

        var newItem = new Item
        {
            Name = createItem.Name,
            Description = createItem.Description,
            TodoListId = listId
        };
        
        _context.Items.Add(newItem);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(
            nameof(GetItem),
            routeValues: new { listId, itemId = newItem.Id },
            value: newItem);
    }

    [HttpDelete("{itemId}")]
    public async Task<ActionResult<Item>> DeleteItem(long listId, long itemId)
    {
        var itemToDelete = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TodoListId == listId);

        if (itemToDelete == null)
        {
            return NotFound();
        }
        
        _context.Items.Remove(itemToDelete);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}