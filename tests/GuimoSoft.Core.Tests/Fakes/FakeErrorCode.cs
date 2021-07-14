using System.ComponentModel;

namespace GuimoSoft.Core.Tests.Fakes
{
    public enum FakeErrorCode
    {
        [Description("Fake error message 1")]
        FakeErrorCode1 = 1,
        FakeErrorCode2 = 2,
        [Description("Fake error message 3")]
        FakeErrorCode3 = 3,
        [Description("Invalid fake entity")]
        InvalidFakeEntity = 100
    }
}
