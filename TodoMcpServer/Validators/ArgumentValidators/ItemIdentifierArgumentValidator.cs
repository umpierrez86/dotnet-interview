using System.Text.Json;
using FluentValidation;

namespace TodoMcpServer.InputValidator;

public class ItemIdentifierArgumentValidator : AbstractValidator<IReadOnlyDictionary<string, JsonElement>>, IValidator<IReadOnlyDictionary<string, JsonElement>>
{
    public ItemIdentifierArgumentValidator()
    {
        RuleFor(args => args)
            .Must(args => args.ContainsKey("name"))
            .WithMessage(args => $"Missing required field: 'name'. Got: {string.Join(", ", args.Keys)}");
        
        RuleFor(args => args)
            .Must(args => args.ContainsKey("listName"))
            .WithMessage(args => $"Missing required field: 'listName'. Got: {string.Join(", ", args.Keys)}");
    }
}