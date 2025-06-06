using Todo.ApplicationCore.Interfaces;
using TodoApi.Dtos;
using TodoApi.Models;

namespace Todo.ApplicationCore.Services;

public class ItemService : IItemsService
{
    private IRepository<Item> _repository;
    private IRepository<TodoList> _listRepository;
    
    public ItemService(IRepository<Item> repository, IRepository<TodoList> listRepository)
    {
        _repository = repository;
        _listRepository = listRepository;
    }

    public async Task<List<ReadItem>> Get(long listId, string name = "")
    {
        List<Item> items;
        if (string.IsNullOrEmpty(name))
        {
            items = await _repository.GetAll(item => item.TodoListId == listId);
        }
        else
        {
            items = await _repository.GetAll(item => item.Name == name && item.TodoListId == listId);
        }

        return items.Select(CreateReadItem).ToList();
    }

    public async Task<ReadItem> GetById(long id, long listId)
    {
        var result = await _repository.Get(item => item.Id == id && item.TodoListId == listId);

        return CreateReadItem(result);
    }

    public async Task<ReadItem> Create(long listId, CreateItem createItem)
    {
        var listExists = await _listRepository.Exist(list => list.Id == listId);

        if (!listExists)
        {
            throw new ArgumentException("List not found");
        }

        var newItem = new Item
        {
            Name = createItem.Name,
            Description = createItem.Description,
            TodoListId = listId
        };
        
        var result = await _repository.Add(newItem);
        
        return CreateReadItem(result);
    }

    public async Task<ReadItem> Update(long listId, long itemId, UpdateItem updateItem)
    {
        var itemToUpdate = await _repository
            .Get(item => item.Id == itemId && item.TodoListId == listId);
        
        if (itemToUpdate == null)
        {
            throw new ArgumentException("Item not found");
        }
        
        if (!string.IsNullOrEmpty(updateItem.Name) && updateItem.Name != itemToUpdate.Name)
        {
            itemToUpdate.Name = updateItem.Name;
        }

        if (!string.IsNullOrEmpty(updateItem.Description) && updateItem.Description != itemToUpdate.Description)
        {
            itemToUpdate.Description = updateItem.Description;
        }
        
        var result = await _repository.Update(itemToUpdate);
        return CreateReadItem(result);
    }

    public async Task Delete(long listId, long itemId)
    {
        var itemToDelete = await _repository.Get(i => i.Id == itemId && i.TodoListId == listId);

        if (itemToDelete == null)
        {
            throw new ArgumentException("Item not found");
        }

        await _repository.Remove(itemToDelete);
    }

    private ReadItem CreateReadItem(Item item)
    {
        return new ReadItem
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            isComplete = item.IsComplete,
        };
    }
}