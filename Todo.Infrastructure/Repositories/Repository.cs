using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Todo.ApplicationCore.Interfaces;

namespace Todo.Infrastructure.Repositories;

public class Repository<TEntity>(DbContext context) : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();
    
    public async Task<bool> Exist(Expression<Func<TEntity, bool>> predicate)
    {
        return await _entities.AnyAsync(predicate);
    }

    public async Task<TEntity> Add(TEntity entity)
    {
        await _entities.AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null ? await _entities.ToListAsync() : await _entities.Where(predicate).ToListAsync();
    }

    public async Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
    {
        var entity = await _entities.FirstOrDefaultAsync(predicate);

        if(entity == null)
        {
            throw new ArgumentException($"Entity {typeof(TEntity).Name} not found");
        }

        return entity;
    }

    public async Task<TEntity> Remove(TEntity entity)
    {
        _entities.Remove(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public async Task<TEntity> Update(TEntity entity)
    {
        _entities.Update(entity);
        await context.SaveChangesAsync();

        return entity;
    }
}