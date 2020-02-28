using System;
using System.Net;

namespace PfSenseFauxApi.Net.Exceptions
{
	/// <summary>
	/// The exception raised when an HTTP error occurs in an API request.
	/// </summary>
	public class ApiHttpException : ApiException
	{
		/// <summary>
		/// The status code returned by the request.
		/// </summary>
		public HttpStatusCode HttpStatusCode { get; }
		
		/// <summary>
		/// The response body (if any).
		/// </summary>
		public string ResponseBody { get; }
		
		/// <summary>
		/// Creates a new HTTP exception with no response body.
		/// </summary>
		/// <param name="code"></param>
		public ApiHttpException(HttpStatusCode code) : base($"The HTTP request failed with code {(int) code} ({code}).")
		{
			HttpStatusCode = code;
		}

		/// <summary>
		/// Creates a new HTTP exception with a response body.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="body"></param>
		public ApiHttpException(HttpStatusCode code, string body) : this(code)
		{
			ResponseBody = body;
		}

		/// <summary>
		/// Creates a new HTTP exception with no response body.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="innerException"></param>
		public ApiHttpException(HttpStatusCode code, Exception innerException) : base($"The HTTP request failed with code {(int) code} ({code}).", innerException)
		{
			HttpStatusCode = code;
		}

		/// <summary>
		/// Creates a new HTTP exception with a response body.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="body"></param>
		/// <param name="innerException"></param>
		public ApiHttpException(HttpStatusCode code, string body, Exception innerException) : this(code, innerException)
		{
			ResponseBody = body;
		}
		
	}
}
