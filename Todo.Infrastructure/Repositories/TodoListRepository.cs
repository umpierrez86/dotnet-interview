using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Todo.ApplicationCore.Exceptions;
using TodoApi.Models;
using TodoMcpServer.Interfaces;

namespace Todo.Infrastructure.Repositories;

public class TodoListRepository(DbContext context) : Repository<TodoList>(context), ITodoListRepository
{
    private readonly DbSet<TodoList> _entities = context.Set<TodoList>();
    
    public async Task<List<TodoList>> GetAllWithItems(Expression<Func<TodoList, bool>>? predicate = null)
    {
        return predicate == null ? await _entities.Include(list => list.Items).ToListAsync() 
            : await _entities.Where(predicate).Include(list => list.Items).ToListAsync();
    }
    
    public async Task<TodoList> GetWithItems(Expression<Func<TodoList, bool>> predicate)
    {
        var entity = await _entities.Include(list => list.Items)
            .FirstOrDefaultAsync(predicate);

        if(entity == null)
        {
            throw new NotFoundException($"Todo list not found");
        }

        return entity;
    }
}