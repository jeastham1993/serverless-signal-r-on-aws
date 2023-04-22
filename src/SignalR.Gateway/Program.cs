using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using SignalR.Sample.Chat;

var builder = WebApplication.CreateBuilder(args);

var chain = new CredentialProfileStoreChain();
AWSCredentials? awsCredentials;
var regionEndpoint = RegionEndpoint.USEast1;

AmazonSQSClient sqsClient = new AmazonSQSClient(regionEndpoint);

if (chain.TryGetAWSCredentials("dev", out awsCredentials))
{
    sqsClient = new AmazonSQSClient(awsCredentials, regionEndpoint);
}

builder.Services.AddSingleton(sqsClient);

builder.Services.AddControllers();

var hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "localhost";
var portNumber = 6379;
var password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? "eYVX7EwVmmxKPCDmwMtyKVge8oLd2t81";

var connectionString = $"{hostName}:{portNumber}";

if (!string.IsNullOrEmpty(password))
{
    connectionString = $"{connectionString},password={password}";
}

builder.Services.AddSignalR()
    .AddStackExchangeRedis(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapGet(
    "/health",
    () => Results.Ok());

app.MapControllers();

app.MapHub<TranslationHub>("/chatHub");

app.Run();
