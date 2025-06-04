using FluentValidation;
using TodoMcpServer.Inputs;

namespace TodoMcpServer.InputValidator;

public class ToDoLIstValidator : AbstractValidator<CreateToDoList>, IValidator<CreateToDoList>
{
    public ToDoLIstValidator()
    {
        RuleFor(list => list.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}