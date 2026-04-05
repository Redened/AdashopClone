namespace Adashop.Common.Results;

public record Result<T>( int Status, T? Value, string? Message, List<string>? Errors )
{
    public bool IsSuccess => Status >= 200 && Status < 300;


    // Success
    public static Result<T> Success( int status, T value, string? message = null ) => new(status, value, message, default);
    // return Result<Order>.Success(200, order, "Order processed");     // OK - Custom message
    // return Result<User>.Success(200, user);                          // OK - Standard success
    // return Result<User>.Success(201, newUser);                       // Created - New resource
    // return Result<bool>.Success(204, true);                          // No Content - Delete success


    // Error
    public static Result<T> Error( int status, string message, List<string>? errors = null ) => new(status, default, message, errors);
    // return Result<User>.Error(400, "Invalid email format");                      // Bad Request
    // return Result<User>.Error(400, "Validation failed", validationErrors);       // Bad Request with errors
    // return Result<User>.Error(401, "Invalid credentials");                       // Unauthorized
    // return Result<User>.Error(403, "Admin access required");                     // Forbidden
    // return Result<User>.Error(404, "User not found");                            // Not Found
    // return Result<User>.Error(409, "Email already exists");                      // Conflict
    // return Result<User>.Error(500, "Database connection failed");                // Internal Server Error
}