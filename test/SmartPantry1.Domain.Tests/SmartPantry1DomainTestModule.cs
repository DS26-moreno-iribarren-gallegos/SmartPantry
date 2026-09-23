using Volo.Abp.Modularity;

namespace SmartPantry1;

[DependsOn(
    typeof(SmartPantry1DomainModule),
    typeof(SmartPantry1TestBaseModule)
)]
public class SmartPantry1DomainTestModule : AbpModule
{

}
