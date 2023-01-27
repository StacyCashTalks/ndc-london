using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Reflection;

namespace Api.Cosmos;

internal class CosmosAccessor
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosSettings cosmosSettings;
    private Container? _container;

    public CosmosAccessor(CosmosSettings cosmosSettings)
    {
        _cosmosClient = new CosmosClientBuilder(cosmosSettings.CosmosConnectionString)
        .WithApplicationName(Assembly.GetExecutingAssembly().GetName().Name)
        .Build();
        this.cosmosSettings = cosmosSettings;
    }

    public async Task<Container> GetContainer()
    { 
        _container ??= await CreateContainer();
        return _container;
    }

    private async Task<Container> CreateContainer()
    {
        Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosSettings.DatabaseName);

        var partitionKey = cosmosSettings.PartitionKey;
        ContainerProperties containerProperties = new()
        {
            Id = cosmosSettings.ContainerName,
            PartitionKeyPath = partitionKey
        };

        await database.CreateContainerIfNotExistsAsync(containerProperties);
        var container = database.GetContainer(cosmosSettings.ContainerName);
        return container;
    }
}
