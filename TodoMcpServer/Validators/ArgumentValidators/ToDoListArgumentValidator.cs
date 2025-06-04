using System.Text.Json;
using FluentValidation;

namespace TodoMcpServer.InputValidator;

public class ToDoListArgumentValidator : AbstractValidator<IReadOnlyDictionary<string, JsonElement>>, IValidator<IReadOnlyDictionary<string, JsonElement>>
{
    public ToDoListArgumentValidator()
    {
        RuleFor(args => args)
            .Must(args => args.ContainsKey("name"))
            .WithMessage(args => $"Missing required field: 'name'. Got: {string.Join(", ", args.Keys)}");
    }
}