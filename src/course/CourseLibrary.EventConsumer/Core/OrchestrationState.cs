using Microsoft.DurableTask.Client;

namespace CourseLibrary.EventConsumer.Core;

internal static class OrchestrationState
{

    public static bool IsActiveInstance(OrchestrationMetadata? existingInstance)
    {
        return existingInstance?.RuntimeStatus is
            OrchestrationRuntimeStatus.Pending or
            OrchestrationRuntimeStatus.Running or
            OrchestrationRuntimeStatus.Suspended;
    }

    public static bool IsStaleInstance(
    OrchestrationMetadata? existingInstance)
    {
        return existingInstance?.RuntimeStatus is
            OrchestrationRuntimeStatus.Completed or
            OrchestrationRuntimeStatus.Failed or
            OrchestrationRuntimeStatus.Terminated;
    }
}
