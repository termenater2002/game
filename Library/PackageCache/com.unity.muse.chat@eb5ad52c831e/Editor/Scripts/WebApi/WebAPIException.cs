using System;

namespace Unity.Muse.Chat.WebApi
{
    class WebApiException : Exception
    {
        public object ApiData { get; }

        public WebApiException(string message, object data) : base(message) => ApiData = data;
    }
}
