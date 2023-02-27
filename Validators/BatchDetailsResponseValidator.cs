using AssessmentAPI.Models;
using FluentValidation;
using System;

namespace AssessmentAPI.Validators
{
    public class BatchDetailsResponseValidator : AbstractValidator<BatchDetailsResponse>
    {
        public BatchDetailsResponseValidator() 
        {
            RuleFor(batch => batch.BatchId).NotNull().NotEmpty();
            RuleFor(batch => batch.BusinessUnit).NotNull().NotEmpty();
            RuleFor(batch => batch.ExpiryDate).GreaterThan(DateTime.Now);
        }
    }
}
