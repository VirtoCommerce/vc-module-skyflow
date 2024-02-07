using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Providers;
using VirtoCommerce.Skyflow.Data.Services;

namespace VirtoCommerce.Skyflow.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.Configure<SkyflowOptions>(Configuration.GetSection("Skyflow"));
        serviceCollection.AddTransient<ISkyflowClient, SkyflowClient>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        var paymentMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPaymentMethodsRegistrar>();
        paymentMethodsRegistrar.RegisterPaymentMethod(() => new SkyflowPaymentMethod(
            appBuilder.ApplicationServices.GetService<ISkyflowClient>()
        ));
        //Associate the settings with the particular payment method
        settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.AllSettings, nameof(SkyflowPaymentMethod));
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
