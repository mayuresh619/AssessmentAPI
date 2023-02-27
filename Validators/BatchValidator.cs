using AssessmentAPI.Models;
using FluentValidation;

namespace AssessmentAPI.Validators
{
    public class BatchValidator : AbstractValidator<BatchRequest>
    {
        public BatchValidator() 
        {
            RuleFor(batch => batch.BusinessUnit).NotNull().NotEmpty();
            RuleForEach(x => x.Attritubes).ChildRules(orders =>
            {
                orders.RuleForEach(x => x.Key).NotNull().NotEmpty();
            });
        }
    }
}
