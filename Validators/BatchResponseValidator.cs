using AssessmentAPI.Models;
using FluentValidation;

namespace AssessmentAPI.Validators
{
    public class BatchResponseValidator : AbstractValidator<BatchResponse>
    {
        public BatchResponseValidator()
        {
            RuleFor(resp => resp.BatchId).NotNull().NotEmpty();
        }
    }
}
