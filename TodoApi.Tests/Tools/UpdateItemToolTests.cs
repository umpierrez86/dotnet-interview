using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Tools;
using Xunit;

namespace TodoApi.Tests.Tools;

public class UpdateItemToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenItemIsUpdated()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        lookupServiceMock
            .Setup(x => x.GetItemIdByNameAsync(1, "Milk", It.IsAny<CancellationToken>()))
            .ReturnsAsync(101);

        var tool = new UpdateItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement },
                { "newName", JsonDocument.Parse("\"Almond Milk\"").RootElement },
                { "description", JsonDocument.Parse("\"Lactose free\"").RootElement },
                { "isComplete", JsonDocument.Parse("true").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenMissingArguments()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.NoContent), out _);
        var lookupServiceMock = new Mock<ITodoLookupService>();

        var tool = new UpdateItemTool(httpClient, lookupServiceMock.Object);
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

        var tool = new UpdateItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement },
                { "newName", JsonDocument.Parse("\"Almond Milk\"").RootElement },
                { "description", JsonDocument.Parse("\"Lactose free\"").RootElement },
                { "isComplete", JsonDocument.Parse("false").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Item not found")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        lookupServiceMock
            .Setup(x => x.GetItemIdByNameAsync(1, "Milk", It.IsAny<CancellationToken>()))
            .ReturnsAsync(101);

        var tool = new UpdateItemTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Milk\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement },
                { "newName", JsonDocument.Parse("\"Almond Milk\"").RootElement },
                { "description", JsonDocument.Parse("\"Lactose free\"").RootElement },
                { "isComplete", JsonDocument.Parse("false").RootElement }
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
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}