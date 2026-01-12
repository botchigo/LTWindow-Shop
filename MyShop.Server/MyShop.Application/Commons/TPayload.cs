namespace MyShop.Application.Commons
{
    public class TPayload<T>
    {
        public T? Data { get; set; }
        public List<UserError>? Errors { get; }

        public TPayload(T data)
        {
            Data = data;
        }

        public TPayload(string error)
        {
            Errors = new List<UserError> { new UserError(error, "INTERNAL_ERROR") };
        }
    }

    public record UserError(string Message, string Code);
}
