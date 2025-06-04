using System.Text.Json;
using FluentValidation;

namespace TodoMcpServer.InputValidator;

public class ItemArgumentValidator : AbstractValidator<IReadOnlyDictionary<string, JsonElement>>
{
    public ItemArgumentValidator()
    {
        RuleFor(args => args)
            .Must(args => args.ContainsKey("name"))
            .WithMessage(args => $"Missing required field: 'name'. Got: {string.Join(", ", args.Keys)}");
        
        RuleFor(args => args)
            .Must(args => args.ContainsKey("description"))
            .WithMessage(args => $"Missing required field: 'description'. Got: {string.Join(", ", args.Keys)}");
        
        RuleFor(args => args)
            .Must(args => args.ContainsKey("listName"))
            .WithMessage(args => $"Missing required field: 'listName'. Got: {string.Join(", ", args.Keys)}");
    }
}