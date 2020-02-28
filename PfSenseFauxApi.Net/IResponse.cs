namespace PfSenseFauxApi.Net
{
    public interface IResponse
    {
        string CallID { get; }
        
        string Action { get; }
        
        string Message { get; }
        
    }
}
