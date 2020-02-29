namespace PfSenseFauxApi.Net.Exceptions
{
    public class ApiMissingActionException : ApiException
    {
        public ApiMissingActionException(string action)
            : base($"The '{action}' action is missing from the host.")
        {
            
        }
    }
}
