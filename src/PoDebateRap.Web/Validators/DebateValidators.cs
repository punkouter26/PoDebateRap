using FluentValidation;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Validators
{
    /// <summary>
    /// Validator for StartDebateRequest
    /// </summary>
    public class StartDebateRequestValidator : AbstractValidator<StartDebateRequest>
    {
        public StartDebateRequestValidator()
        {
            RuleFor(x => x.Rapper1)
                .NotNull().WithMessage("Rapper1 is required")
                .SetValidator(new RapperValidator());

            RuleFor(x => x.Rapper2)
                .NotNull().WithMessage("Rapper2 is required")
                .SetValidator(new RapperValidator());

            RuleFor(x => x)
                .Must(x => x.Rapper1?.Name != x.Rapper2?.Name)
                .WithMessage("Rappers must be different");

            RuleFor(x => x.Topic)
                .NotNull().WithMessage("Topic is required")
                .SetValidator(new TopicValidator());
        }
    }

    /// <summary>
    /// Validator for Rapper model
    /// </summary>
    public class RapperValidator : AbstractValidator<Rapper>
    {
        public RapperValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Rapper name is required")
                .MinimumLength(2).WithMessage("Rapper name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Rapper name cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9\s\-\.]+$").WithMessage("Rapper name contains invalid characters");

            RuleFor(x => x.Wins)
                .GreaterThanOrEqualTo(0).WithMessage("Wins must be non-negative");

            RuleFor(x => x.Losses)
                .GreaterThanOrEqualTo(0).WithMessage("Losses must be non-negative");

            RuleFor(x => x.TotalDebates)
                .GreaterThanOrEqualTo(0).WithMessage("Total debates must be non-negative");

            // PartitionKey and RowKey are infrastructure concerns and may be empty when sent from client
            // They will be set properly when needed for storage operations
            // Removing strict validation to allow client-side rapper objects to pass validation
        }
    }

    /// <summary>
    /// Validator for Topic model
    /// </summary>
    public class TopicValidator : AbstractValidator<Topic>
    {
        public TopicValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Topic title is required")
                .MinimumLength(10).WithMessage("Topic title must be at least 10 characters")
                .MaximumLength(200).WithMessage("Topic title cannot exceed 200 characters");

            RuleFor(x => x.Category)
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Category));
        }
    }
}
