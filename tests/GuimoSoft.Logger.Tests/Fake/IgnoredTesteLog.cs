using GuimoSoft.Logger.Attributes;

namespace GuimoSoft.Logger.Tests.Fake
{
    [LoggerIgnore]
    public class IgnoredTesteLog
    {
        public int PropriedadeLogavel { get; set; } = 0;
    }
}
