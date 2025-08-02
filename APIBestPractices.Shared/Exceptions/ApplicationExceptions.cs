namespace APIBestPractices.Shared.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }

    protected ApplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }

    public ValidationException(string property, string error) : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { property, new[] { error } }
        };
    }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message)
    {
    }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}