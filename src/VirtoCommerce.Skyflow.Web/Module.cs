using GraphQL;
using GraphQL.MicrosoftDI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Providers;
using VirtoCommerce.Skyflow.Data.Services;
using VirtoCommerce.Skyflow.XApi;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Skyflow.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        _ = new GraphQLBuilder(serviceCollection, builder =>
        {
            builder.AddSchema(serviceCollection, typeof(AssemblyMarker));
        });

        serviceCollection.Configure<SkyflowOptions>(Configuration.GetSection("Payments:Skyflow"));
        serviceCollection.AddTransient<ISkyflowClient, SkyflowClient>();
        serviceCollection.AddTransient<SkyflowPaymentMethod>();
        serviceCollection.AddTransient<SkyflowAuthorizationHandler>();
        serviceCollection.AddHttpClient(ModuleConstants.SkyflowHttpClientName)
          .AddHttpMessageHandler<SkyflowAuthorizationHandler>();

    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var paymentMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPaymentMethodsRegistrar>();
        paymentMethodsRegistrar.RegisterPaymentMethod(() => appBuilder.ApplicationServices.GetService<SkyflowPaymentMethod>());
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
