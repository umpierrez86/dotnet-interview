using FluentValidation;
using TodoMcpServer.Inputs;

namespace TodoMcpServer.InputValidator;

public class DeleteItemValidator : AbstractValidator<DeleteItem>
{
    public DeleteItemValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(item => item.ListName)
            .NotEmpty().WithMessage("List name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}