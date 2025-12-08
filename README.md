# Agent Framework MCP Demo

A .NET console application demonstrating the integration of Azure AI Agents with Model Context Protocol (MCP) tools, encapsulated using the **Microsoft Agent 365 SDK**. This project showcases how to create persistent AI agents that can leverage external MCP servers for enhanced capabilities and are compatible with Microsoft 365 Agent platform.

## Features

- **Agent 365 SDK Integration**: Encapsulates the agent using Microsoft Agent 365 SDK for enterprise-ready deployment
- **Azure AI Agent Integration**: Uses Azure AI Projects for creating and managing persistent agents
- **MCP Tool Support**: Demonstrates integration with Model Context Protocol servers
- **Interactive Console Interface**: Command-line interface for creating agents and interacting with them
- **Tool Approval Workflow**: Handles tool execution approvals for secure operations
- **Enterprise-Ready Architecture**: Structured for compatibility with Microsoft 365 Agent ecosystem

## Prerequisites

- .NET 9.0 SDK
- Azure CLI (for authentication)
- Azure AI Project with deployed model
- Access to an MCP server endpoint

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

2. **Follow the interactive prompts**:
   - Enter a name for your new agent
   - Provide a prompt/question for the agent
   - The agent will process your request using available tools
   - Review and approve any tool executions when prompted

## Project Structure

```
├── Program.cs                 # Main application entry point
├── PetStoreAgent.cs           # Agent 365 SDK encapsulated agent class
├── agent-framework-mcp-demo.csproj  # Project file with dependencies
├── agent-manifest.json        # Agent 365 manifest for deployment
├── appsettings.json          # Configuration (not in repo)
├── appsettings.sample.json   # Sample configuration template
└── README.md                 # This file
```

## Key Components

### Agent 365 SDK Integration
The application uses the Microsoft Agent 365 SDK to encapsulate agent functionality:
- **PetStoreAgent**: Encapsulated agent class implementing agent logic
- **Agent Manifest**: Declarative agent definition for Agent 365 platform
- **Hosting Infrastructure**: Ready for ASP.NET Core hosting and enterprise deployment

### Agent Creation
The application creates a persistent agent with:
- Custom instructions (configured as "petstore owner" in the demo)
- MCP tool integration
- Agent 365 SDK compatibility

### Tool Integration
- **MCP Tools**: External tools accessed via Model Context Protocol
- **Tool Approval**: Interactive approval process for tool executions

### Message Handling
- Creates conversation threads
- Processes user messages
- Handles agent responses and tool outputs
- Displays conversation history with timestamps
- Interactive loop for multiple queries

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Azure.AI.Projects | 1.1.0 | Azure AI project integration |
| Azure.Identity | 1.17.1 | Azure authentication |
| Microsoft.Agents.AI.AzureAI | 1.0.0-preview.251104.1 | AI agent framework |
| Microsoft.Agents.Client | 1.3.175 | **Agent 365 SDK - Core agent functionality** |
| Microsoft.Agents.Hosting.AspNetCore | 1.3.175 | **Agent 365 SDK - Hosting infrastructure** |
| Microsoft.Extensions.Configuration | 10.0.0 | Configuration management |
| Microsoft.Extensions.Configuration.Json | 10.0.0 | JSON configuration support |

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

- [Microsoft Agent 365 SDK Documentation](https://learn.microsoft.com/en-us/microsoft-agent-365/developer/)
- [Microsoft 365 Agents SDK](https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/)
- [Azure AI Projects Documentation](https://docs.microsoft.com/azure/ai-services/openai/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/dotnet/core/)
- [Agent 365 Samples](https://github.com/microsoft/Agent365-Samples)