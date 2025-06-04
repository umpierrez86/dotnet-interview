using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Tools;

namespace TodoApi.Tests.Tools;

public class CreateToDoListToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenListIsCreated()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("{\"id\":1}")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new CreateToDoListTool(httpClient);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"My List\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Contains("Creation of ToDo list was successful", result.Content[0].Text);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenArgumentIsMissing()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.Created), out _);

        var tool = new CreateToDoListTool(httpClient);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenNameIsEmpty()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.Created), out _);

        var tool = new CreateToDoListTool(httpClient);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiReturnsBadRequest()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new CreateToDoListTool(httpClient);

        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"Fail List\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response, out Mock<HttpMessageHandler> handlerMock)
    {
        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}
