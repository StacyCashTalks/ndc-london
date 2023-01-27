using System.Reflection;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace CosmosDBAccessor;

public class CosmosContainerAccess<T> : IDisposable, IContainerAccess<T> where T : ICosmosEntity
{
    private readonly string _databaseName;
    private readonly string _containerName;
    private readonly CosmosClient _cosmosClient;
    private Container? _container;

    private static string TypeSpecifier => $"{typeof(T).FullName}, {typeof(T).Assembly.GetName().Name}";

    // Access via connection string details
    public CosmosContainerAccess(CosmosEndpointKeySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        _databaseName = settings.DatabaseName;
        _containerName = settings.ContainerName;

        _cosmosClient = new CosmosClientBuilder(settings.Endpoint, settings.Key)
            .WithApplicationName(Assembly.GetExecutingAssembly().GetName().Name)
            .WithCustomSerializer(new CustomCosmosSerializer())
            .Build();
    }

    //Access via managed identities
    public CosmosContainerAccess(CosmosDefaultCredentialSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        _databaseName = settings.DatabaseName;
        _containerName = settings.ContainerName;

        _cosmosClient = new CosmosClientBuilder(settings.Endpoint, new DefaultAzureCredential())
            .WithApplicationName(Assembly.GetExecutingAssembly().GetName().Name)
            .Build();
    }

    public async Task Create(T objectToStore)
    {
        var cosmosContainer = await GetCosmosContainer();
        await cosmosContainer.CreateItemAsync(objectToStore, new PartitionKey(GetPartitionKeyValue(objectToStore)));
    }

    public async Task<T> Get(string id, string partitionKey)
    {
        var cosmosContainer = await GetCosmosContainer();
        // TODO: How to check that the item is the correct type
        return await cosmosContainer.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
    }

    public async Task<IEnumerable<T>> GetItems(string partitionKey)
    {
        var selectFromCWhereCTypeType = $"SELECT * FROM c WHERE c[\"$type\"] = @type AND c[\"{PartitionKeyName}\"] = @partitionKey";

        List<KeyValuePair<string, object>> queryParameters = new()
        {
            new KeyValuePair<string, object>("@type", TypeSpecifier),
            new KeyValuePair<string, object>("@partitionKey", partitionKey)
        };

        return await QueryItems(selectFromCWhereCTypeType, partitionKey, queryParameters);
    }

    public async Task<IEnumerable<T>> QueryItems(string query, string partitionKey, List<KeyValuePair<string, object>> queryParams)
    {
        var cosmosContainer = await GetCosmosContainer();
        QueryDefinition queryDefinition = new(query);
        queryParams.ForEach(p => queryDefinition.WithParameter(p.Key, p.Value));

        using var queryIterator = cosmosContainer.GetItemQueryIterator<T>(
            queryDefinition,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey), }
            );

        var results = new List<T>();
        while (queryIterator.HasMoreResults)
        {
            var response = await queryIterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task Replace(T objectToStore)
    {
        var cosmosContainer = await GetCosmosContainer();
        // TODO: Check that the item is the correct type
        // TODO: Check that the item exists before trying to replace it
        await cosmosContainer.ReplaceItemAsync(objectToStore, objectToStore.Id, new PartitionKey(GetPartitionKeyValue(objectToStore)));
    }

    public async Task Patch(T objectToPatch, IReadOnlyList<PatchOperation> patchOperations)
    {
        var cosmosContainer = await GetCosmosContainer();
        // TODO: Check that the item is the correct type
        // TODO: Check that the item exists before trying to patch it
        await cosmosContainer.PatchItemAsync<T>(objectToPatch.Id, new PartitionKey(GetPartitionKeyValue(objectToPatch)), patchOperations);
    }

    public async Task Delete(string id, string partitionKey)
    {
        var cosmosContainer = await GetCosmosContainer();
        // TODO: Check that the item is the correct type
        // TODO: Check that the item exists before trying to patch it
        await cosmosContainer.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
    }

    private async Task<Container> GetCosmosContainer()
    {
        // TODO: Lock before getting the container
        if (_container == null)
        {
            _container = await CreateContainer();
        }

        return _container;
    }

    private async Task<Container> CreateContainer()
    {
        Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);

        var partitionKey = $"/{PartitionKeyName}";
        ContainerProperties containerProperties = new()
        {
            Id = _containerName,
            PartitionKeyPath = partitionKey
        };

        await database.CreateContainerIfNotExistsAsync(containerProperties);
        var container = database.GetContainer(_containerName);
        return container;
    }

    private static string PartitionKeyName
    {
        get
        {
            var partitionKeyRaw = GetPartitionKeyName();
            var partitionKey = $"{partitionKeyRaw.First().ToString().ToLowerInvariant()}{partitionKeyRaw[1..]}";
            return partitionKey;
        }
    }

    private static string GetPartitionKeyValue(T objectToStore)
    {
        var testType = typeof(T).GetProperties();
        var values = new List<string>();

        foreach (var propertyInfo in testType)
        {
            var att = (PartitionKeyAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(PartitionKeyAttribute), true);
            if (att == null) continue;

            var value = propertyInfo.GetValue(objectToStore);
            if (value is null) continue;

            var stringValue = value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue)) continue;

            values.Add(stringValue);
        }

        return values.Count switch
        {
            0 => throw new InvalidDataException(),
            1 => values[0],
            _ => throw new InvalidDataException()
        };
    }

    private static string GetPartitionKeyName()
    {

        var properties = typeof(T).GetProperties();

        var propertyInfos = new List<PropertyInfo>();

        foreach (var property in properties)
        {
            var att = (PartitionKeyAttribute?)Attribute.GetCustomAttribute(property, typeof(PartitionKeyAttribute), true);
            if (att != null)
            {
                propertyInfos.Add(property);
            }

        }

        return propertyInfos.Count switch
        {
            0 => throw new InvalidDataException(),
            1 => propertyInfos[0].Name,
            _ => throw new InvalidDataException()
        };
    }

    public void Dispose()
    {
        _cosmosClient.Dispose();
    }
}