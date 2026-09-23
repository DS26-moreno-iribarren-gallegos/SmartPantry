using Volo.Abp.Settings;

namespace SmartPantry1.Settings;

public class SmartPantry1SettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SmartPantry1Settings.MySetting1));
    }
}
