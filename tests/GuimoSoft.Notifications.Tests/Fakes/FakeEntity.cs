using GuimoSoft.Notifications;

namespace GuimoSoft.Notifications.Tests.Fakes
{
    public class FakeEntity : NotifiableObject, IObjectWithAssociatedErrorCode<FakeErrorCode>
    {
        public string PropertyNotNullOrEmpty { get; }

        public FakeEntity()
        {

        }

        public FakeEntity(string propertyNotNullOrEmpty)
        {
            PropertyNotNullOrEmpty = propertyNotNullOrEmpty;
        }

        public FakeErrorCode GetInvalidErrorCode()
            => FakeErrorCode.InvalidFakeEntity;
    }
}
