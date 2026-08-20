using FluentValidation;
using Pollynx.Application.DTOs.Polls;

namespace Pollynx.Application.Validators;

public class CreatePollValidator : AbstractValidator<CreatePollDto>
{
    public CreatePollValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.StartTime)
            .NotEmpty();

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .GreaterThan(x => x.StartTime);

        RuleFor(x => x.Options)
            .NotNull()
            .Must(x => x.Count >= 2)
            .WithMessage(
                "A poll must have at least two options.");

        RuleForEach(x => x.Options)
            .NotEmpty()
            .MaximumLength(200);
    }
}