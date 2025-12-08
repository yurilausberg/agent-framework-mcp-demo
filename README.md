# Agent Framework MCP Demo

A .NET web application demonstrating the integration of Azure AI Agents with Model Context Protocol (MCP) tools using the **Microsoft Agent 365 Framework**. This project showcases how to create enterprise-ready AI agents that can leverage external MCP servers and are fully integrated with the Agent 365 platform for monitoring, management, and observability.

## Features

- **Agent 365 Framework Integration**: Built on Agent 365 Framework with AgentApplication base class
- **A365 Dashboard Compatible**: Full observability and telemetry for Agent 365 dashboard monitoring
- **Azure AI Agent Integration**: Uses Azure AI Projects for creating and managing persistent agents with MCP tools
- **MCP Tool Support**: Demonstrates integration with Model Context Protocol servers
- **ASP.NET Core Hosting**: Web-based agent with REST API endpoints
- **Tool Approval Workflow**: Automatic approval of MCP tool executions
- **OpenTelemetry Integration**: Complete metrics, traces, and observability
- **Enterprise-Ready Architecture**: Production-ready for Microsoft 365 Agent ecosystem deployment

## Prerequisites

- .NET 9.0 SDK
- Azure CLI (for authentication)
- Azure AI Project with deployed model
- Access to an MCP server endpoint
- Agent 365 Dashboard (optional, for production monitoring)

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd agent-framework-mcp-demo
   ```

2. **Configure application settings**
   
   Copy the sample configuration file:
   ```bash
   cp appsettings.sample.json appsettings.json
   ```
   
   Edit `appsettings.json` with your Azure AI Project details:
   ```json
   {
     "FoundryProject": {
       "URL": "https://your-project.cognitiveservices.azure.com/",
       "ModelName": "your-deployed-model-name"
     },
     "MCPToolDefinition": {
       "Name": "your-mcp-tool-name",
       "URL": "https://your-mcp-server-endpoint"
     }
   }
   ```

3. **Authenticate with Azure**
   ```bash
   az login
   ```

4. **Install dependencies**
   ```bash
   dotnet restore
   ```

## Usage

1. **Run the application**
   ```bash
   dotnet run
   ```
   
   The agent will start as a web service on `http://localhost:3978`

2. **Test the agent**:
   - Use the Agent 365 Playground for interactive testing
   - Send POST requests to `/api/messages` endpoint
   - Access via Microsoft Teams (when registered)
   - Monitor via Agent 365 Dashboard (when deployed)

3. **Sending messages**:
   ```bash
   curl -X POST http://localhost:3978/api/messages \
     -H "Content-Type: application/json" \
     -d '{"type":"message","text":"Hello, Pet Store Agent!"}'
   ```

## Project Structure

```
├── Program.cs                 # ASP.NET Core web application entry point
├── PetStoreAgent.cs           # AgentApplication implementation with MCP support
├── AspNetExtensions.cs        # Authentication and token validation extensions
├── Telemetry/
│   ├── AgentMetrics.cs        # OpenTelemetry metrics and tracing
│   ├── A365OtelWrapper.cs     # Agent 365 observability wrapper
│   └── AgentOTELExtensions.cs # OpenTelemetry configuration
├── agent-framework-mcp-demo.csproj  # Web SDK project file with A365 packages
├── agent-manifest.json        # Agent 365 manifest for deployment
├── appsettings.json          # Configuration (not in repo)
├── appsettings.sample.json   # Sample configuration template
└── README.md                 # This file
```

## Key Components

### Agent 365 Framework Integration
The application is built on the Microsoft Agent 365 Framework following official patterns:
- **PetStoreAgent**: Extends `AgentApplication` base class with MCP tool integration
- **A365 Observability**: Complete OpenTelemetry integration for dashboard monitoring
- **A365 Tooling Services**: `IMcpToolRegistrationService` for MCP tool management
- **Authentication**: Token validation for secure agent-to-agent communication
- **Hosting Infrastructure**: ASP.NET Core web application with `/api/messages` endpoint

### Agent Creation and Management
The application dynamically creates persistent agents with:
- Custom instructions for pet store assistance
- MCP tool integration from Azure AI Projects
- Conversation state management across turns
- Agent caching for performance optimization

### Tool Integration
- **MCP Tools**: Azure AI Projects persistent agents with MCP tool definitions
- **Tool Approval**: Automatic approval workflow for MCP tool executions
- **A365 Tooling**: Integration with Agent 365 tooling services

### Observability and Monitoring
- **OpenTelemetry**: Metrics, traces, and activities for comprehensive monitoring
- **Agent Metrics**: Custom metrics for message processing, routes, and conversations
- **A365 Dashboard**: Full integration for production monitoring and management
- **Baggage Propagation**: Tenant ID and Agent ID tracking across operations

### Message Handling
- ASP.NET Core endpoint at `/api/messages`
- Streaming response support for real-time feedback
- Conversation thread persistence
- Multiple authentication handlers (agentic and user)

## Dependencies

### Agent 365 Framework Packages
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Agents.A365.Notifications | *-beta.* | Agent 365 notifications support |
| Microsoft.Agents.A365.Tooling.Extensions.AgentFramework | *-beta.* | **MCP tooling integration** |
| Microsoft.Agents.A365.Observability.Extensions.AgentFramework | *-beta.* | **A365 dashboard observability** |
| Microsoft.Agents.AI | 1.0.0-preview.251113.1 | AI agent framework |
| Microsoft.Agents.Hosting.AspNetCore | 1.3.*-* | ASP.NET Core hosting |
| Microsoft.Agents.Authentication.Msal | 1.3.*-* | MSAL authentication |

### Azure AI Integration
| Package | Version | Purpose |
|---------|---------|---------|
| Azure.AI.Projects | 1.1.0 | Azure AI Projects (MCP tools) |
| Azure.Identity | 1.17.1 | Azure authentication |
| Microsoft.Agents.AI.AzureAI | 1.0.0-preview.251104.1 | Azure AI integration |

### Telemetry and Observability
| Package | Version | Purpose |
|---------|---------|---------|
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.12.0 | OTLP exporter |
| OpenTelemetry.Extensions.Hosting | 1.12.0 | Hosting integration |
| OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | ASP.NET Core telemetry |
| OpenTelemetry.Instrumentation.Http | 1.12.0 | HTTP telemetry |
| OpenTelemetry.Instrumentation.Runtime | 1.12.0 | Runtime metrics |

## Configuration Options

### FoundryProject
- **URL**: Your Azure AI Project endpoint URL
- **ModelName**: The name of your deployed model in the project

### MCPToolDefinition
- **Name**: Display name for your MCP tool
- **URL**: Endpoint URL for your MCP server

## Security Considerations

- The application uses Azure CLI credentials for authentication
- Tool executions require explicit user approval
- Configuration files should not be committed to version control
- Ensure MCP server endpoints are secure and trusted

## Troubleshooting

### Common Issues

1. **Missing configuration**: Ensure `appsettings.json` is properly configured
2. **Authentication errors**: Run `az login` and verify your Azure credentials
3. **Model not found**: Verify the model name matches your Azure AI Project deployment
4. **MCP server connection**: Ensure the MCP server URL is accessible and running

### Error Messages
- `Missing FoundryProject:URL`: Check your `appsettings.json` configuration
- `Missing FoundryProject:Model`: Verify your model deployment name
- `Missing MCPToolDefinition:Name/URL`: Complete the MCP tool configuration

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Agent 365 SDK Features

This application is encapsulated with the Microsoft Agent 365 SDK, which provides:

- **Enterprise-Grade Identity**: Secure authentication and authorization
- **Multi-Channel Support**: Compatible with Teams, Copilot, and web applications
- **Observability**: Built-in telemetry and monitoring capabilities
- **Governed Access**: Secure access to organizational data and tools
- **Deployment Ready**: Infrastructure for production deployment to Agent 365 platform

### Agent 365 Deployment

To deploy this agent to the Agent 365 platform:

1. **Customize the agent manifest** (`agent-manifest.json`):
   - Update the agent ID, name, and descriptions
   - Configure permissions and valid domains
   - Adjust tool definitions as needed

2. **Deploy to Azure**:
   ```bash
   az login
   dotnet publish -c Release
   # Deploy to Azure App Service or Container Apps
   ```

3. **Register with Agent 365**:
   - Use the Microsoft 365 Admin Center to register your agent
   - Upload the agent manifest
   - Configure policies and governance

4. **Test in Microsoft 365**:
   - Access your agent through Teams, Copilot, or web chat
   - Test multi-channel interactions
   - Monitor telemetry and usage

## Additional Resources

- [Microsoft Agent 365 Developer Documentation](https://learn.microsoft.com/en-us/microsoft-agent-365/developer/)
- [Microsoft 365 Agents SDK](https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/)
- [Microsoft Agents for .NET](https://github.com/microsoft/Agents-for-net)
- [Azure AI Projects Documentation](https://docs.microsoft.com/azure/ai-services/openai/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/dotnet/core/)