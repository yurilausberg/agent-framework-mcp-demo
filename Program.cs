using agent_framework_mcp_demo;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Configuration;

public class Program
{
  public static async Task Main()
  {
    Console.WriteLine("=== Pet Store Agent (Agent 365 SDK) ===\n");

    // Build configuration from appsettings.json
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Create and initialize the agent
    var agent = new PetStoreAgent(configuration);

    // Prompt user for new agent name
    Console.WriteLine("Please provide new agent name:");
    var agentName = Console.ReadLine();

    // Initialize the agent
    await agent.InitializeAsync(agentName ?? "PetStoreAgent");

    Console.WriteLine($"\nAgent '{agent.AgentName}' is ready!");
    Console.WriteLine($"Agent ID: {agent.AgentId}\n");

    // Interactive loop for multiple queries
    while (true)
    {
      Console.WriteLine("Enter your prompt (or 'exit' to quit): ");
      var userMessage = Console.ReadLine();

      if (string.IsNullOrEmpty(userMessage) || userMessage.ToLower() == "exit")
      {
        break;
      }

      // Process the message
      var messages = await agent.ProcessMessageAsync(userMessage);

      // Print the messages in the thread
      Console.WriteLine("\n--- Conversation ---");
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
      Console.WriteLine("--- End Conversation ---\n");
    }

    Console.WriteLine("Thank you for using Pet Store Agent!");
  }
}
