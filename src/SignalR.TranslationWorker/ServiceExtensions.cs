using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.Translate;

namespace SignalR.TranslationWorker;

public static class ServiceExtensions
{
    public static IServiceCollection SetupAwsSdks(this IServiceCollection services)
    {
        // Setup AWS Credentials and SDK
        var chain = new CredentialProfileStoreChain();
        AWSCredentials? awsCredentials;
        var regionEndpoint = RegionEndpoint.EUWest1;

        AmazonSQSClient sqsClient = new AmazonSQSClient(regionEndpoint);
        AmazonTranslateClient translateClient = new AmazonTranslateClient(regionEndpoint);
        AmazonBedrockRuntimeClient bedrockRuntimeClient = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

        if (chain.TryGetAWSCredentials("dev", out awsCredentials))
        {
            sqsClient = new AmazonSQSClient(awsCredentials, regionEndpoint);
            translateClient = new AmazonTranslateClient(awsCredentials, regionEndpoint);
            bedrockRuntimeClient = new AmazonBedrockRuntimeClient(awsCredentials, RegionEndpoint.USEast1);
        }

        services.AddSingleton(sqsClient);
        services.AddSingleton(translateClient);
        services.AddSingleton(bedrockRuntimeClient);

        return services;
    }
}