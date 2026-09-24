using SmartPantry1.Samples;
using Xunit;

namespace SmartPantry1.EntityFrameworkCore.Applications;

[Collection(SmartPantry1TestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<SmartPantry1EntityFrameworkCoreTestModule>
{

}
