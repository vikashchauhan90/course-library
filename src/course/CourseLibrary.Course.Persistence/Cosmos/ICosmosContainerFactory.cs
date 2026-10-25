using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Course.Persistence.Cosmos;

internal interface ICosmosContainerFactory
{
    Container GetContainer(string containerName);
}
