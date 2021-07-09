using FluentAssertions;
using GuimoSoft.Notifications.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Notifications.Tests
{
    public class NotifiableObjectTests
    {
        [Fact]
        public void NotifiableObjectFacts()
        {
            var entity = new FakeEntity();

            entity.Validate(entity, FakeEntityValidator.Instance);

            entity.IsValid
                .Should().BeFalse();

            entity.Notifications
                .Should().NotBeEmpty(); 
            
            entity = new FakeEntity("test");

            entity.Validate(entity, FakeEntityValidator.Instance);

            entity.IsValid
                .Should().BeTrue();

            entity.Notifications
                .Should().BeNullOrEmpty();
        }
    }
}
