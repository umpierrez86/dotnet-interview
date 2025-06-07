using System.Linq.Expressions;
using Moq;
using Todo.ApplicationCore.Exceptions;
using Todo.ApplicationCore.Interfaces;
using Todo.ApplicationCore.Services;
using TodoApi.Models;
using TodoApi.Dtos;

namespace TodoApi.Tests.Services;

public class ItemServiceTests
{
    private readonly Mock<IRepository<Item>> _mockItemRepo;
    private readonly Mock<IRepository<TodoList>> _mockListRepo;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _mockItemRepo = new Mock<IRepository<Item>>();
        _mockListRepo = new Mock<IRepository<TodoList>>();
        _service = new ItemService(_mockItemRepo.Object, _mockListRepo.Object);
    }
    
    [Fact]
    public async Task Get_WithoutNameFilter_ReturnsItems()
    {
        var items = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", TodoListId = 1 },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", TodoListId = 1 }
        };

        _mockItemRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<Item, bool>>>()))
            .ReturnsAsync(items);

        var result = await _service.Get(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Name == "Item 1");
    }
    
    [Fact]
    public async Task GetById_ValidIds_ReturnsItem()
    {
        var item = new Item
        {
            Id = 10,
            Name = "Test",
            Description = "Test Desc",
            TodoListId = 5
        };

        _mockItemRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>()))
            .ReturnsAsync(item);

        var result = await _service.GetById(10, 5);

        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task Create_WhenListExists_AddsItem()
    {
        _mockListRepo.Setup(r => r.Exist(It.IsAny<Expression<Func<TodoList, bool>>>())).ReturnsAsync(true);

        var toCreate = new CreateItem { Name = "Item1", Description = "Desc" };
        var created = new Item { Id = 1, Name = "Item1", Description = "Desc" };
        _mockItemRepo.Setup(r => r.Add(It.IsAny<Item>())).ReturnsAsync(created);

        var result = await _service.Create(1, toCreate);

        Assert.Equal("Item1", result.Name);
    }
    
    [Fact]
    public async Task Create_ListDoesNotExist_ThrowsNotFoundException()
    {
        var mockRepo = new Mock<IRepository<Item>>();
        var mockListRepo = new Mock<IRepository<TodoList>>();
        mockListRepo.Setup(r => r.Exist(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync(false);

        var service = new ItemService(mockRepo.Object, mockListRepo.Object);

        var createItem = new CreateItem { Name = "Test", Description = "desc" };

        await Assert.ThrowsAsync<NotFoundException>(() => service.Create(123, createItem));
    }
    
    [Fact]
    public async Task Update_ValidUpdate_ReturnsUpdatedItem()
    {
        var itemId = 1L;
        var listId = 2L;
        var existingItem = new Item
        {
            Id = itemId,
            Name = "Old Name",
            Description = "Old Desc",
            TodoListId = listId
        };

        var updatedItem = new Item
        {
            Id = itemId,
            Name = "New Name",
            Description = "New Desc",
            TodoListId = listId
        };

        _mockItemRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>()))
            .ReturnsAsync(existingItem);

        _mockItemRepo.Setup(r => r.Update(It.IsAny<Item>()))
            .ReturnsAsync(updatedItem);
        
        var update = new UpdateItem { Name = "New Name", Description = "New Desc" };
        var result = await _service.Update(listId, itemId, update);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task Delete_WhenItemExists_RemovesItem()
    {
        var item = new Item
        {
            Id = 1,
            TodoListId = 1,
            Name = "The item",
            Description = "The new item"
        };
        _mockItemRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>())).ReturnsAsync(item);

        await _service.Delete(1, 1);

        _mockItemRepo.Verify(r => r.Remove(item), Times.Once);
    }
    
    [Fact]
    public async Task Delete_ItemNotFound_ThrowsNotFoundException()
    {
        var mockRepo = new Mock<IRepository<Item>>();
        var mockListRepo = new Mock<IRepository<TodoList>>();
        mockRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>()))
            .ReturnsAsync((Item)null!);

        var service = new ItemService(mockRepo.Object, mockListRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.Delete(1, 999));
    }

    [Fact]
    public async Task MarkComplete_WhenItemNotComplete_SetsComplete()
    {
        var item = new Item
        {
            Id = 1,
            TodoListId = 1,
            IsComplete = false,
            Name = "Item",
            Description = "The new item"
        };
        _mockItemRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>())).ReturnsAsync(item);
        _mockItemRepo.Setup(r => r.Update(It.IsAny<Item>())).ReturnsAsync(item);

        var result = await _service.MarkComplete(1, 1);

        Assert.True(result.isComplete);
    }
    
    [Fact]
    public async Task MarkComplete_ItemNotFound_ThrowsNotFoundException()
    {
        var mockRepo = new Mock<IRepository<Item>>();
        var mockListRepo = new Mock<IRepository<TodoList>>();
        mockRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Item, bool>>>()))
            .ReturnsAsync((Item)null!);

        var service = new ItemService(mockRepo.Object, mockListRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.MarkComplete(1, 99));
    }
}