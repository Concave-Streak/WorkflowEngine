namespace WorkflowEngine.Models;

public class WorkflowAction
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public bool Enabled { get; set; } = true;
    public required List<string> FromStates { get; set; } = new();
    public required string ToState { get; set; }
    public string? Description { get; set; }
}
