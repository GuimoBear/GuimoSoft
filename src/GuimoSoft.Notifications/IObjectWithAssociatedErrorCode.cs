using System;

namespace GuimoSoft.Notifications
{
    public interface IObjectWithAssociatedErrorCode<out TEnumType>
        where TEnumType : Enum
    {
        TEnumType GetInvalidErrorCode();
    }
}
