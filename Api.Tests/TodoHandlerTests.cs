using Moq;
using Api.Models;
using CosmosDBAccessor;
using Api.Handlers;
using Models;
using StaticWebAppAuthentication.Models;
using FluentAssertions;

namespace Api.Tests;

public class TodoHandlerTests
{
    [Fact]
    public async Task Should_Update_Id_On_Create()
    {
        var mock = new Mock<IContainerAccess<CosmosTodo>>();
        mock.Setup(accessor => accessor.Create(It.IsAny<CosmosTodo>()));

        TodoHandler sut = new TodoHandler(mock.Object);
        Todo todo = new();
        await sut.CreateTodo(todo, new ClientPrincipal());

        todo.Id.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Return_Todo_List_On_Get()
    {
        var mock = new Mock<IContainerAccess<CosmosTodo>>();

        var cosmosTodo = new CosmosTodo
        {
            Complete = false,
            Id = "id",
            Label = "label",
            userId = "userId",
        };

        mock.Setup(accessor => accessor.GetItems(It.IsAny<string>())).ReturnsAsync(new List<CosmosTodo> { cosmosTodo });
        var expected = new List<Todo> { new Todo { Complete = cosmosTodo.Complete, Label = cosmosTodo.Label, Id = cosmosTodo.Id } };

        TodoHandler sut = new TodoHandler(mock.Object);
        var result = await sut.GetAllTodosForUser(new ClientPrincipal());

        result.Should().BeEquivalentTo(expected);
    }
}