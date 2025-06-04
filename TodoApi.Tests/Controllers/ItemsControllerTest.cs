using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Tests;

public class ItemsControllerTest
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
        context.Items.Add(new Item { Id = 1, Name = "Item 1", Description = "Desc 1", TodoListId = 1 });
        context.Items.Add(new Item { Id = 2, Name = "Item 2", Description = "Desc 2", TodoListId = 1 });
        context.SaveChanges();
    }
    
    [Fact]
    public async Task GetItems_WhenCalled_ReturnsItems()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new ItemsController(context);

        var result = await controller.Get(1, "");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IList<Item>>(okResult.Value);
        Assert.Equal(2, items.Count);
    }
    
    [Fact]
    public async Task GetItem_WhenExists_ReturnsItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new ItemsController(context);

        var result = await controller.GetItem(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<Item>(okResult.Value);
        Assert.Equal("Item 1", item.Name);
    }
    
    [Fact]
    public async Task PostItem_WhenCalled_CreatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new ItemsController(context);

        var createDto = new CreateItem { Name = "New Item", Description = "New Desc" };

        var result = await controller.PostItem(1, createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdItem = Assert.IsType<Item>(createdResult.Value);
        Assert.Equal("New Item", createdItem.Name);
        Assert.Equal(3, context.Items.Count());
    }
    
    [Fact]
    public async Task PatchItem_WhenCalled_UpdatesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new ItemsController(context);

        var updateDto = new UpdateItem { Name = "Updated", Description = "Updated desc" };

        var result = await controller.PatchItem(1, 1, updateDto);
        
        var updatedItem = await context.Items.FirstOrDefaultAsync(i => i.Id == 1);
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedItem.Description, updateDto.Description);
    }
    
    [Fact]
    public async Task DeleteItem_WhenCalled_DeletesItem()
    {
        using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new ItemsController(context);

        var result = await controller.DeleteItem(1, 1);

        Assert.IsType<NoContentResult>(result.Result);
        Assert.Single(context.Items);
    }
}