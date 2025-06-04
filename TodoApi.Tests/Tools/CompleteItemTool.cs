using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.Tools;
using TodoMcpServer.Interfaces;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;

public class CompleteItemToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenItemIsMarkedComplete()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock.Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);
        lookupMock.Setup(x => x.GetItemIdByNameAsync(1, "Yerba", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(42);

        var tool = new CompleteItemTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Yerba\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenArgumentsMissing()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK), out _);
        var lookupMock = new Mock<ITodoLookupService>();

        var tool = new CompleteItemTool(httpClient, lookupMock.Object);

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
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK), out _);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock.Setup(x => x.GetListIdByNameAsync("Unknown", It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("Not found"));

        var tool = new CompleteItemTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Item\"").RootElement },
                { "listName", JsonDocument.Parse("\"Unknown\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiCallFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock.Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(10);
        lookupMock.Setup(x => x.GetItemIdByNameAsync(10, "Milk", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(5);

        var tool = new CompleteItemTool(httpClient, lookupMock.Object);

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
    public async Task HandleAsync_SendsCorrectRequest()
    {
        HttpRequestMessage? capturedRequest = null;
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out _, req => capturedRequest = req);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock.Setup(x => x.GetListIdByNameAsync("Groceries", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(3);
        lookupMock.Setup(x => x.GetItemIdByNameAsync(3, "Bread", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(77);

        var tool = new CompleteItemTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Bread\"").RootElement },
                { "listName", JsonDocument.Parse("\"Groceries\"").RootElement }
            }
        };

        await tool.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Equal("http://localhost:5083/api/todoLists/3/items/77/complete", capturedRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Patch, capturedRequest.Method);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response, out Mock<HttpMessageHandler> handlerMock, Action<HttpRequestMessage>? capture = null)
    {
        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capture?.Invoke(req))
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}
