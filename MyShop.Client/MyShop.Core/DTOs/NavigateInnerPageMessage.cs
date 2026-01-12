namespace MyShop.Core.DTOs
{
    public class NavigateInnerPageMessage
    {
        public string Key { get; } 
        public object? Parameter { get; }

        public NavigateInnerPageMessage(string key, object? parameter = null)
        {
            Key = key;
            Parameter = parameter;
        }
    }
}
