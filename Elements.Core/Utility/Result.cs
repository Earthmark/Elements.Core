using System;
#nullable enable
namespace Elements.Core
{
    // Based on: https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern
    // And: https://github.com/altmann/FluentResults
    // TODO: Should we just use FluentResults?
    /// <summary>
    /// A generic Result object, that can represent the Result of an operation with an optional message
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public LocaleString? Message { get; }

        public Exception? Exception;

        protected Result(bool isSuccess, LocaleString? error , Exception? ex = null)
        {
            IsSuccess = isSuccess;
            Exception = ex;
            Message = error;
        }
        protected Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
            Exception = null;
            Message = null;
        }

        public static Result Success() => new Result(true);
        public static Result Failure(Exception ex) => new Result(false, null, ex);
        public static Result Failure(LocaleString error) => new Result(false, error);

    }

    /// <summary>
    /// Like <see cref="Result"/> but contains a <typeparamref name="T"/> as a value for the result
    /// </summary>
    /// <typeparam name="T">The type of value that the result can contain</typeparam>
    public class Result<T> : Result where T : class
    {
        public T? Value { get; }

        public Result(T? result, bool success, Exception? ex = null, LocaleString? error = null) : base(success, error, ex)
        {
            Value = result;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true);
        public static Result<T> Failure(Exception ex) => new Result<T>(null, false, ex);
        public static Result<T> Failure(LocaleString error) => new Result<T>(null, false, null, error);
    }
}
