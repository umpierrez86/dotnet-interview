using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("/todoLists/{listId}/items")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly TodoContext _context;

    public ItemsController(TodoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IList<Item>>> Get(long listId)
    {
        var items = await _context.Items
            .FirstOrDefaultAsync(i => i.TodoListId == listId);

        if (items == null)
        {
            return NotFound();
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

        if (updateItem.IsComplete != itemToUpdate.IsComplete)
        {
            itemToUpdate.IsComplete = updateItem.IsComplete;
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(itemToUpdate);
    }

    [HttpPost]
    public async Task<ActionResult<Item>> PostItem(long listId, UpdateItem updateItem)
    {
        var listExists = await _context.TodoList.AnyAsync(list => list.Id == listId);

        if (!listExists)
        {
            return NotFound();
        }

        var newItem = new Item
        {
            Name = updateItem.Name,
            Description = updateItem.Description,
            TodoListId = listId
        };
        
        _context.Items.Add(newItem);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(
            nameof(GetItem),
            routeValues: new { listId, id = newItem.Id },
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