namespace Api;

internal class Todos
{
    private readonly ILogger _logger;

    private readonly TodoHandler _todoHandler;

    public Todos(ILoggerFactory loggerFactory, TodoHandler todoHandler)
    {
        _logger = loggerFactory.CreateLogger<Todos>();
        _todoHandler = todoHandler;
    }

    [Function($"{nameof(Todos)}_GetList")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")]
        HttpRequestData request)
    {
        ClientPrincipal clientPrincipal =
            StaticWebAppApiAuthorization
                .ParseHttpHeaderForClientPrinciple(request.Headers.ToList());

        List<Todo> todos = await _todoHandler.GetAllTodosForUser(clientPrincipal);
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(todos);
        return response;
    }

    [Function($"{nameof(Todos)}_Post")]
    public async Task<HttpResponseData> PostTodo(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "todos")]
        HttpRequestData request)
    {
        Todo? todo = await request.ReadFromJsonAsync<Todo>();
        if (todo is null || todo.Id != default)
        {
            var badResponse = request.CreateResponse(HttpStatusCode.NotFound);
            badResponse.WriteString("ID must not be filled");

            return badResponse;
        }

        var clientPrincipal =
            StaticWebAppApiAuthorization
                .ParseHttpHeaderForClientPrinciple(request.Headers.ToList());

        await _todoHandler.CreateTodo(todo, clientPrincipal);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(todo);
        return response;
    }

    [Function($"{nameof(Todos)}_Put")]
    public async Task<HttpResponseData> PutTodo(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put",
            Route = "todos")]
        HttpRequestData request)
    {
        Todo? todo = await request.ReadFromJsonAsync<Todo>();
        if (todo is null)
        {
            var badRequest = request.CreateResponse(HttpStatusCode.NotFound);
            badRequest.WriteString("No ToDo passed");
            return badRequest;

        }

        var clientPrincipal =
            StaticWebAppApiAuthorization
                .ParseHttpHeaderForClientPrinciple(request.Headers.ToList());

        try
        {
            await _todoHandler.UpdateTodo(todo, clientPrincipal);
        }
        catch(ArgumentException) { 
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        return request.CreateResponse(HttpStatusCode.OK);
    }
}
