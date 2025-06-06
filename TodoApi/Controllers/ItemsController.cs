using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.ApplicationCore.Interfaces;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("api/todoLists/{listId}/items")]
[ApiController]
public class ItemsController : ControllerBase
{
    private IItemsService _itemsService;
        
    public ItemsController(IItemsService itemsService)
    {
        _itemsService = itemsService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<ReadItem>>> Get(long listId, [FromQuery] string name)
    {
        var items = await _itemsService.Get(listId, name);
        
        return Ok(items);
    }

    [HttpGet("{itemId}")]
    public async Task<ActionResult<IList<ReadItem>>> GetItem(long listId, long itemId)
    {
        try
        {
            var item = await _itemsService.GetById(listId, itemId);
            return Ok(item);
        }
        catch(ArgumentNullException)
        {
            return NotFound();
        }
    }

    [HttpPut("{itemId}")]
    public async Task<ActionResult<ReadItem>> PutItem(long listId, long itemId, UpdateItem updateItem)
    {
        try
        {
            var item = await _itemsService.Update(listId, itemId, updateItem);
            return Ok(item);
        }
        catch(ArgumentNullException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ReadItem>> PostItem(long listId, CreateItem createItem)
    {
        try
        {
            var item = await _itemsService.Create(listId, createItem);
            
            return CreatedAtAction(
                nameof(GetItem),
                routeValues: new { listId, itemId = item.Id },
                value: item);
        }
        catch (ArgumentNullException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{itemId}")]
    public async Task<ActionResult> DeleteItem(long listId, long itemId)
    {
        try
        {
            await _itemsService.Delete(listId, itemId);
            
            return NoContent();
        }
        catch (ArgumentNullException)
        {
            return NotFound();
        }
    }
}