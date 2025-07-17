# Configurable Workflow Engine

A minimal backend service that implements a configurable state-machine workflow engine using .NET 8 and ASP.NET Core Minimal APIs.

## Features

- **Workflow Definitions**: Create and manage workflow definitions with states and actions
- **State Machine Logic**: Enforce state transitions with full validation
- **Workflow Instances**: Start and manage multiple workflow instances
- **Action Execution**: Execute actions with proper validation and history tracking
- **In-Memory Storage**: Simple in-memory persistence

## Quick Start

### Prerequisites
- .NET 8 SDK installed, [Download Here](https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.412/dotnet-sdk-8.0.412-win-x64.exe)
- Any IDE that supports .NET (Visual Studio, VS Code, etc.)

### Running the Application

1. **Clone or download the project files**
```bash
git clone https://github.com/Concave-Streak/WorkflowEngine.git
```

2. **Navigate to the project directory**
```bash
cd WorkflowEngine
```

3. **Restore dependencies**
```bash
dotnet restore
```

4. **Run the application**
```bash
dotnet run
```
To test the application using Swagger web UI:
```bash
dotnet run --environment Development
```

5. **Access the API**
- Application URL: `http://localhost:5000` (or `https://localhost:5001`)
- Swagger web UI: `http://localhost:5000/swagger`


### Testing the Application
- A simple python script `check.py` is included for automatically testing the endpoints
- Swagger web UI automatically generates live, interactive documentation for all the endpoints
    - Each endpoint displays the expected request/response formats
    - Endpoints can be directly Tested from the browser using the "Try it out" feature


## API Endpoints

### Workflow Definitions
- `POST /api/definitions` - Create a new workflow definition
- `GET /api/definitions` - Get all workflow definitions
- `GET /api/definitions/{definitionId}` - Get a specific workflow definition

### Workflow Instances
- `POST /api/instances/{definitionId}` - Start a new workflow instance
- `GET /api/instances` - Get all workflow instances
- `GET /api/instances/{instanceId}` - Get a specific workflow instance
- `POST /api/instances/{instanceId}/actions` - Execute an action on an instance


## Example Usage

### 1. Create a Workflow Definition
```json
POST /api/definitions
{
"name": "Order Processing",
"description": "Simple order processing workflow",
"states": [
{
"id": "pending",
"name": "Pending",
"isInitial": true,
"isFinal": false,
"enabled": true
},
{
"id": "approved",
"name": "Approved",
"isInitial": false,
"isFinal": false,
"enabled": true
},
{
"id": "completed",
"name": "Completed",
"isInitial": false,
"isFinal": true,
"enabled": true
}
],
"actions": [
{
"id": "approve",
"name": "Approve Order",
"enabled": true,
"fromStates": ["pending"],
"toState": "approved"
},
{
"id": "complete",
"name": "Complete Order",
"enabled": true,
"fromStates": ["approved"],
"toState": "completed"
}
]
}
```

### 2. Start a Workflow Instance
```json
POST /api/instances/{definitionId}
```

### 3. Execute an Action
```json
POST /api/instances/{instanceId}/actions
{
"actionId": "approve"
}
```


## Implementation

### Concepts
- **State**: id(unique), name(human readable identifier), isInitial(bool), isFinal(bool),
enabled(bool)
- **Action**: id(unique), name(human readable identifier), enabled(bool), fromStates(collection of state IDs), toState(singlestate ID)
- **Workflow definition**: Collection of states and actions, makes sure there's one starting point, and checks all the references are valid.
- **Workflow instance**: References a workflow definition, tracks current state and maintains execution history

### Validation Rules
- **Definition Validation**
    - No duplicate state or action IDs
    - Exactly one initial state required
    - All action references must point to existing states

- **Action Execution Validation**
    - Action must be enabled
    - Current state must be in action's fromStates
    - Cannot execute actions on final states
    - Target state must exist and be enabled

## Architecture

The solution has a well-structured, modular architecture optimized for readability and expandability:

- **Models**: Core domain entities
- **DTOs**: Data transfer objects for API contracts
- **Services**: Business logic and validation
- **Program.cs**: API endpoints and configuration

The architecture balances separation of concerns and simplicity in a manner that makes it simple to extend the solution, i.e., add validation rules or features without having to rebuild.

## Assumptions and Limitations

1. **In-Memory Storage**: Data is lost when the application restarts
2. **Single Instance**: No support for distributed scenarios
3. **Synchronous Processing**: All operations are synchronous
4. **No Authentication**: No security layer implemented
5. **No Concurrency Control**: No locking mechanism for concurrent access

## Future Enhancements

With more time, the following could be added:

- Persistent storage (database integration)
- Async processing capabilities
- Workflow versioning
- Role-based access control
- Conditional transitions
- Workflow scheduling
- Event notifications
- Audit logging
- Performance monitoring

## Error Handling

The API returns consistent error responses with:
- Success/failure status
- Error messages
- Validation errors (when applicable)
- HTTP status codes (200, 400, 404)