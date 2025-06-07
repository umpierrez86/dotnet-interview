using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.ApplicationCore.Interfaces;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Tests;

public class ItemsControllerTest
{
    private Mock<IItemsService> _mockService;
    private ItemsController _controller;

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
        _mockService = new Mock<IItemsService>();
        _mockService.Setup(s => s.Get(1, ""))
            .ReturnsAsync(new List<ReadItem>
            {
                new ReadItem { Id = 1, Name = "Item 1", Description = "Desc 1" },
                new ReadItem { Id = 2, Name = "Item 2", Description = "Desc 2" }
            });

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.Get(1, "");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IList<ReadItem>>(okResult.Value);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetItem_WhenExists_ReturnsItem()
    {
        _mockService = new Mock<IItemsService>();
        _mockService.Setup(s => s.GetById(1, 1))
            .ReturnsAsync(new ReadItem { Id = 1, Name = "Item 1", Description = "Desc 1" });

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.GetItem(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<ReadItem>(okResult.Value);
        Assert.Equal("Item 1", item.Name);
    }

    [Fact]
    public async Task PostItem_WhenCalled_CreatesItem()
    {
        _mockService = new Mock<IItemsService>();
        var createdItem = new ReadItem { Id = 3, Name = "New Item", Description = "New Desc" };

        _mockService.Setup(s => s.Create(1, It.IsAny<CreateItem>()))
            .ReturnsAsync(createdItem);

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.PostItem(1, new CreateItem { Name = "New Item", Description = "New Desc" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var item = Assert.IsType<ReadItem>(createdResult.Value);
        Assert.Equal("New Item", item.Name);
    }

    [Fact]
    public async Task PatchItem_WhenCalled_UpdatesItem()
    {
        _mockService = new Mock<IItemsService>();
        var updatedItem = new ReadItem { Id = 1, Name = "Updated", Description = "Updated desc", isComplete = true };

        _mockService.Setup(s => s.Update(1, 1, It.IsAny<UpdateItem>()))
            .ReturnsAsync(updatedItem);

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.PatchItem(1, 1,
            new UpdateItem { Name = "Updated", Description = "Updated desc" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<ReadItem>(okResult.Value);
        Assert.Equal("Updated", item.Name);
    }
    
    [Fact]
    public async Task MarkComple_WhenCalled_UpdatesItemToComplete()
    {
        _mockService = new Mock<IItemsService>();
        var updatedItem = new ReadItem { Id = 1, Name = "Updated", Description = "Updated desc", isComplete = true };

        _mockService.Setup(s => s.MarkComplete(1, 1))
            .ReturnsAsync(updatedItem);

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.MarkAsComplete(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var item = Assert.IsType<ReadItem>(okResult.Value);
        Assert.Equal("Updated", item.Name);
    }

    [Fact]
    public async Task DeleteItem_WhenCalled_DeletesItem()
    {
        _mockService = new Mock<IItemsService>();
        _mockService.Setup(s => s.Delete(1, 1)).Returns(Task.CompletedTask);

        _controller = new ItemsController(_mockService.Object);
        var result = await _controller.DeleteItem(1, 1);

        Assert.IsType<NoContentResult>(result);
    }
}