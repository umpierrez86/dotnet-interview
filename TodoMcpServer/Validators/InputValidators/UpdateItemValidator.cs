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
        
        RuleFor(item => item.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(item => !string.IsNullOrEmpty(item.Description));

        RuleFor(item => item.NewName)
            .MaximumLength(100).WithMessage("New name cannot exceed 100 characters.")
            .When(item => !string.IsNullOrEmpty(item.NewName));
        
        RuleFor(item => item)
            .Custom((item, context) =>
            {
                var hasNewName = !string.IsNullOrWhiteSpace(item.NewName);
                var hasDescription = !string.IsNullOrWhiteSpace(item.Description);

                if (!hasNewName && !hasDescription)
                {
                    context.AddFailure("You must specify at least one field to update (NewName or Description).");
                }
            });

        RuleFor(item => item.ListName)
            .NotEmpty().WithMessage("The name of the list where the item belongs is needed.");
    }
}