using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Tools;

namespace TodoApi.Tests.Tools;

public class DeleteItemToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenItemIsDeleted()
    {
        var listId = 1;
        var itemId = 42;

        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out var handlerMock);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(listId);
        lookupServiceMock
            .Setup(x => x.GetItemIdByNameAsync(listId, "Milk", It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemId);

        var tool = new DeleteItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenMissingArguments()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.NoContent), out _);
        var lookupService = new Mock<ITodoLookupService>().Object;

        var tool = new DeleteItemTool(httpClient, lookupService);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenLookupFails()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.NoContent), out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("List not found"));

        var tool = new DeleteItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiCallFails()
    {
        var listId = 1;
        var itemId = 42;
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Item not found")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(listId);
        lookupServiceMock
            .Setup(x => x.GetItemIdByNameAsync(listId, "Milk", It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemId);

        var tool = new DeleteItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response, out Mock<HttpMessageHandler> handlerMock)
    {
        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}
