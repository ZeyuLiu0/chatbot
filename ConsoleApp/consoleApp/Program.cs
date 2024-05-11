// Create the kernel
using consoleApp;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Plugins.Core;


var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-3.5-turbo", "sk-proj-dn0qNBO7d40ZBwCKjgABT3BlbkFJweye6FXQzcG3jGwvzRmQ");
var path = Path.Combine(Directory.GetCurrentDirectory(), "Prompts");
// builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
// #pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
// builder.Plugins.AddFromType<ConversationSummaryPlugin>();
// #pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Plugins.AddFromType<MathPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
// builder.Plugins.AddFromType<AuthorEmailPlanner>();
// builder.Plugins.AddFromType<EmailPlugin>();
Kernel kernel = builder.Build();
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


ChatHistory chatMessages = new ChatHistory("""
You are a friendly assistant who likes to follow the rules. You will complete required steps
and request approval before taking any consequential actions. If the user doesn't provide
enough information for you to complete a task, you will keep asking questions until you have
enough information to complete the task.
""");

ChatHistory history = [];
Console.Write("User > ");
string? userInput;
while ((userInput = Console.ReadLine()) != null)
{
        history.AddUserMessage(userInput);

        // Enable auto function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Get the response from the AI
        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                            history,
                            executionSettings: openAIPromptExecutionSettings,
                            kernel: kernel);

        // Stream the results
        string fullMessage = "";
        var first = true;
        await foreach (var content in result)
        {
                if (content.Role.HasValue && first)
                {
                        Console.Write("Assistant > ");
                        first = false;
                }
                Console.Write(content.Content);
                fullMessage += content.Content;
        }
        Console.WriteLine();

        // Add the message from the agent to the chat history
        history.AddAssistantMessage(fullMessage);

        // Get user input again
        Console.Write("User > ");
}

// var prompts = kernel.CreatePluginFromPromptDirectory(path);
// List<string> choices = ["ContinueConversation", "EndConversation"];
// List<ChatHistory> fewShotExamples =
// [
//     [
//         new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
//         new ChatMessageContent(AuthorRole.System, "Intent:"),
//         new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation")
//     ],
//     [
//         new ChatMessageContent(AuthorRole.User, "Can you send the full update to the marketing team?"),
//         new ChatMessageContent(AuthorRole.System, "Intent:"),
//         new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
//     ]
// ];

// var getIntent = kernel.CreateFunctionFromPrompt(
//     new()
//     {
//             Template = """
//                    <message role="system">Instructions: What is the intent of this request?
//                    Do not explain the reasoning, just reply back with the intent. If you are unsure, reply with {{choices[0]}}.
//                    Choices: {{choices}}.</message>

//                    {{#each fewShotExamples}}
//                        {{#each this}}
//                            <message role="{{role}}">{{content}}</message>
//                        {{/each}}
//                    {{/each}}

//                    {{#each chatHistory}}
//                        <message role="{{role}}">{{content}}</message>
//                    {{/each}}

//                    <message role="user">{{request}}</message>
//                    <message role="system">Intent:</message>
//                    """,
//             TemplateFormat = "handlebars"
//     },
//     new HandlebarsPromptTemplateFactory()
// );


// while (true)
// {
//         // Get user input
//         System.Console.Write("User > ");
//         chatMessages.AddUserMessage(Console.ReadLine()!);

//         // Get the chat completions
//         OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
//         {
//                 ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
//         };
//         var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
//             chatMessages,
//             executionSettings: openAIPromptExecutionSettings,
//             kernel: kernel);

//         // Stream the results
//         string fullMessage = "";
//         await foreach (var content in result)
//         {
//                 if (content.Role.HasValue)
//                 {
//                         System.Console.Write("Assistant > ");
//                 }
//                 System.Console.Write(content.Content);
//                 fullMessage += content.Content;
//         }
//         System.Console.WriteLine();

//         // Add the message from the agent to the chat history
//         chatMessages.AddAssistantMessage(fullMessage);
// }


// Console.Write("User > ");
// string? userInput;
// // Start the chat loop
// while ((userInput = Console.ReadLine()) != null)
// {

//         // Invoke prompt
//         var intent = await kernel.InvokeAsync(
//         getIntent,
//         new()
//         {
//             { "request", userInput },
//             { "choices", choices },
//             { "history", history },
//             { "fewShotExamples", fewShotExamples }
//         }
//     );

//         // End the chat if the intent is "Stop"
//         if (intent.ToString() == "EndConversation")
//         {
//                 break;
//         }

//         // Get chat response
//         var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
//         prompts["chat"],
//         new()
//         {
//             { "request", userInput },
//             { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
//         }
//     );

//         // Stream the response
//         string message = "";
//         await foreach (var chunk in chatResult)
//         {
//                 if (chunk.Role.HasValue)
//                 {
//                         Console.Write(chunk.Role + " > ");
//                 }

//                 message += chunk;
//                 Console.Write(chunk);
//         }
//         Console.WriteLine();

//         // Append to history
//         history.AddUserMessage(userInput);
//         history.AddAssistantMessage(message);
//         Console.Write("User > ");
// }