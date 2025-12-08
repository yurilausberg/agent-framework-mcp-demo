# Web Hosting with Agent 365 SDK

This guide explains how to extend the Pet Store Agent to be hosted as a web service using ASP.NET Core, making it compatible with Agent 365 platform hosting requirements.

## Converting to Web Host

To host the agent as a web service for Agent 365 integration:

### 1. Update Project Type

Modify `agent-framework-mcp-demo.csproj`:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net9.0</TargetFramework>
  <RootNamespace>agent_framework_mcp_demo</RootNamespace>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

### 2. Create Web Program.cs (Alternative)

Create a `WebProgram.cs` for web hosting:

```csharp
using agent_framework_mcp_demo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the PetStoreAgent as a singleton
builder.Services.AddSingleton<PetStoreAgent>();

// Add Agent 365 SDK services
// builder.Services.AddAgentApplicationOptions();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Agent endpoint
app.MapPost("/api/agent/message", async (string message, PetStoreAgent agent) =>
{
    var messages = await agent.ProcessMessageAsync(message);
    return Results.Ok(messages);
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
```

### 3. Agent Controller (Alternative)

Create `Controllers/AgentController.cs`:

```csharp
using agent_framework_mcp_demo;
using Microsoft.AspNetCore.Mvc;

namespace agent_framework_mcp_demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly PetStoreAgent _agent;

    public AgentController(PetStoreAgent agent)
    {
        _agent = agent;
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] string agentName)
    {
        await _agent.InitializeAsync(agentName);
        return Ok(new { agentId = _agent.AgentId, agentName = _agent.AgentName });
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        var messages = await _agent.ProcessMessageAsync(message);
        return Ok(messages);
    }
}
```

### 4. Docker Support

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["agent-framework-mcp-demo.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "agent-framework-mcp-demo.dll"]
```

### 5. Azure Deployment

Deploy to Azure App Service:

```bash
# Login to Azure
az login

# Create resource group
az group create --name agent365-rg --location eastus

# Create App Service plan
az appservice plan create --name agent365-plan --resource-group agent365-rg --sku B1 --is-linux

# Create web app
az webapp create --resource-group agent365-rg --plan agent365-plan --name petstoreagent --runtime "DOTNET:9.0"

# Deploy
dotnet publish -c Release
cd bin/Release/net9.0/publish
az webapp deploy --resource-group agent365-rg --name petstoreagent --src-path .
```

### 6. Agent 365 Registration

After deploying:

1. Note the web app URL (e.g., `https://petstoreagent.azurewebsites.net`)
2. Update `agent-manifest.json` with the production URL
3. Register in Microsoft 365 Admin Center
4. Configure authentication and permissions

## Benefits of Web Hosting

- **Multi-Channel Support**: Accessible from Teams, Copilot, and web
- **Scalability**: Azure App Service auto-scaling
- **Monitoring**: Application Insights integration
- **Security**: Managed identity and Key Vault integration
- **CI/CD**: GitHub Actions or Azure DevOps pipelines

## Current Implementation

The current console application is perfect for:
- Local development and testing
- Quick prototyping
- Single-user scenarios
- Learning and experimentation

For production deployment to Agent 365, consider converting to the web hosting model.
