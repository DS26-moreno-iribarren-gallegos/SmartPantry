using SmartPantry1.Books;
using Xunit;

namespace SmartPantry1.EntityFrameworkCore.Applications.Books;

[Collection(SmartPantry1TestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<SmartPantry1EntityFrameworkCoreTestModule>
{

}