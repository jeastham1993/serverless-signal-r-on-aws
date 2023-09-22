using Amazon;
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

        if (chain.TryGetAWSCredentials("dev", out awsCredentials))
        {
            sqsClient = new AmazonSQSClient(awsCredentials, regionEndpoint);
            translateClient = new AmazonTranslateClient(awsCredentials, regionEndpoint);
        }

        services.AddSingleton(sqsClient);
        services.AddSingleton(translateClient);

        return services;
    }
}