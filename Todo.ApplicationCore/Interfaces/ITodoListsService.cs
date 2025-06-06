using TodoApi.Dtos;

namespace Todo.ApplicationCore.Interfaces;

public interface ITodoListsService
{
    Task<List<ReadTodoList>> Get(string name = "");
    Task<ReadTodoList> GetById(long id);
    Task<ReadTodoList> Update(long id, UpdateTodoList updatedList);
    Task<ReadTodoList> Create(CreateTodoList todoList);
    Task Delete(long id);
    Task<bool> Exists(long id);
}