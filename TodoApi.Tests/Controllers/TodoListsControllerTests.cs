using Microsoft.AspNetCore.Mvc;
using Moq;
using Todo.ApplicationCore.Interfaces;
using TodoApi.Controllers;
using TodoApi.Dtos;

namespace TodoApi.Tests
{
    public class TodoListsControllerTests
    {
        private Mock<ITodoListsService> _serviceMock;
        private TodoListsController _controller;

        [Fact]
        public async Task GetTodoList_WhenCalled_ReturnsTodoListList()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            var todoLists = new List<ReadTodoList>
            {
                new ReadTodoList { Id = 1, Name = "Task 1", Items = new() },
                new ReadTodoList { Id = 2, Name = "Task 2", Items = new() }
            };
            _serviceMock.Setup(s => s.Get("")).ReturnsAsync(todoLists);

            var result = await _controller.GetTodoLists("");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var lists = Assert.IsAssignableFrom<IList<ReadTodoList>>(okResult.Value);
            Assert.Equal(2, lists.Count);
        }

        [Fact]
        public async Task GetTodoList_WhenCalled_ReturnsTodoListById()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            var todoList = new ReadTodoList { Id = 1, Name = "Task 1", Items = new() };

            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(todoList);

            var result = await _controller.GetTodoList(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedList = Assert.IsType<ReadTodoList>(okResult.Value);
            Assert.Equal(1, returnedList.Id);
        }

        [Fact]
        public async Task PutTodoList_WhenTodoListDoesntExist_ReturnsNotFound()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            _serviceMock.Setup(s => s.Update(It.IsAny<long>(), It.IsAny<UpdateTodoList>()))
                .ThrowsAsync(new ArgumentException());

            var result = await _controller.PutTodoList(3, new UpdateTodoList { Name = "Task 3" });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutTodoList_WhenCalled_UpdatesTheTodoList()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            var updatedList = new ReadTodoList { Id = 2, Name = "Changed Task 2", Items = new() };

            _serviceMock.Setup(s => s.Update(2, It.IsAny<UpdateTodoList>())).ReturnsAsync(updatedList);

            var result = await _controller.PutTodoList(2, new UpdateTodoList { Name = "Changed Task 2" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<ReadTodoList>(okResult.Value);
            Assert.Equal("Changed Task 2", returnedList.Name);
        }

        [Fact]
        public async Task PostTodoList_WhenCalled_CreatesTodoList()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            var createdList = new ReadTodoList { Id = 3, Name = "Task 3", Items = new() };

            _serviceMock.Setup(s => s.Create(It.IsAny<CreateTodoList>())).ReturnsAsync(createdList);

            var result = await _controller.PostTodoList(new CreateTodoList { Name = "Task 3" });

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedList = Assert.IsType<ReadTodoList>(createdAtResult.Value);
            Assert.Equal(3, returnedList.Id);
        }

        [Fact]
        public async Task DeleteTodoList_WhenCalled_RemovesTodoList()
        {
            _serviceMock = new Mock<ITodoListsService>();
            _controller = new TodoListsController(_serviceMock.Object);

            _serviceMock.Setup(s => s.Delete(2)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteTodoList(2);

            Assert.IsType<NoContentResult>(result);
        }
    }
}

