using FluentValidation;

namespace CreateFamily.Validate
{
    internal class DoubleValidator : AbstractValidator<double>
    {
        public DoubleValidator()
        {
            RuleFor(value => value)
                .NotEmpty().WithMessage("Giá trị không được để trống")
                .Must(BeValidDouble).WithMessage("Giá trị phải là một số Double");
        }

        private bool BeValidDouble(double value)
        {
            return value == 0;
        }
    }
}