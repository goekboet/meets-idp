namespace Ids
{
    public abstract class Result<T> { }
    public sealed class Ok<T> : Result<T>
    {
        public Ok(T value)
        {
            Value = value;
        }
        public T Value { get; }
    }

    public sealed class Unit { }

    public sealed class Error<T> : Result<T>
    {
        public Error(
            string desc
        )
        {
            Description = desc;
        }

        public string Description { get; }
    }
}