using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

namespace Client.Services;

public class TodoService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private List<Todo>? todos;
    public event Action? RefreshRequested;

    public IReadOnlyList<Todo>? CompleteTodos => todos?.Where(todo => todo.Complete).ToList();
    public IReadOnlyList<Todo>? NotCompleteTodos => todos?.Where(todo => !todo.Complete).ToList();

    public TodoService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentNullException.ThrowIfNull(authenticationStateProvider, nameof(authenticationStateProvider));

        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task LoadTodos()
    {
        if (todos == null)
        {
            if (await IsAuthenticated())
            {
                todos = await _httpClient.GetFromJsonAsync<List<Todo>>("api/todos");
            }
            else
            {
                todos = new List<Todo>();
            }
        }
    }

    public async Task AddTodo(Todo todo)
    {
        if (todos is null)
        {
            return;
        }

        if (await IsAuthenticated())
        {
            await _httpClient.PostAsJsonAsync("api/todos", todo);
        }
        else
        {
            todo.Id = Guid.NewGuid().ToString();
        }

        todos.Add(todo);
        RefreshRequested?.Invoke();
    }

    public async Task UpdateTodo(Todo todo)
    {
        if (todos is null)
        {
            return;
        }

        var storedTodo = todos.Find(t => t.Id == todo.Id);
        if (storedTodo is null)
        {
            throw new ArgumentException($"Cannot find Todo with id {todo.Id}");
        }

        if (await IsAuthenticated())
        {
            var putResult = await _httpClient.PutAsJsonAsync("api/todos", todo);

            putResult.EnsureSuccessStatusCode();
        }

        storedTodo.Label = todo.Label;
        storedTodo.Complete = todo.Complete;

        RefreshRequested?.Invoke();
    }

    private async Task<bool> IsAuthenticated()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.HasClaim(ClaimTypes.Role, "authorised");
    }
}
