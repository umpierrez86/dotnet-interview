using System.Linq.Expressions;

namespace Todo.ApplicationCore.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<bool> Exist(Expression<Func<TEntity, bool>> expression);

    Task<TEntity> Add(TEntity entity);

    Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? predicate = null);

    Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> Remove(TEntity entity);

    Task<TEntity> Update(TEntity entity);
}