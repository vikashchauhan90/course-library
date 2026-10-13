namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

internal static class CosmosActivityNames
{
    public const string GetItem = "Cosmos.GetItem";

    public const string ReadItem = "Cosmos.ReadItem";
    public const string UpsertItem = "Cosmos.UpsertItem";
    public const string DeleteItem = "Cosmos.DeleteItem";
    public const string QueryItems = "Cosmos.QueryItems";
    public const string QueryPage = "Cosmos.QueryPage";


}

internal static class OperationNames
{
    public const string GetItem = "GetItem";
    public const string ReadItem = "ReadItem";
    public const string UpsertItem = "UpsertItem";
    public const string DeleteItem = "DeleteItem";
    public const string QueryItems = "QueryItems";
    public const string QueryPage = "QueryPage";
}
