using CosmosDBAccessor;

namespace Api.Models;

public class CosmosTodo: Todo, ICosmosEntity
{
    [PartitionKey]
    public string? userId { get; set; }
}
