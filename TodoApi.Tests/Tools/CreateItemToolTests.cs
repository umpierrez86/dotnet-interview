using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Tools;

public class CreateItemToolTests
{
    private static HttpClient CreateMockHttpClient(HttpResponseMessage responseMessage, out Mock<HttpMessageHandler> handlerMock)
    {
        handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        return new HttpClient(handlerMock.Object);
    }

    private CallToolRequest CreateValidRequest()
    {
        var args = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonDocument.Parse("\"test item\"").RootElement,
            ["description"] = JsonDocument.Parse("\"test description\"").RootElement,
            ["listName"] = JsonDocument.Parse("\"test list\"").RootElement
        };

        return new CallToolRequest { Arguments = args };
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenItemIsCreated()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("{\"id\":1}")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(s => s.GetListIdByNameAsync("test list", It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        var tool = new CreateItemTool(httpClient, lookupServiceMock.Object);
        var request = CreateValidRequest();

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Contains("The item was created successfully", result.Content[0].Text);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenHttpFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request")
        };

        var httpClient = CreateMockHttpClient(response, out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(s => s.GetListIdByNameAsync("test list", It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        var tool = new CreateItemTool(httpClient, lookupServiceMock.Object);
        var request = CreateValidRequest();

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("There was an error creating the item", result.Content[0].Text);
    }

    [Fact]
    public async Task HandleAsync_ReturnsError_WhenLookupServiceThrows()
    {
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK), out _);

        var lookupServiceMock = new Mock<ITodoLookupService>();
        lookupServiceMock
            .Setup(s => s.GetListIdByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("list not found"));

        var tool = new CreateItemTool(httpClient, lookupServiceMock.Object);
        var request = CreateValidRequest();

        var result = await tool.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("Argument error", result.Content[0].Text);
    }
}
