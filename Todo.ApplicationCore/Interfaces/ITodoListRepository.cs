using System.Linq.Expressions;
using Todo.ApplicationCore.Interfaces;
using TodoApi.Models;

namespace TodoMcpServer.Interfaces;

public interface ITodoListRepository : IRepository<TodoList>
{
    Task<List<TodoList>> GetAllWithItems(Expression<Func<TodoList, bool>>? predicate = null);
    Task<TodoList> GetWithItems(Expression<Func<TodoList, bool>> predicate);
}