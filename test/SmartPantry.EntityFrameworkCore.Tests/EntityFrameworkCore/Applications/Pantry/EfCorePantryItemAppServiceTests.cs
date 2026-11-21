using SmartPantry;
using SmartPantry.Pantry;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications.Pantry;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCorePantryItemAppServiceTests : PantryItemAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}