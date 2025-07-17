namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public required string Id { get; set; }
    public required string DefinitionId { get; set; }
    public required string CurrentStateId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<WorkflowHistoryEntry> History { get; set; } = new();
}

public class WorkflowHistoryEntry
{
    public required string ActionId { get; set; }
    public required string FromStateId { get; set; }
    public required string ToStateId { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
