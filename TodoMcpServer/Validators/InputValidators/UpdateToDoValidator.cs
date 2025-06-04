using FluentValidation;
using TodoMcpServer.Inputs;

namespace TodoMcpServer.InputValidator;

public class UpdateToDoValidator : AbstractValidator<UpdateToDoList>
{
    public UpdateToDoValidator()
    {
        RuleFor(list => list.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(list => list.NewName)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(list => list.NewName)
            .NotEqual(list => list.Name).WithMessage("Name cannot be the same.");
    }
}