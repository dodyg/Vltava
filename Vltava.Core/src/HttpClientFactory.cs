using System.Net.Http;

namespace Vltava.Core
{
    public class HttpClientFactory
    {
        static readonly System.Net.Http.HttpClient _client = new HttpClient();

        public static HttpClient Get() => _client;
    }
}