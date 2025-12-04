# Agent Framework MCP Demo

A .NET console application demonstrating the integration of Azure AI Agents with Model Context Protocol (MCP) tools. This project showcases how to create persistent AI agents that can leverage external MCP servers for enhanced capabilities.

## Features

- **Azure AI Agent Integration**: Uses Azure AI Projects for creating and managing persistent agents
- **MCP Tool Support**: Demonstrates integration with Model Context Protocol servers
- **Interactive Console Interface**: Command-line interface for creating agents and interacting with them
- **Code Interpreter Tools**: Built-in support for code execution capabilities
- **Tool Approval Workflow**: Handles tool execution approvals for secure operations

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
├── agent-framework-mcp-demo.csproj  # Project file with dependencies
├── appsettings.json          # Configuration (not in repo)
├── appsettings.sample.json   # Sample configuration template
└── README.md                 # This file
```

## Key Components

### Agent Creation
The application creates a persistent agent with:
- Custom instructions (configured as "petstore owner" in the demo)
- Code interpreter capabilities
- MCP tool integration

### Tool Integration
- **Code Interpreter**: Built-in tool for code execution
- **MCP Tools**: External tools accessed via Model Context Protocol
- **Tool Approval**: Interactive approval process for tool executions

### Message Handling
- Creates conversation threads
- Processes user messages
- Handles agent responses and tool outputs
- Displays conversation history with timestamps

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Azure.AI.Projects | 1.1.0 | Azure AI project integration |
| Azure.Identity | 1.17.1 | Azure authentication |
| Microsoft.Agents.AI.AzureAI | 1.0.0-preview.251104.1 | AI agent framework |
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

## Additional Resources

- [Azure AI Projects Documentation](https://docs.microsoft.com/azure/ai-services/openai/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/dotnet/core/)