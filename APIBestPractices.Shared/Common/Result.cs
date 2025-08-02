namespace APIBestPractices.Shared.Common;

public class Result<T> : IResult
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public Error? Error { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Result(bool isSuccess, T? value, Error? error = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value);
    
    public static Result<T> ValidationFailure(Error errors) => 
        new(false, default, errors);
    
    public static Result<T> Failure(string errorMessage) => 
        new(false, default, errorMessage: errorMessage);
}

public class Result : IResult
{
    public bool IsSuccess { get; private set; }
    public Error? Error { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Result(bool isSuccess, Error? error = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true);
    
    public static Result ValidationFailure(Error errors) => 
        new(false, errors);
    
    public static Result Failure(string errorMessage) => 
        new(false, errorMessage: errorMessage);
}