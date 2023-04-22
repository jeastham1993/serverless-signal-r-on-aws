using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.Translate;
using Amazon.Translate.Model;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TranslationProcessor
{
    public class Function
    {
        private readonly AmazonTranslateClient _translateClient;
        private readonly AmazonSQSClient _sqsClient;
        
        
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            this._translateClient = new AmazonTranslateClient();
            this._sqsClient = new AmazonSQSClient();
        }
        
        public async Task FunctionHandler(SQSEvent sqsEvent)
        {
            foreach (var message in sqsEvent.Records)
            {
                var translationRequest = JsonSerializer.Deserialize<TranslateMessageCommand>(message.Body);

                var translationResponse = await this._translateClient.TranslateTextAsync(new TranslateTextRequest
                {
                    SourceLanguageCode = "en",
                    TargetLanguageCode = translationRequest.TranslateTo.ToLower(),
                    Text = translationRequest.Message
                });

                await this._sqsClient.SendMessageAsync(
                    Environment.GetEnvironmentVariable("QUEUE_URL"), JsonSerializer.Serialize(new TranslateMessageResponse()
                    {
                        Translation = translationResponse.TranslatedText,
                        ConnectionId = translationRequest.ConnectionId,
                        Username = translationRequest.Username
                    }));
            }
        }
    }
}
