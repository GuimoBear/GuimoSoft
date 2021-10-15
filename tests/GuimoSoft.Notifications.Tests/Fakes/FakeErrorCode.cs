using System.ComponentModel;

namespace GuimoSoft.Notifications.Tests.Fakes
{
    public enum FakeErrorCode
    {
        [Description("Fake error @event 1")]
        FakeErrorCode1 = 1,
        FakeErrorCode2 = 2,
        [Description("Fake error @event 3")]
        FakeErrorCode3 = 3,
        [Description("Invalid fake entity")]
        InvalidFakeEntity = 100
    }
}
