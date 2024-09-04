using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
#pragma warning disable SKEXP0050 
#pragma warning disable SKEXP0060

string yourDeploymentName = "gpt-35-turbo-16k";
string yourEndpoint = "https://oasys-openai-dev-beta.openai.azure.com/";
string yourApiKey = "e3768a2badd446158f7129fdf536666c";

var builder = Kernel.CreateBuilder();
builder.Services.AddAzureOpenAIChatCompletion(
    yourDeploymentName,
    yourEndpoint,
    yourApiKey,
    "gpt-35-turbo-16k");
var kernel = builder.Build();

// Note: ChatHistory isn't working correctly as of SemanticKernel v 1.4.0
// StringBuilder chatHistory = new();

kernel.ImportPluginFromType<CurrencyConverter>();
kernel.ImportPluginFromType<ConversationSummaryPlugin>();
var prompts = kernel.ImportPluginFromPromptDirectory("Prompts");

// var result = await kernel.InvokeAsync("CurrencyConverter", 
//     "ConvertAmount", 
//     new() {
//         {"targetCurrencyCode", "USD"}, 
//         {"amount", "52000"}, 
//         {"baseCurrencyCode", "VND"}
//     }
// );

// Console.WriteLine(result);


// var result = await kernel.InvokeAsync(prompts["GetTargetCurrencies"],
//     new() {
//         {"input", "How many Australian Dollars is 140,000 Korean Won worth?"}
//     }
// );

// Console.WriteLine(result);


Console.WriteLine("What would you like to do?");
var input = Console.ReadLine();

var intent = await kernel.InvokeAsync<string>(
    prompts["GetIntent"], 
    new() {{ "input",  input }}
);

Console.WriteLine(intent);

// set automatic function calling
OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

//use switch case to  to choose plugin
switch (intent) {
    case "ConvertCurrency": 
        var currencyText = await kernel.InvokeAsync<string>(
            prompts["GetTargetCurrencies"], 
            new() {{ "input",  input }}
        );
        var currencyInfo = currencyText!.Split("|");
        var result = await kernel.InvokeAsync("CurrencyConverter", 
            "ConvertAmount", 
            new() {
                {"targetCurrencyCode", currencyInfo[0]}, 
                {"baseCurrencyCode", currencyInfo[1]},
                {"amount", currencyInfo[2]}, 
            }
        );
        Console.WriteLine(result);
        break;
    case "SuggestDestinations":
    case "SuggestActivities":
    case "HelpfulPhrases":
    case "Translate":
        var autoInvokeResult = await kernel.InvokePromptAsync(input!, new(settings));
        Console.WriteLine(autoInvokeResult);
        break;
    default:
        Console.WriteLine("Sure, I can help with that.");
        var otherIntentResult = await kernel.InvokePromptAsync(input!, new(settings));
        Console.WriteLine(otherIntentResult);
        break;

}