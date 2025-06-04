using TodoApi.Dtos;

namespace Todo.ApplicationCore.Interfaces;

public interface IItemsService
{
     Task<List<ReadItem>> Get(long listId, string name = "");
     Task<ReadItem> GetById(long id, long listId);
     Task<ReadItem> Update(long listId, long itemId, UpdateItem updateItem);
     Task<ReadItem> Create(long listId, CreateItem createItem);
     Task Delete(long listId, long itemId);
}