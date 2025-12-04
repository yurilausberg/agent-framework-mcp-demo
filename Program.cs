using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;

public class Program
{
  public static async Task Main()
  {
    // Build configuration from appsettings.json
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Initialize the AI project client using Azure CLI credentials
    var projectClient = new Azure.AI.Projects.AIProjectClient(
      new Uri(configuration["FoundryProject:URL"] ?? throw new Exception("Missing FoundryProject:URL in appsettings.json")),
      new AzureCliCredential());
    var persistentAgentsClient = projectClient.GetPersistentAgentsClient();

    // Prompt user for new agent name
    Console.WriteLine("Please provide new agent name:");
    var agentName = Console.ReadLine();

    PersistentAgent agentMetadata;

    // Create a new agent
    agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
      model: configuration["FoundryProject:ModelName"] ?? throw new Exception("Missing FoundryProject:Model in appsettings.json"),
      instructions: "You are a petstore owner",
      name: agentName,
      tools: [
        new CodeInterpreterToolDefinition(),
        new MCPToolDefinition(
          configuration["MCPToolDefinition:Name"] ?? throw new Exception("Missing MCPToolDefinition:Name in appsettings.json"),
          configuration["MCPToolDefinition:URL"] ?? throw new Exception("Missing MCPToolDefinition:URL in appsettings.json"))
      ]
    );

    // Get the created agent
    AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Id);
    Console.WriteLine(agent.DisplayName);

    // Create a new thread
    PersistentAgentThread thread = persistentAgentsClient.Threads.CreateThread();

    Console.WriteLine("Enter your prompt: ");
    var userMessage = Console.ReadLine();

    // Create message to thread
    PersistentThreadMessage message = persistentAgentsClient.Messages.CreateMessage(
        thread.Id,
        MessageRole.User,
        userMessage);

    // Create run
    ThreadRun run = persistentAgentsClient.Runs.CreateRun(thread: thread, agent: agentMetadata, cancellationToken: default);

    // Wait for the run to complete
    while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
    {
      Thread.Sleep(TimeSpan.FromMilliseconds(1000));
      run = persistentAgentsClient.Runs.GetRun(thread.Id, run.Id);

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
          run = persistentAgentsClient.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolApprovals: toolApprovals);
        }
      }
    }

    // Print the messages in the thread
    Azure.Pageable<PersistentThreadMessage> messages = persistentAgentsClient.Messages.GetMessages(
        threadId: thread.Id,
        order: ListSortOrder.Ascending
    );

    foreach (PersistentThreadMessage threadMessage in messages)
    {
      Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
      foreach (MessageContent contentItem in threadMessage.ContentItems)
      {
        if (contentItem is MessageTextContent textItem)
        {
          Console.Write(textItem.Text);
        }
        else if (contentItem is MessageImageFileContent imageFileItem)
        {
          Console.Write($"<image from ID: {imageFileItem.FileId}>");
        }
        Console.WriteLine();
      }
    }

  }
}
