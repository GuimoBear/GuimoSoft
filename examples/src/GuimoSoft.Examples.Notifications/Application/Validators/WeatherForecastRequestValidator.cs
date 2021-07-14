using FluentValidation;
using GuimoSoft.Examples.Cache.ValueObjects;

namespace GuimoSoft.Examples.Notifications.Application.Validators
{
    public class WeatherForecastRequestValidator : AbstractValidator<WeatherForecastRequest>
    {
        public WeatherForecastRequestValidator()
        {
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City must not be empty");

            RuleFor(x => x.City)
                .MinimumLength(4).WithMessage("City must contains at least 4 characters");
        }
    }
}
