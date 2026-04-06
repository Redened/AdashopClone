namespace Adashop.Shared.Results;

public record Result<T>( int Status, T? Value, string? Message, List<string>? Errors )
{
    public bool IsSuccess => Status >= 200 && Status < 300;

    public static Result<T> Success( int status, T value, string? message = null ) => new(status, value, message, default);

    public static Result<T> Error( int status, string message, List<string>? errors = null ) => new(status, default, message, errors);
}
