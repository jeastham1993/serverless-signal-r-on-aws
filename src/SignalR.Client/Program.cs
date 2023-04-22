using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("What is the connection URL?");
var connectionUrl = Console.ReadLine();

var connection = new HubConnectionBuilder()
    .WithUrl($"{connectionUrl}/chatHub")
    .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0,5) * 1000);
    await connection.StartAsync();
};

await connection.StartAsync();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"[{user}]: {message}");
});

Console.WriteLine("What is your name?");
var name = Console.ReadLine();

var message = "";

while (message != "exit")
{
    Console.WriteLine("Send new message.......");
    message = Console.ReadLine();
    
    await connection.InvokeAsync("SendMessage", name, message);
}