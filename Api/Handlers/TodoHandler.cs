using Api.Models;
using CosmosDBAccessor;
using Microsoft.Azure.Cosmos;

namespace Api.Handlers;
public class TodoHandler
{
    private readonly IContainerAccess<CosmosTodo> cosmosAccessor;

    public TodoHandler(IContainerAccess<CosmosTodo> cosmosAccessor)
    {
        this.cosmosAccessor = cosmosAccessor;
    }

    public async Task<List<Todo>> GetAllTodosForUser(ClientPrincipal clientPrincipal)
    {
        var todos = await cosmosAccessor.GetItems(clientPrincipal.UserId);
        return todos.Select(todo => new Todo { Id = todo.Id, Label = todo.Label, Complete = todo.Complete }).ToList();
    }

    public async Task CreateTodo(Todo todo, ClientPrincipal clientPrincipal)
    {
        todo.Id = Guid.NewGuid().ToString();

        CosmosTodo cosmosTodo = new()
        {
            Id = todo.Id,
            Label = todo.Label,
            Complete = todo.Complete,
            userId = clientPrincipal.UserId
        };
        await cosmosAccessor.Create(cosmosTodo);
    }

    public async Task UpdateTodo(Todo todo, ClientPrincipal clientPrincipal)
    {
        var currentTodo = await cosmosAccessor.Get(todo.Id!, clientPrincipal.UserId);
        if (currentTodo is null) { throw new ArgumentException($"Todo not found"); }

        List<PatchOperation> operations = new()
        {
            PatchOperation.Replace($"/label", todo.Label),
            PatchOperation.Replace("/complete", todo.Complete)
        };

        await cosmosAccessor.Patch(currentTodo, operations);
    }
}
