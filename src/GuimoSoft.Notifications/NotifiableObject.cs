using FluentValidation;
using GuimoSoft.Core;
using System.Collections.Generic;
using System.Linq;

namespace GuimoSoft.Notifications
{
    public abstract class NotifiableObject
    {
        public bool IsValid { get; private set; }
        public IEnumerable<Notification> Notifications { get; private set; }

        public void Validate<TModel>(TModel model, AbstractValidator<TModel> validator)
        {
            var validationResult = validator.Validate(model);

            IsValid = validationResult.IsValid;
            if (!IsValid)
                Notifications = validationResult.Errors.Select(error => new Notification(error.PropertyName, error.ErrorMessage, error.AttemptedValue));
        }
    }
}
