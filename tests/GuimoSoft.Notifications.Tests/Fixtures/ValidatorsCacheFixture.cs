using GuimoSoft.Notifications.AspNetCore;
using GuimoSoft.Notifications.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Notifications.Tests.Fixtures
{
    public class ValidatorsCacheFixture
    {
        public ValidatorsCacheFixture()
        {
            AbstractValidatorCache<FakeErrorCode>
                .FindValidators(typeof(ValidatorsCacheFixture).Assembly);
        }
    }

    [CollectionDefinition(FIXTURE_COLLECTION_NAME)]
    public class ValidatorsCacheFixtureCollection : ICollectionFixture<ValidatorsCacheFixture>
    {
        public const string FIXTURE_COLLECTION_NAME = "Validators cache";
    }
}
