using Amazon.Translate;
using Amazon.Translate.Model;

namespace SignalR.Lambda.Services;

public class AmazonTranslateTranslationService : ITranslationService
{
    private readonly AmazonTranslateClient translateClient;

    public AmazonTranslateTranslationService(AmazonTranslateClient translateClient)
    {
        this.translateClient = translateClient;
    }

    public async Task<string> Translate(string requestedByUser, string textToTranslate, string translateTo)
    {
        var translationResponse = await translateClient.TranslateTextAsync(new TranslateTextRequest
        {
            SourceLanguageCode = "en",
            TargetLanguageCode = translateTo.ToLower(),
            Text = textToTranslate
        });

        return translationResponse.TranslatedText;
    }
}