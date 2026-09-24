using SmartPantry1.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SmartPantry1.Permissions;

public class SmartPantry1PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SmartPantry1Permissions.GroupName);

        var booksPermission = myGroup.AddPermission(SmartPantry1Permissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(SmartPantry1Permissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(SmartPantry1Permissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(SmartPantry1Permissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(SmartPantry1Permissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(SmartPantry1Permissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(SmartPantry1Permissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(SmartPantry1Permissions.Authors.Delete, L("Permission:Authors.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SmartPantry1Permissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartPantry1Resource>(name);
    }
}
