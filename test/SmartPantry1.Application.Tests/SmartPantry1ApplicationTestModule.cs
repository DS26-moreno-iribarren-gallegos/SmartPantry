using Volo.Abp.Modularity;

namespace SmartPantry1;

[DependsOn(
    typeof(SmartPantry1ApplicationModule),
    typeof(SmartPantry1DomainTestModule)
)]
public class SmartPantry1ApplicationTestModule : AbpModule
{

}
