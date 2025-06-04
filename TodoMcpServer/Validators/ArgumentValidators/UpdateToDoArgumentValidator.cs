using System.Text.Json;
using FluentValidation;

namespace TodoMcpServer.InputValidator;

public class UpdateToDoArgumentValidator : AbstractValidator<IReadOnlyDictionary<string, JsonElement>>
{
    public UpdateToDoArgumentValidator()
    {
        RuleFor(args => args)
            .Must(args => args.ContainsKey("name"))
            .WithMessage(args => $"Missing required field: 'name'. Got: {string.Join(", ", args.Keys)}");
        
        RuleFor(args => args)
            .Must(args => args.ContainsKey("newName"))
            .WithMessage(args => $"Missing required field: 'new name'. Got: {string.Join(", ", args.Keys)}");
    }
}