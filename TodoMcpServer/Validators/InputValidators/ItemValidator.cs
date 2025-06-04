using FluentValidation;
using TodoMcpServer.Inputs;

namespace TodoMcpServer.InputValidator;

public class ItemValidator : AbstractValidator<CreateItem>
{
    public ItemValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(item => item.Description)
            .NotEmpty().WithMessage("Description cannot be empty.")
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.");

        RuleFor(item => item.ListName)
            .NotEmpty().WithMessage("The name of the list where the item belongs is needed.");
    }
}