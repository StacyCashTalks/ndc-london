namespace CosmosDBAccessor;

public record CosmosEndpointKeySettings(string Endpoint, string Key, string DatabaseName, string ContainerName);

public record CosmosConnectionStringSettings(string ConnectionString, string DatabaseName, string ContainerName);

public record CosmosDefaultCredentialSettings(string Endpoint, string DatabaseName, string ContainerName);
