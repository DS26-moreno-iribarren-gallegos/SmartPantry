using SmartPantry1.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SmartPantry1.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SmartPantry1EntityFrameworkCoreModule),
    typeof(SmartPantry1ApplicationContractsModule)
)]
public class SmartPantry1DbMigratorModule : AbpModule
{
}
