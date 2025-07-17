using WorkflowEngine.Services;
using WorkflowEngine.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Workflow Definition endpoints
app.MapPost("/api/definitions", async (CreateWorkflowDefinitionRequest request, IWorkflowService service) =>
{
    var result = await service.CreateDefinitionAsync(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("CreateWorkflowDefinition")
.WithOpenApi();

app.MapGet("/api/definitions/{definitionId}", async (string definitionId, IWorkflowService service) =>
{
    var result = await service.GetDefinitionAsync(definitionId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
})
.WithName("GetWorkflowDefinition")
.WithOpenApi();

app.MapGet("/api/definitions", async (IWorkflowService service) =>
{
    var result = await service.GetAllDefinitionsAsync();
    return Results.Ok(result);
})
.WithName("GetAllWorkflowDefinitions")
.WithOpenApi();

// Workflow Instance endpoints
app.MapPost("/api/instances/{definitionId}", async (string definitionId, IWorkflowService service) =>
{
    var result = await service.StartInstanceAsync(definitionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("StartWorkflowInstance")
.WithOpenApi();

app.MapPost("/api/instances/{instanceId}/actions", async (string instanceId, ExecuteActionRequest request, IWorkflowService service) =>
{
    var result = await service.ExecuteActionAsync(instanceId, request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("ExecuteWorkflowAction")
.WithOpenApi();

app.MapGet("/api/instances/{instanceId}", async (string instanceId, IWorkflowService service) =>
{
    var result = await service.GetInstanceAsync(instanceId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
})
.WithName("GetWorkflowInstance")
.WithOpenApi();

app.MapGet("/api/instances", async (IWorkflowService service) =>
{
    var result = await service.GetAllInstancesAsync();
    return Results.Ok(result);
})
.WithName("GetAllWorkflowInstances")
.WithOpenApi();

app.Run();
