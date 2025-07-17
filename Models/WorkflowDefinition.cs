namespace WorkflowEngine.Models;

public class WorkflowDefinition
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required List<State> States { get; set; } = new();
    public required List<WorkflowAction> Actions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
