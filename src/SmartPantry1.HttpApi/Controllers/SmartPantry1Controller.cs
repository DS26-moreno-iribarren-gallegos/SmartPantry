using SmartPantry1.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartPantry1.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SmartPantry1Controller : AbpControllerBase
{
    protected SmartPantry1Controller()
    {
        LocalizationResource = typeof(SmartPantry1Resource);
    }
}
