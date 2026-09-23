using Microsoft.Extensions.Localization;
using SmartPantry1.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SmartPantry1;

[Dependency(ReplaceServices = true)]
public class SmartPantry1BrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<SmartPantry1Resource> _localizer;

    public SmartPantry1BrandingProvider(IStringLocalizer<SmartPantry1Resource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
