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

public class UpdateToDoListToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenListIsUpdated()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out _);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock
            .Setup(x => x.GetListIdByNameAsync("Errands", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var tool = new UpdateToDoListTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Errands\"").RootElement },
                { "newName", JsonDocument.Parse("\"Weekend Errands\"").RootElement }
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
        var tool = new UpdateToDoListTool(httpClient, lookupMock.Object);

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
        lookupMock
            .Setup(x => x.GetListIdByNameAsync("Nonexistent", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var tool = new UpdateToDoListTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Nonexistent\"").RootElement },
                { "newName", JsonDocument.Parse("\"Something\"").RootElement }
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
        lookupMock
            .Setup(x => x.GetListIdByNameAsync("Errands", It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var tool = new UpdateToDoListTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Errands\"").RootElement },
                { "newName", JsonDocument.Parse("\"Updated Errands\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_SendsCorrectPayload()
    {
        HttpRequestMessage? capturedRequest = null;

        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var httpClient = CreateMockHttpClient(response, out var handlerMock, req => capturedRequest = req);

        var lookupMock = new Mock<ITodoLookupService>();
        lookupMock
            .Setup(x => x.GetListIdByNameAsync("OldName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var tool = new UpdateToDoListTool(httpClient, lookupMock.Object);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"OldName\"").RootElement },
                { "newName", JsonDocument.Parse("\"NewName\"").RootElement }
            }
        };

        await tool.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(capturedRequest);
        var json = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("\"Name\":\"NewName\"", json);
        Assert.Equal("http://localhost:5083/api/todoLists/42", capturedRequest.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Put, capturedRequest.Method);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response, out Mock<HttpMessageHandler> handlerMock, Action<HttpRequestMessage>? capture = null)
    {
        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capture?.Invoke(req))
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}
