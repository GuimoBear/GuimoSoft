using System.ComponentModel;

namespace GuimoSoft.Examples.Notifications.Domain
{
    public enum ErrorCode
    {
        [Description("Existem erros de validação")]
        InvalidWeatherForecastRequest = 100
    }
}
