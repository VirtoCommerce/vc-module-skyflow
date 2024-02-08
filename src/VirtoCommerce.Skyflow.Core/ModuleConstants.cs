using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Skyflow.Core
{
    public static class ModuleConstants
    {
        public static class Settings
        {
            public static class General
            {
                public static readonly SettingDescriptor VaultId = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.VaultId",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor VaultUrl = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.VaultUrl",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor TableName = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.TableName",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "cards"
                };


                public static IEnumerable<SettingDescriptor> AllGeneralSettings
                {
                    get
                    {
                        yield return VaultId;
                        yield return VaultUrl;
                        yield return TableName;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllGeneralSettings;
                }
            }
        }
    }
}
