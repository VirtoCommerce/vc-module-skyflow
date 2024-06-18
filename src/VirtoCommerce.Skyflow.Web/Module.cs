using GraphQL.Server;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Providers;
using VirtoCommerce.Skyflow.Data.Services;
using VirtoCommerce.Skyflow.XApi;

namespace VirtoCommerce.Skyflow.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        var assemblyMarker = typeof(AssemblyMarker);
        var graphQlBuilder = new CustomGraphQLBuilder(serviceCollection);
        graphQlBuilder.AddGraphTypes(assemblyMarker);
        serviceCollection.AddMediatR(assemblyMarker);
        serviceCollection.AddAutoMapper(assemblyMarker);
        serviceCollection.AddSchemaBuilders(assemblyMarker);

        serviceCollection.Configure<SkyflowOptions>(Configuration.GetSection("Payments:Skyflow"));
        serviceCollection.AddTransient<ISkyflowClient, SkyflowClient>();
        serviceCollection.AddTransient<SkyflowPaymentMethod>();
        serviceCollection.AddTransient<SkyflowAuthorizationHandler>();
        serviceCollection.AddHttpClient("Skyflow")
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
