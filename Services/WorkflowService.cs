using WorkflowEngine.Models;
using WorkflowEngine.DTOs;
using System.Collections.Concurrent;

namespace WorkflowEngine.Services;

public class WorkflowService : IWorkflowService
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
    private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new();

    public async Task<ApiResponse<WorkflowDefinition>> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request)
    {
        var validationErrors = ValidateDefinitionRequest(request);
        if (validationErrors.Any())
        {
            return ApiResponse<WorkflowDefinition>.ValidationErrorResult(validationErrors);
        }

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            States = request.States.Select(s => new State
            {
                Id = s.Id,
                Name = s.Name,
                IsInitial = s.IsInitial,
                IsFinal = s.IsFinal,
                Enabled = s.Enabled,
                Description = s.Description
            }).ToList(),
            Actions = request.Actions.Select(a => new WorkflowAction
            {
                Id = a.Id,
                Name = a.Name,
                Enabled = a.Enabled,
                FromStates = a.FromStates,
                ToState = a.ToState,
                Description = a.Description
            }).ToList()
        };

        _definitions.TryAdd(definition.Id, definition);
        return ApiResponse<WorkflowDefinition>.SuccessResult(definition);
    }

    public async Task<ApiResponse<WorkflowDefinition>> GetDefinitionAsync(string definitionId)
    {
        if (_definitions.TryGetValue(definitionId, out var definition))
        {
            return ApiResponse<WorkflowDefinition>.SuccessResult(definition);
        }

        return ApiResponse<WorkflowDefinition>.ErrorResult($"Definition with ID '{definitionId}' not found");
    }

    public async Task<ApiResponse<List<WorkflowDefinition>>> GetAllDefinitionsAsync()
    {
        var definitions = _definitions.Values.ToList();
        return ApiResponse<List<WorkflowDefinition>>.SuccessResult(definitions);
    }

    public async Task<ApiResponse<WorkflowInstance>> StartInstanceAsync(string definitionId)
    {
        if (!_definitions.TryGetValue(definitionId, out var definition))
        {
            return ApiResponse<WorkflowInstance>.ErrorResult($"Definition with ID '{definitionId}' not found");
        }

        var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
        if (initialState == null)
        {
            return ApiResponse<WorkflowInstance>.ErrorResult("No initial state found in definition");
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = definitionId,
            CurrentStateId = initialState.Id
        };

        _instances.TryAdd(instance.Id, instance);
        return ApiResponse<WorkflowInstance>.SuccessResult(instance);
    }

    public async Task<ApiResponse<WorkflowInstance>> ExecuteActionAsync(string instanceId, ExecuteActionRequest request)
    {
        if (!_instances.TryGetValue(instanceId, out var instance))
        {
            return ApiResponse<WorkflowInstance>.ErrorResult($"Instance with ID '{instanceId}' not found");
        }

        if (!_definitions.TryGetValue(instance.DefinitionId, out var definition))
        {
            return ApiResponse<WorkflowInstance>.ErrorResult("Definition not found for this instance");
        }

        var action = definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
        if (action == null)
        {
            return ApiResponse<WorkflowInstance>.ErrorResult($"Action '{request.ActionId}' not found in definition");
        }

        var validationResult = ValidateActionExecution(instance, action, definition);
        if (!validationResult.IsValid)
        {
            return ApiResponse<WorkflowInstance>.ErrorResult(validationResult.ErrorMessage);
        }

        // Execute the action
        var historyEntry = new WorkflowHistoryEntry
        {
            ActionId = action.Id,
            FromStateId = instance.CurrentStateId,
            ToStateId = action.ToState
        };

        instance.CurrentStateId = action.ToState;
        instance.History.Add(historyEntry);

        return ApiResponse<WorkflowInstance>.SuccessResult(instance);
    }

    public async Task<ApiResponse<WorkflowInstance>> GetInstanceAsync(string instanceId)
    {
        if (_instances.TryGetValue(instanceId, out var instance))
        {
            return ApiResponse<WorkflowInstance>.SuccessResult(instance);
        }

        return ApiResponse<WorkflowInstance>.ErrorResult($"Instance with ID '{instanceId}' not found");
    }

    public async Task<ApiResponse<List<WorkflowInstance>>> GetAllInstancesAsync()
    {
        var instances = _instances.Values.ToList();
        return ApiResponse<List<WorkflowInstance>>.SuccessResult(instances);
    }

    private List<string> ValidateDefinitionRequest(CreateWorkflowDefinitionRequest request)
    {
        var errors = new List<string>();

        // Check for duplicate state IDs
        var duplicateStates = request.States.GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateState in duplicateStates)
        {
            errors.Add($"Duplicate state ID: {duplicateState}");
        }

        // Check for duplicate action IDs
        var duplicateActions = request.Actions.GroupBy(a => a.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateAction in duplicateActions)
        {
            errors.Add($"Duplicate action ID: {duplicateAction}");
        }

        // Check for exactly one initial state
        var initialStates = request.States.Count(s => s.IsInitial);
        if (initialStates == 0)
        {
            errors.Add("Workflow must have exactly one initial state");
        }
        else if (initialStates > 1)
        {
            errors.Add("Workflow must have exactly one initial state, found multiple");
        }

        // Validate action references
        var stateIds = request.States.Select(s => s.Id).ToHashSet();
        foreach (var action in request.Actions)
        {
            // Check if toState exists
            if (!stateIds.Contains(action.ToState))
            {
                errors.Add($"Action '{action.Id}' references non-existent target state '{action.ToState}'");
            }

            // Check if fromStates exist
            foreach (var fromState in action.FromStates)
            {
                if (!stateIds.Contains(fromState))
                {
                    errors.Add($"Action '{action.Id}' references non-existent source state '{fromState}'");
                }
            }
        }

        return errors;
    }

    private (bool IsValid, string ErrorMessage) ValidateActionExecution(WorkflowInstance instance, WorkflowAction action, WorkflowDefinition definition)
    {
        // Check if action is enabled
        if (!action.Enabled)
        {
            return (false, $"Action '{action.Id}' is disabled");
        }

        // Check if current state is in fromStates
        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            return (false, $"Action '{action.Id}' cannot be executed from current state '{instance.CurrentStateId}'");
        }

        // Check if current state is final
        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState?.IsFinal == true)
        {
            return (false, "Cannot execute actions on final states");
        }

        // Check if target state exists and is enabled
        var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState);
        if (targetState == null)
        {
            return (false, $"Target state '{action.ToState}' not found");
        }

        if (!targetState.Enabled)
        {
            return (false, $"Target state '{action.ToState}' is disabled");
        }

        return (true, string.Empty);
    }
}
