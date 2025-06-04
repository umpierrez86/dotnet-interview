namespace TodoApi.Tests.Tools;

using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Tools;
using Xunit;

public class DeleteToDoListToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenListIsDeleted()
    {
        var listId = 1;
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(listId);

        var tool = new DeleteToDoListTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Groceries\"").RootElement }
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

        var tool = new DeleteToDoListTool(httpClient, lookupService);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenListNotFound()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.NoContent), out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("List not found"));

        var tool = new DeleteToDoListTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiCallFails()
    {
        var listId = 1;
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("List not found")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(listId);

        var tool = new DeleteToDoListTool(httpClient, lookupServiceMock.Object);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Groceries\"").RootElement }
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
