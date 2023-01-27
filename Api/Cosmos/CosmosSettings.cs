namespace Api.Cosmos;
internal class CosmosSettings
{
    public string DatabaseName { get; set; }
    public string ContainerName { get; set; }
    public string PartitionKey { get; set; }
    public string CosmosConnectionString { get; set; }
}
