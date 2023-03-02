using AssessmentAPI.Models;
using FluentValidation;
using System;

namespace AssessmentAPI.Validators
{
    public class BatchDetailsResponseValidator : AbstractValidator<BatchDetailsResponse>
    {
        public BatchDetailsResponseValidator() 
        {
            RuleFor(batch => batch.BatchId).NotNull().NotEmpty().WithMessage("Batch ID should not be null or empty");
            RuleFor(batch => batch.BusinessUnit).NotNull().NotEmpty().WithMessage("Batch ID should not be null or empty");
            RuleFor(batch => batch.ExpiryDate).GreaterThan(DateTime.Now).WithMessage("Expiry date should be greater than current date");
        }
    }
}
