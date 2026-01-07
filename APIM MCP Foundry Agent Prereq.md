# APIM MCP Foundry Agent Prerequisites

## 1. Shared APIM v2 (Azure API Management v2)

### Prerequisites:

- **APIM v2 Instance**: Must be provisioned in advance, using Basic v2, Standard v2, or Premium v2 SKU depending on scale and network isolation needs.

## 2. Shared Foundry Project

### Prerequisites:

- **Foundry Resource**: A shared Azure AI Foundry resource must be created, with a project container for agents, files, and evaluations.
- **Access Control**: Assign Azure AI User RBAC roles to all pod members needing to create/edit agents. Minimum permissions: `agents/*/read`, `agents/*/action`, `agents/*/delete`.
- **Model Deployments**: Ensure at least one model (e.g., GPT-4.1) is deployed and accessible via the Foundry project endpoint. >= ~500K TPM

## 3. Local Machine

### Prerequisites:

- VS Code (November 2025 (v 1.107) or later)
- .NET CLI
- Azure CLI
- Microsoft Foundry VS Code extension
- Access to Azure subscription and resources (Foundry Project)

## 4. Sample Code

- **GitHub Repo**: [agent-framework-mcp-demo](https://github.com/EXP-Projects/agent-framework-mcp-demo)
- Foundry SDK (classic mode)
- Agent Framework SDK
- C# .NET 9
- Microsoft Foundry for VSCode extension

## 5. Sample OpenAPI Endpoint

### Demo RESTful API:

- https://petstore3.swagger.io/ or BYO

## 6. Resources

- [Microsoft Foundry SDK](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/sdk-overview?view=foundry-classic&pivots=programming-language-csharp)
- [Microsoft Foundry for Visual Studio Code extension](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/get-started-projects-vs-code?view=foundry-classic)
- [MCP servers in Azure API Management](https://learn.microsoft.com/en-us/azure/api-management/mcp-server-overview)
- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Azure AI Foundry Agent](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-ai-foundry-agent?pivots=programming-language-csharp)
- [MCP Inspector](https://modelcontextprotocol.io/docs/tools/inspector)
