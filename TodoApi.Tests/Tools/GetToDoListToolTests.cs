using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Tools;
using Xunit;

namespace TodoApi.Tests.Tools;

public class GetToDoListToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenListIsFound()
    {
        var responseContent = "[{ \"id\": 1, \"name\": \"Groceries\" }]";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListTool(httpClient);
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
    public async Task HandleAsync_ReturnsError_WhenListIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListTool(httpClient);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "name", JsonDocument.Parse("\"UnknownList\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiCallFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Server error")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListTool(httpClient);
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
    public async Task HandleAsync_ReturnsError_WhenArgumentMissing()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListTool(httpClient);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
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
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object);
    }
}
