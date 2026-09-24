using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartPantry1.Data;

/* This is used if database provider does't define
 * ISmartPantry1DbSchemaMigrator implementation.
 */
public class NullSmartPantry1DbSchemaMigrator : ISmartPantry1DbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
