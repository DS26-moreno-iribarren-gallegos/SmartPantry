using Volo.Abp.Modularity;

namespace SmartPantry1;

/* Inherit from this class for your domain layer tests. */
public abstract class SmartPantry1DomainTestBase<TStartupModule> : SmartPantry1TestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
