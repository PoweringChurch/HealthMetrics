public class ValidationError
{
    public string Message { get; set; }
    public string? Field { get; set; }
    public int StatusCode { get; set; }

    public ValidationError(string message, string? field = null, int statusCode = 400)
    {
        Message = message;
        Field = field;
        StatusCode = statusCode;
    }
}