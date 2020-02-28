using System;

namespace PfSenseFauxApi.Net.Exceptions
{
	/// <summary>
	/// The base exception throw by the API library.
	/// </summary>
	public class ApiException : ApplicationException
	{
		/// <summary>
		/// Creates a new API exception.
		/// </summary>
		public ApiException() : base()
		{
			
		}

		/// <summary>
		/// Creates a new API exception with a message.
		/// </summary>
		/// <param name="message"></param>
		public ApiException(string message) : base(message)
		{
			
		}

		/// <summary>
		/// Creates a new API exception with a message and inner exception.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public ApiException(string message, Exception innerException) : base(message, innerException)
		{
			
		}
	}
}
