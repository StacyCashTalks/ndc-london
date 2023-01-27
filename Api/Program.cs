using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CosmosDBAccessor;
using Api.Models;

var environmentVariables = Environment.GetEnvironmentVariables();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
        {
            s.AddSingleton<IContainerAccess<CosmosTodo>>(factory => new CosmosContainerAccess<CosmosTodo>(
                new CosmosEndpointKeySettings(
                    (string)environmentVariables["CosmosEndpoint"]!,
                    (string)environmentVariables["CosmosKey"]!,
                    (string)environmentVariables["TodoDatabaseName"]!,
                    (string)environmentVariables["TodoContainerName"]!
                    )));
            s.AddSingleton<TodoHandler>();
        }
    )
    .Build();

host.Run();
