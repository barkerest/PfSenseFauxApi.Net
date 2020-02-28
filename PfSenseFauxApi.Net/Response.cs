using System.Text.Json.Serialization;
using PfSenseFauxApi.Net.Exceptions;

namespace PfSenseFauxApi.Net
{
    public class Response : IResponse
    {
        /// <summary>
        /// The ID of the call that can be tracked in log files.
        /// </summary>
        [JsonPropertyName("callid")]
        public string CallID { get; set; }
        
        /// <summary>
        /// The action performed.
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }
        
        /// <summary>
        /// Message returned for the action (status of the call).
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
    
    internal class Response<T> : Response
    {
        /// <summary>
        /// The data returned with the response.
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
    
    internal static class ResponseHelper
    {
        internal static void TestOk(this IResponse response)
        {
            if (!string.Equals("ok", response.Message))
            {
                throw new ApiMessageException(response.Message);
            }
        }
    }
}
