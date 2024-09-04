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
    default:
        Console.WriteLine("Other intent detected");
        break;
}