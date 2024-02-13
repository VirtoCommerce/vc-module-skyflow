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

                public static readonly SettingDescriptor ConnectionUrl = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.ConnectionUrl",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.ShortText
                };

                public static readonly SettingDescriptor ConnectionContentType = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.ConnectionContentType",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.ShortText
                };

                public static readonly SettingDescriptor ConnectionBody = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Skyflow.ConnectionBody",
                    GroupName = "Payment|Skyflow",
                    ValueType = SettingValueType.LongText
                };

                public static IEnumerable<SettingDescriptor> AllGeneralSettings
                {
                    get
                    {
                        yield return VaultId;
                        yield return VaultUrl;
                        yield return TableName;
                        yield return ConnectionUrl;
                        yield return ConnectionContentType;
                        yield return ConnectionBody;
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
