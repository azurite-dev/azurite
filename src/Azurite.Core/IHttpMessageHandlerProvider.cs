using System.Net.Http;

namespace Azurite
{
    public interface IHttpMessageHandlerProvider
    {
         HttpMessageHandler BuildHandler();
    }
}