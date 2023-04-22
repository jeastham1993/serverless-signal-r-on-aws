using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("What is the connection URL?");
var connectionUrl = Console.ReadLine();

Console.WriteLine("Which user would you like to connect as?");
var username = Console.ReadLine();

var connection = new HubConnectionBuilder()
    .WithUrl($"{connectionUrl}/translationHub?username={username}")
    .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0,5) * 1000);
    await connection.StartAsync();
};

await connection.StartAsync();

connection.On<string>("ReceiveTranslationResponse", (translation) =>
{
    Console.WriteLine($"Translation is {translation}");
});

var message = "";

while (message != "exit")
{
    Console.WriteLine("Translate new message?");
    message = Console.ReadLine();

    Console.WriteLine("What language would you like that to be translated to? Please use the 2 digit ISO country code");
    var translateTo = Console.ReadLine();
    
    await connection.InvokeAsync("TranslateMessage", username,translateTo, message);
}