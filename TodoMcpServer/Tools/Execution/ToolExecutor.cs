using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;

namespace TodoMcpServer.Services;

public abstract class ToolExecutor
{
    public abstract string Name { get; }
    public async ValueTask<CallToolResponse> HandleAsync(CallToolRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(request, cancellationToken);
    }
    
    protected virtual string GetSuccessMessage() => "Operation completed successfully";
    protected virtual string GetFailureMessagePrefix() => "Operation failed";
    
    private async ValueTask<CallToolResponse> ExecuteAsync(CallToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Logic(request, cancellationToken);
            return await InterpretAsync(response, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Error("Argument error: " + ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return Error("HTTP error: " + ex.Message);
        }
        catch (Exception ex)
        {
            return Error("Unexpected error: " + ex.Message);
        }
    }

    public abstract Tool GetToolDefinition();
    protected abstract ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken);

    private async ValueTask<CallToolResponse> InterpretAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return await ValueTask.FromResult( new CallToolResponse
            {
                IsError = true,
                Content =
                [
                    new Content
                    {
                        Type = "text",
                        Text = $"{GetFailureMessagePrefix()}: {response.StatusCode} - {error}"
                    }
                ]
            });
        }
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CallToolResponse
        {
            Content =
            [
                new Content
                {
                    Type = "text",
                    Text = $"{GetSuccessMessage()}: {content}"
                }
            ]
        };
    }

    private CallToolResponse Error(string message) => new CallToolResponse
    {
        IsError = true,
        Content = [ new Content { Type = "text", Text = message } ]
    };
}