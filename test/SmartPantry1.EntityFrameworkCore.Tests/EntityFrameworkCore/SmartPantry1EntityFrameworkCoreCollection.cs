using Xunit;

namespace SmartPantry1.EntityFrameworkCore;

[CollectionDefinition(SmartPantry1TestConsts.CollectionDefinitionName)]
public class SmartPantry1EntityFrameworkCoreCollection : ICollectionFixture<SmartPantry1EntityFrameworkCoreFixture>
{

}
