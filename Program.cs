using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
#pragma warning disable SKEXP0050 
#pragma warning disable SKEXP0060

string yourDeploymentName = "gpt-35-turbo";
string yourEndpoint = "https://oasys-openai-dev.openai.azure.com/";
string yourApiKey = "8dd92f75fa4a4c79b58ab51eeae38927";

var builder = Kernel.CreateBuilder();
builder.Services.AddAzureOpenAIChatCompletion(
    yourDeploymentName,
    yourEndpoint,
    yourApiKey,
    "gpt-35-turbo-16k");
var kernel = builder.Build();

// Note: ChatHistory isn't working correctly as of SemanticKernel v 1.4.0
StringBuilder chatHistory = new();

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


var result = await kernel.InvokeAsync(prompts["GetTargetCurrencies"],
    new() {
        {"input", "How many Australian Dollars is 140,000 Korean Won worth?"}
    }
);

Console.WriteLine(result);
