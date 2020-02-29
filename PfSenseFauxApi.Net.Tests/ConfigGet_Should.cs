namespace PfSenseFauxApi.Net.Tests
{
    public class ConfigGet_Should
    {
        private readonly Device _dev;

        public ConfigGet_Should()
        {
            _dev = TestConfig.Instance.GetDevice();
        }
        
        
    }
}
