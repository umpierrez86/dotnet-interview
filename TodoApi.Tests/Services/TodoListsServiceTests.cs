using System.Linq.Expressions;
using Moq;
using Todo.ApplicationCore.Services;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoMcpServer.Interfaces;

namespace TodoApi.Tests.Services;

public class TodoListsServiceTests
{
    private readonly Mock<ITodoListRepository> _mockListRepo;
    private readonly TodoListsService _service;

    public TodoListsServiceTests()
    {
        _mockListRepo = new Mock<ITodoListRepository>();
        _service = new TodoListsService(_mockListRepo.Object);
    }
    
    [Fact]
    public async Task Get_ByName_NotFound_ReturnsEmptyList()
    {
        _mockListRepo.Setup(r => r.GetAllWithItems(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync(new List<TodoList>());

        var result = await _service.Get("NonExistentList");

        Assert.Empty(result);
    }

    [Fact]
    public async Task Get_WhenCalledWithoutName_ReturnsAllListsWithItems()
    {
        var lists = new List<TodoList> {
            new TodoList { Id = 1, Name = "List1", Items = new List<Item>() }
        };
        _mockListRepo.Setup(r => r.GetAllWithItems(null)).ReturnsAsync(lists);

        var result = await _service.Get();

        Assert.Single(result);
        Assert.Equal("List1", result[0].Name);
    }

    [Fact]
    public async Task GetById_WhenCalled_ReturnsListWithItems()
    {
        var list = new TodoList { Id = 1, Name = "List1", Items = new List<Item>() };
        _mockListRepo.Setup(r => r.GetWithItems(It.IsAny<Expression<Func<TodoList, bool>>>())).ReturnsAsync(list);

        var result = await _service.GetById(1);

        Assert.Equal("List1", result.Name);
    }
    
    [Fact]
    public async Task GetById_ListNotFound_ThrowsException()
    {
        var mockRepo = new Mock<ITodoListRepository>();
        mockRepo.Setup(r => r.GetWithItems(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync((TodoList)null!);

        var service = new TodoListsService(mockRepo.Object);

        await Assert.ThrowsAsync<NullReferenceException>(() => service.GetById(404)); // o tu excepción custom si la usás
    }

    [Fact]
    public async Task Create_WhenCalled_AddsNewList()
    {
        var toCreate = new CreateTodoList { Name = "NewList" };
        var created = new TodoList { Id = 1, Name = "NewList" };
        _mockListRepo.Setup(r => r.Add(It.IsAny<TodoList>())).ReturnsAsync(created);

        var result = await _service.Create(toCreate);

        Assert.Equal("NewList", result.Name);
    }
    
    [Fact]
    public async Task Update_ValidData_ReturnsUpdatedList()
    {
        var list = new TodoList { Id = 1, Name = "Old Name" };
        var updatedList = new TodoList { Id = 1, Name = "New Name" };

        _mockListRepo.Setup(r => r.Get(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync(list);

        _mockListRepo.Setup(r => r.Update(It.IsAny<TodoList>()))
            .ReturnsAsync(updatedList);

        var result = await _service.Update(1, new UpdateTodoList { Name = "New Name" });

        Assert.Equal("New Name", result.Name);
    }
    
    [Fact]
    public async Task Update_ListNotFound_ThrowsException()
    {
        var mockRepo = new Mock<ITodoListRepository>();
        mockRepo.Setup(r => r.Get(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync((TodoList)null!);

        var service = new TodoListsService(mockRepo.Object);
        var update = new UpdateTodoList { Name = "Updated Name" };

        await Assert.ThrowsAsync<NullReferenceException>(() => service.Update(999, update));
    }

    [Fact]
    public async Task Delete_WhenCalled_RemovesList()
    {
        var list = new TodoList { Id = 1, Name = "ToDelete" };
        _mockListRepo.Setup(r => r.Get(It.IsAny<Expression<Func<TodoList, bool>>>())).ReturnsAsync(list);

        await _service.Delete(1);

        _mockListRepo.Verify(r => r.Remove(list), Times.Once);
    }
    
    [Fact]
    public async Task Exists_ListDoesNotExist_ReturnsFalse()
    {
        var mockRepo = new Mock<ITodoListRepository>();
        mockRepo.Setup(r => r.Exist(It.IsAny<Expression<Func<TodoList, bool>>>()))
            .ReturnsAsync(false);

        var service = new TodoListsService(mockRepo.Object);

        var exists = await service.Exists(888);

        Assert.False(exists);
    }
}