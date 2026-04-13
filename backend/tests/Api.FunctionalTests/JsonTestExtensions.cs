using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codebasky.Api.FunctionalTests;

public static class JsonTestExtensions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<T?> ReadAsAsync<T>(this HttpClient client, string requestUri)
    {
        return client.GetFromJsonAsync<T>(requestUri, Options);
    }

    public static Task<T?> ReadAsAsync<T>(this HttpContent content)
    {
        return content.ReadFromJsonAsync<T>(Options);
    }
}
