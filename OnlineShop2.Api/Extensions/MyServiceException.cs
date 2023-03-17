namespace OnlineShop2.Api.Extensions
{
    [Serializable]
    public class MyServiceException:Exception
    {
        public MyServiceException() { }
        public MyServiceException(string message) : base(message) { }
        public MyServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
