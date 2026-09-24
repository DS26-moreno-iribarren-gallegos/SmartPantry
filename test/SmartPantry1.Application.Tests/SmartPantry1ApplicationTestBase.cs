using Volo.Abp.Modularity;

namespace SmartPantry1;

public abstract class SmartPantry1ApplicationTestBase<TStartupModule> : SmartPantry1TestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
