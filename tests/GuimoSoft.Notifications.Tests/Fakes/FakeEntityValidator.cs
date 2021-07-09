using FluentValidation;

namespace GuimoSoft.Notifications.Tests.Fakes
{
    public class FakeEntityValidator : AbstractValidator<FakeEntity>
    {
        public static FakeEntityValidator Instance = new FakeEntityValidator();

        private FakeEntityValidator()
        {
            RuleFor(e => e.PropertyNotNullOrEmpty)
                .NotEmpty().WithMessage("error");
        }
    }
}
