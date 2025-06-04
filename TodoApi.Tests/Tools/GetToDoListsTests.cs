namespace TodoApi.Tests.Tools;

using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Tools;
using Xunit;

public class GetToDoListsToolTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenListsAreRetrieved()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListsTool(httpClient);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenApiCallFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Server error")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListsTool(httpClient);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>()
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task HandleAsync_IgnoresArguments_WhenUnexpectedArgsProvided()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        };
        var httpClient = CreateMockHttpClient(response, out _);

        var tool = new GetToDoListsTool(httpClient);
        var request = new CallToolRequest
        {
            Arguments = new Dictionary<string, JsonElement>
            {
                { "unexpected", JsonDocument.Parse("\"value\"").RootElement }
            }
        };

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
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
