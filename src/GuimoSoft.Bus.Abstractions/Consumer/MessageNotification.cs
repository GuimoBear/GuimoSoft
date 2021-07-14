using MediatR;
using System;

namespace GuimoSoft.Bus.Abstractions.Consumer
{
    public sealed class MessageNotification<TMessage> : INotification
        where TMessage : IMessage
    {
        public MessageNotification(TMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message), "É necessario que a mensagem não seja nula");
        }

        public TMessage Message { get; }

        public override bool Equals(object obj)
        {
            return obj is not null &&
                   (obj is MessageNotification<TMessage> messageEnvelop && Message.Equals(messageEnvelop.Message) ||
                    obj is TMessage message && Message.Equals(message));
        }

        public override int GetHashCode()
            => Message.GetHashCode();

        public static explicit operator TMessage(MessageNotification<TMessage> message)
        {
            if (message is not null)
                return message.Message;
            return default;
        }

        public static explicit operator MessageNotification<TMessage>(TMessage message)
            => new MessageNotification<TMessage>(message);
    }
}
