using System.Threading.Tasks;

namespace SmartPantry1.Data;

public interface ISmartPantry1DbSchemaMigrator
{
    Task MigrateAsync();
}
