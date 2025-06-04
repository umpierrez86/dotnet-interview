using FluentValidation;
using TodoMcpServer.Inputs;

namespace TodoMcpServer.InputValidator;

public class UpdateItemValidator : AbstractValidator<UpdateItem>
{
    public UpdateItemValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(item => item.ListName)
            .NotEmpty().WithMessage("The name of the list where the item belongs is needed.");
    }
}