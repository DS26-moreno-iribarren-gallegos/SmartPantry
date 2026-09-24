using SmartPantry1.Samples;
using Xunit;

namespace SmartPantry1.EntityFrameworkCore.Domains;

[Collection(SmartPantry1TestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SmartPantry1EntityFrameworkCoreTestModule>
{

}
