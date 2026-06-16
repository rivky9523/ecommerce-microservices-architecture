namespace ECommerce.Monolith.Api.Services;

/// <summary>
/// A tiny result wrapper so the service layer can report business outcomes
/// (success, not-found, validation failure) without throwing exceptions or
/// depending on ASP.NET types. Controllers translate this into HTTP results.
/// </summary>
public class ServiceResult<T>
{
    public bool Succeeded { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public ServiceErrorKind ErrorKind { get; private init; }

    public static ServiceResult<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static ServiceResult<T> NotFound(string error) =>
        new() { Succeeded = false, Error = error, ErrorKind = ServiceErrorKind.NotFound };

    public static ServiceResult<T> Validation(string error) =>
        new() { Succeeded = false, Error = error, ErrorKind = ServiceErrorKind.Validation };
}

public enum ServiceErrorKind
{
    None,
    NotFound,
    Validation
}
