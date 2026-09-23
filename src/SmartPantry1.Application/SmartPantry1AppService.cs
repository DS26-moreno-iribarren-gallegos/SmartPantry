using SmartPantry1.Localization;
using Volo.Abp.Application.Services;

namespace SmartPantry1;

/* Inherit your application services from this class.
 */
public abstract class SmartPantry1AppService : ApplicationService
{
    protected SmartPantry1AppService()
    {
        LocalizationResource = typeof(SmartPantry1Resource);
    }
}
