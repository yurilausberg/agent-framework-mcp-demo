using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.Client;
using Microsoft.Extensions.Configuration;

namespace agent_framework_mcp_demo;

/// <summary>
/// Pet Store Agent implementation using Agent 365 SDK
/// </summary>
public class PetStoreAgent
{
    private readonly IConfiguration _configuration;
    private readonly Azure.AI.Projects.AIProjectClient _projectClient;
    private readonly PersistentAgentsClient _persistentAgentsClient;
    private PersistentAgent? _agentMetadata;
    private AIAgent? _agent;

    public PetStoreAgent(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Initialize the AI project client using Azure CLI credentials
        _projectClient = new Azure.AI.Projects.AIProjectClient(
            new Uri(_configuration["FoundryProject:URL"] ?? throw new Exception("Missing FoundryProject:URL in appsettings.json")),
            new AzureCliCredential());
        _persistentAgentsClient = _projectClient.GetPersistentAgentsClient();
    }

    /// <summary>
    /// Initialize the agent with specified name
    /// </summary>
    public async Task InitializeAsync(string agentName)
    {
        // Create a new agent
        _agentMetadata = await _persistentAgentsClient.Administration.CreateAgentAsync(
            model: _configuration["FoundryProject:ModelName"] ?? throw new Exception("Missing FoundryProject:Model in appsettings.json"),
            instructions: "You are a petstore owner",
            name: agentName,
            tools: [
                new MCPToolDefinition(
                    _configuration["MCPToolDefinition:Name"] ?? throw new Exception("Missing MCPToolDefinition:Name in appsettings.json"),
                    _configuration["MCPToolDefinition:URL"] ?? throw new Exception("Missing MCPToolDefinition:URL in appsettings.json"))
            ]
        );
        
        Console.WriteLine($"Successfully created agent with ID: {_agentMetadata.Id}");
        
        // Get the created agent
        _agent = await _persistentAgentsClient.GetAIAgentAsync(_agentMetadata.Id);
    }

    /// <summary>
    /// Process a user message and return the conversation
    /// </summary>
    public async Task<IEnumerable<PersistentThreadMessage>> ProcessMessageAsync(string userMessage)
    {
        if (_agentMetadata == null || _agent == null)
        {
            throw new InvalidOperationException("Agent not initialized. Call InitializeAsync first.");
        }

        // Create a new thread
        PersistentAgentThread thread = _persistentAgentsClient.Threads.CreateThread();

        // Create message to thread
        PersistentThreadMessage message = _persistentAgentsClient.Messages.CreateMessage(
            thread.Id,
            MessageRole.User,
            userMessage);

        // Create run
        ThreadRun run = _persistentAgentsClient.Runs.CreateRun(thread: thread, agent: _agentMetadata, cancellationToken: default);

        // Wait for the run to complete
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            run = _persistentAgentsClient.Runs.GetRun(thread.Id, run.Id);

            // Handle tool approval if required
            if (run.Status == RunStatus.RequiresAction && run.RequiredAction is SubmitToolApprovalAction toolApprovalAction)
            {
                var toolApprovals = new List<ToolApproval>();
                foreach (var toolCall in toolApprovalAction.SubmitToolApproval.ToolCalls)
                {
                    if (toolCall is RequiredMcpToolCall mcpToolCall)
                    {
                        Console.WriteLine($"Approving MCP tool call: {mcpToolCall.Name}, Arguments: {mcpToolCall.Arguments}");
                        toolApprovals.Add(new ToolApproval(mcpToolCall.Id, approve: true));
                    }
                }

                if (toolApprovals.Count > 0)
                {
                    run = _persistentAgentsClient.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolApprovals: toolApprovals);
                }
            }
        }

        // Get messages in the thread
        Azure.Pageable<PersistentThreadMessage> messages = _persistentAgentsClient.Messages.GetMessages(
            threadId: thread.Id,
            order: ListSortOrder.Ascending
        );

        return messages.ToList();
    }

    /// <summary>
    /// Get agent ID
    /// </summary>
    public string? AgentId => _agentMetadata?.Id;

    /// <summary>
    /// Get agent name
    /// </summary>
    public string? AgentName => _agentMetadata?.Name;
}
