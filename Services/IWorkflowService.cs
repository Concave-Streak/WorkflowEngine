using WorkflowEngine.Models;
using WorkflowEngine.DTOs;

namespace WorkflowEngine.Services;

public interface IWorkflowService
{
    Task<ApiResponse<WorkflowDefinition>> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request);
    Task<ApiResponse<WorkflowDefinition>> GetDefinitionAsync(string definitionId);
    Task<ApiResponse<List<WorkflowDefinition>>> GetAllDefinitionsAsync();
    Task<ApiResponse<WorkflowInstance>> StartInstanceAsync(string definitionId);
    Task<ApiResponse<WorkflowInstance>> ExecuteActionAsync(string instanceId, ExecuteActionRequest request);
    Task<ApiResponse<WorkflowInstance>> GetInstanceAsync(string instanceId);
    Task<ApiResponse<List<WorkflowInstance>>> GetAllInstancesAsync();
}
