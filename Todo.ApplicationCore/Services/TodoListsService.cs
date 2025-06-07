using Todo.ApplicationCore.Interfaces;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoMcpServer.Interfaces;

namespace Todo.ApplicationCore.Services;

public class TodoListsService : ITodoListsService
{
    private ITodoListRepository _listRepository;
    
    public TodoListsService(ITodoListRepository listRepository)
    {
        _listRepository = listRepository;
    }

    public async Task<List<ReadTodoList>> Get(string name = "")
    {
        List<TodoList> todoList;
        if (string.IsNullOrEmpty(name))
        {
            todoList = await _listRepository.GetAllWithItems();
        }
        else
        {
            todoList = await _listRepository.GetAllWithItems(list => list.Name == name && list.Name == name);
        }

        return todoList.Select(CreateReadTodoList).ToList();
    }

    public async Task<ReadTodoList> GetById(long id)
    {
        var result = await _listRepository.GetWithItems(list => list.Id == id);
        return CreateReadTodoList(result);
    }

    public async Task<ReadTodoList> Update(long id, UpdateTodoList updatedList)
    {
        var listToUpdate = await _listRepository.Get(list => list.Id == id);
        listToUpdate.Name = updatedList.Name;
        
        var result = await _listRepository.Update(listToUpdate);
        return CreateReadTodoList(result);
    }

    public async Task<ReadTodoList> Create(CreateTodoList todoList)
    {
        TodoList newList = new TodoList
        {
            Name = todoList.Name
        };
        
        var result = await _listRepository.Add(newList);
        return CreateReadTodoList(result);
    }

    public async Task Delete(long id)
    {
        var listToRemove = await _listRepository.Get(list => list.Id == id);
        await _listRepository.Remove(listToRemove);
    }

    public async Task<bool> Exists(long id)
    {
        return await _listRepository.Exist(list => list.Id == id);
    }

    private ReadTodoList CreateReadTodoList(TodoList list)
    {
        List<ReadItem> readItems = [];
        foreach (var item in list.Items)
        {
            readItems.Add(new ReadItem
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
            });
        }
        
        return new ReadTodoList
        {
            Id = list.Id,
            Name = list.Name,
            Items = readItems,
        };
    }
}