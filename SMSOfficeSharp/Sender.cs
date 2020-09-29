using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace SMSOfficeSharp
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SMSOfficeException : Exception
    {
        protected SMSOfficeException(string message) : base(message)
        {
        }
    }

    internal class BadRequestException : SMSOfficeException
    {
        internal BadRequestException(string message)
            : base($"Invalid request: {message}")
        {
        }
    }

    internal class SubscriptionException : SMSOfficeException
    {
        internal SubscriptionException(string message)
            : base($"Subscription has problem: {message}")
        {
        }
    }

    internal class InternalServerException : SMSOfficeException
    {
        internal InternalServerException()
            : base("Server is temporarily unavailable")
        {
        }
    }

    internal class Response
    {
        [JsonProperty] public bool Success;
        [JsonProperty] public string Message;
        [JsonProperty] public int ErrorCode;
    }


    public class Sender
    {
        private readonly HttpClient _client = new HttpClient();
        private const string ApiEndpoint = "https://smsoffice.ge/api/v2/send/";
        public string MessageTitle { get; set; }
        public string ApiKey { get; set; }

        public void Send(string text, params string[] phoneNumbers)
        {
            var values = new Dictionary<string, string>
            {
                {"key", ApiKey},
                {"destination", string.Join(",", phoneNumbers)},
                {"sender", MessageTitle},
                {"content", text}
            };
            var content = new FormUrlEncodedContent(values);
            Response resp;
            try
            {
                var response = _client.PostAsync(ApiEndpoint, content).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new BadRequestException($"status code {response.StatusCode}");
                }

                resp = JsonConvert.DeserializeObject<Response>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                throw new BadRequestException(e.Message);
            }

            switch (resp.ErrorCode)
            {
                case 0:
                    return;
                case 10:
                    throw new BadRequestException("Foreign number");
                case 20:
                    throw new SubscriptionException("Not enough funds on balance");
                case 40:
                    throw new BadRequestException("Text message size limit exceeded");
                case 60:
                    throw new BadRequestException("Text message is empty");
                case 70:
                    throw new BadRequestException("No numbers to send message");
                case 80:
                    throw new SubscriptionException("API key isn't valid");
                case 110:
                    throw new BadRequestException("Invalid sender parameter");
                case 120:
                    throw new SubscriptionException("API permissions hasn't granted");
                case 500:
                    throw new BadRequestException("\"key\" parameter is missing");
                case 600:
                    throw new BadRequestException("\"destination\" parameter is missing");
                case 700:
                    throw new BadRequestException("\"sender\" parameter is missing");
                case 800:
                    throw new BadRequestException("\"content\" parameter is missing");
                case -100:
                    throw new InternalServerException();
            }
        }
    }
}