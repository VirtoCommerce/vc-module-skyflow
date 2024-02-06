using System;
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
                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return Array.Empty<SettingDescriptor>();
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings;
                }
            }
        }
    }
}
