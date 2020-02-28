using System.Text.Json;

namespace PfSenseFauxApi.Net.Exceptions
{
	/// <summary>
	/// The exception raised when the response doesn't fit the expected JSON format.
	/// </summary>
	public class ApiJsonException : ApiException
	{
		/// <summary>
		/// The response body.
		/// </summary>
		public string ResponseBody { get; }
		
		/// <summary>
		/// Creates a new JSON exception without the response body.
		/// </summary>
		/// <param name="innerException"></param>
		public ApiJsonException(JsonException innerException) : base("Failed to parse JSON response.", innerException)
		{
			
		}

		/// <summary>
		/// Creates a new JSON exception with the response body.
		/// </summary>
		/// <param name="body"></param>
		/// <param name="innerException"></param>
		public ApiJsonException(string body, JsonException innerException) : this(innerException)
		{
			ResponseBody = body;
		}
	}
}
