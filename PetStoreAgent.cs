using agent_framework_mcp_demo.Telemetry;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.A365.Observability.Caching;
using Microsoft.Agents.A365.Runtime.Utils;
using Microsoft.Agents.A365.Tooling.Extensions.AgentFramework.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Core.Serialization;
using System.Collections.Concurrent;
using System.Text.Json;

namespace agent_framework_mcp_demo;

/// <summary>
/// Pet Store Agent implementation using Agent 365 SDK Framework
/// </summary>
public class PetStoreAgent : AgentApplication
{
    private readonly string AgentWelcomeMessage = "Hello! I'm your Pet Store assistant. I can help you with pet store operations.";

    private readonly string AgentInstructions = """
        You are a petstore owner assistant. 
        You help customers with pet store operations and answer questions about pets, products, and services.
        Be friendly and professional in your responses.
        Use the MCP tools available to you to access pet store data and perform operations.
        """;

    private readonly IConfiguration? _configuration;
    private readonly IExporterTokenCache<AgenticTokenStruct>? _agentTokenCache;
    private readonly ILogger<PetStoreAgent>? _logger;
    private readonly IMcpToolRegistrationService? _toolService;
    private readonly Azure.AI.Projects.AIProjectClient? _projectClient;
    private readonly PersistentAgentsClient? _persistentAgentsClient;

    // Setup reusable auto sign-in handlers
    private readonly string AgenticIdAuthHandler = "agentic";
    private readonly string MyAuthHandler = "me";

    // Cache for MCP tools per user
    private static readonly ConcurrentDictionary<string, PersistentAgent> _agentCache = new();

    public PetStoreAgent(AgentApplicationOptions options,
        IConfiguration configuration,
        IExporterTokenCache<AgenticTokenStruct> agentTokenCache,
        IMcpToolRegistrationService toolService,
        ILogger<PetStoreAgent> logger) : base(options)
    {
        _configuration = configuration;
        _agentTokenCache = agentTokenCache;
        _logger = logger;
        _toolService = toolService;

        // Initialize Azure AI Project client for MCP tools
        try
        {
            var projectUrl = _configuration["FoundryProject:URL"];
            if (!string.IsNullOrEmpty(projectUrl))
            {
                _projectClient = new Azure.AI.Projects.AIProjectClient(
                    new Uri(projectUrl),
                    new AzureCliCredential());
                _persistentAgentsClient = _projectClient.GetPersistentAgentsClient();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Could not initialize Azure AI Project client: {ex.Message}");
        }

        // Greet when members are added to the conversation
        OnConversationUpdate(ConversationUpdateEvents.MembersAdded, WelcomeMessageAsync);

        // Handle A365 Notification Messages and regular messages
        OnActivity(ActivityTypes.Message, OnMessageAsync, isAgenticOnly: true, autoSignInHandlers: new[] { AgenticIdAuthHandler });
        OnActivity(ActivityTypes.Message, OnMessageAsync, isAgenticOnly: false, autoSignInHandlers: new[] { MyAuthHandler });
    }

    protected async Task WelcomeMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        await AgentMetrics.InvokeObservedAgentOperation(
            "WelcomeMessage",
            turnContext,
            async () =>
        {
            foreach (ChannelAccount member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(AgentWelcomeMessage);
                }
            }
        });
    }

    /// <summary>
    /// General Message process for Teams and other channels. 
    /// </summary>
    protected async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        string ObservabilityAuthHandlerName = "";
        string ToolAuthHandlerName = "";
        if (turnContext.IsAgenticRequest())
            ObservabilityAuthHandlerName = ToolAuthHandlerName = AgenticIdAuthHandler;
        else
            ObservabilityAuthHandlerName = ToolAuthHandlerName = MyAuthHandler;

        await A365OtelWrapper.InvokeObservedAgentOperation(
            "MessageProcessor",
            turnContext,
            turnState,
            _agentTokenCache,
            UserAuthorization,
            ObservabilityAuthHandlerName,
            _logger,
            async () =>
        {
            // Start a Streaming Process to let clients that support streaming know that we are processing the request. 
            await turnContext.StreamingResponse.QueueInformativeUpdateAsync("Processing your request...").ConfigureAwait(false);
            try
            {
                var userText = turnContext.Activity.Text?.Trim() ?? string.Empty;

                // Get or create the persistent agent for MCP tools
                var persistentAgent = await GetOrCreatePersistentAgentAsync(turnContext, turnState);

                if (persistentAgent != null && _persistentAgentsClient != null)
                {
                    // Use Azure AI Persistent Agent with MCP tools
                    await ProcessWithPersistentAgentAsync(turnContext, turnState, userText, persistentAgent, cancellationToken);
                }
                else
                {
                    // Fallback to simple response
                    await turnContext.SendActivityAsync($"Echo: {userText}");
                }
            }
            finally
            {
                await turnContext.StreamingResponse.EndStreamAsync(cancellationToken).ConfigureAwait(false);
            }
        });
    }

    private async Task<PersistentAgent?> GetOrCreatePersistentAgentAsync(ITurnContext turnContext, ITurnState turnState)
    {
        if (_persistentAgentsClient == null || _configuration == null)
        {
            return null;
        }

        // Use a shared cache key for all users to reuse the same agent configuration
        string configKey = $"PetStore_{_configuration["FoundryProject:ModelName"]}";
        
        if (_agentCache.TryGetValue(configKey, out var cachedAgent))
        {
            return cachedAgent;
        }

        try
        {
            var modelName = _configuration["FoundryProject:ModelName"];
            var mcpToolName = _configuration["MCPToolDefinition:Name"];
            var mcpToolUrl = _configuration["MCPToolDefinition:URL"];

            if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(mcpToolName) || string.IsNullOrEmpty(mcpToolUrl))
            {
                _logger?.LogWarning("Missing configuration for FoundryProject:ModelName, MCPToolDefinition:Name, or MCPToolDefinition:URL");
                return null;
            }

            // Create agent once and share across all conversations
            var agentResponse = await _persistentAgentsClient.Administration.CreateAgentAsync(
                model: modelName,
                instructions: AgentInstructions,
                name: "PetStoreAgent",
                tools: [
                    new MCPToolDefinition(mcpToolName, mcpToolUrl)
                ]
            );

            var agent = agentResponse.Value;
            
            // Use GetOrAdd to handle race conditions when multiple requests arrive simultaneously
            var finalAgent = _agentCache.GetOrAdd(configKey, agent);
            
            if (finalAgent.Id == agent.Id)
            {
                _logger?.LogInformation($"Created new persistent agent with ID: {agent.Id}");
            }
            else
            {
                _logger?.LogInformation($"Reusing existing agent with ID: {finalAgent.Id}");
            }
            
            return finalAgent;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error creating persistent agent: {ex.Message}");
            return null;
        }
    }

    private async Task ProcessWithPersistentAgentAsync(ITurnContext turnContext, ITurnState turnState, string userText, PersistentAgent agentMetadata, CancellationToken cancellationToken)
    {
        if (_persistentAgentsClient == null)
        {
            return;
        }

        try
        {
            // Get or create thread for this conversation
            var threadInfo = turnState.Conversation.GetValue<string?>("conversation.threadId", () => null);
            PersistentAgentThread thread;

            if (string.IsNullOrEmpty(threadInfo))
            {
                thread = _persistentAgentsClient.Threads.CreateThread();
                turnState.Conversation.SetValue("conversation.threadId", thread.Id);
            }
            else
            {
                // Retrieve existing thread
                thread = _persistentAgentsClient.Threads.GetThread(threadInfo);
            }

            // Create message
            _persistentAgentsClient.Messages.CreateMessage(thread.Id, MessageRole.User, userText);

            // Create and run
            ThreadRun run = _persistentAgentsClient.Runs.CreateRun(thread: thread, agent: agentMetadata, cancellationToken: cancellationToken);

            // Wait for completion with tool approval
            while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
                run = _persistentAgentsClient.Runs.GetRun(thread.Id, run.Id);

                if (run.Status == RunStatus.RequiresAction && run.RequiredAction is SubmitToolApprovalAction toolApprovalAction)
                {
                    var toolApprovals = new List<ToolApproval>();
                    foreach (var toolCall in toolApprovalAction.SubmitToolApproval.ToolCalls)
                    {
                        if (toolCall is RequiredMcpToolCall mcpToolCall)
                        {
                            _logger?.LogInformation($"Auto-approving MCP tool call: {mcpToolCall.Name}");
                            toolApprovals.Add(new ToolApproval(mcpToolCall.Id, approve: true));
                        }
                    }

                    if (toolApprovals.Count > 0)
                    {
                        run = _persistentAgentsClient.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolApprovals: toolApprovals);
                    }
                }
            }

            // Get and stream response
            var messages = _persistentAgentsClient.Messages.GetMessages(threadId: thread.Id, order: ListSortOrder.Descending);
            var latestMessage = messages.FirstOrDefault();
            if (latestMessage != null && latestMessage.Role == MessageRole.Agent)
            {
                foreach (var contentItem in latestMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        turnContext?.StreamingResponse.QueueTextChunk(textItem.Text);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error processing with persistent agent: {ex.Message}");
            await turnContext.SendActivityAsync($"I encountered an error: {ex.Message}");
        }
    }

    private string GetAgentCacheKey(ITurnState turnState)
    {
        string userAgentKey = turnState.User.GetValue<string?>("user.agentKey", () => null) ?? "";
        if (string.IsNullOrEmpty(userAgentKey))
        {
            userAgentKey = Guid.NewGuid().ToString();
            turnState.User.SetValue("user.agentKey", userAgentKey);
        }
        return userAgentKey;
    }
}
