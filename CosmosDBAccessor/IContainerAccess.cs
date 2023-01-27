using Microsoft.Azure.Cosmos;

namespace CosmosDBAccessor;

public interface IContainerAccess<T>
{
    Task<T> Get(string id, string partitionKey);
    Task<IEnumerable<T>> GetItems(string partitionKey);
    Task<IEnumerable<T>> QueryItems(string query, string partitionKey, List<KeyValuePair<string, object>> queryParams);
    Task Create(T objectToStore);
    Task Replace(T objectToStore);
    Task Patch(T objectToPatch, IReadOnlyList<PatchOperation> patchOperations);
    Task Delete(string id, string partitionKey);
}