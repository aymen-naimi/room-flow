using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var endpoint = configuration["Email:Endpoint"]
                ?? throw new InvalidOperationException("Email:Endpoint is missing.");
            return new EmailClient(new Uri(endpoint), new DefaultAzureCredential());
        });
    })
    .Build();

host.Run();
