using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Skyflow.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "Skyflow:access";
            public const string Create = "Skyflow:create";
            public const string Read = "Skyflow:read";
            public const string Update = "Skyflow:update";
            public const string Delete = "Skyflow:delete";

            public static string[] AllPermissions { get; } =
            {
                Access,
                Create,
                Read,
                Update,
                Delete,
            };
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor SkyflowEnabled { get; } = new()
            {
                Name = "Skyflow.SkyflowEnabled",
                GroupName = "Skyflow|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static SettingDescriptor SkyflowPassword { get; } = new()
            {
                Name = "Skyflow.SkyflowPassword",
                GroupName = "Skyflow|Advanced",
                ValueType = SettingValueType.SecureString,
                DefaultValue = "qwerty",
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return SkyflowEnabled;
                    yield return SkyflowPassword;
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
