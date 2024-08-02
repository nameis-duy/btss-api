namespace API.GraphQL.Extends
{
    public class ExceptionFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception != null) return ErrorBuilder.New().SetMessage(error.Exception.Message).Build();
            return error;
        }
    }
}
