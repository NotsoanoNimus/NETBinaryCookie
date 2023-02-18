namespace NETBinaryCookie.Types;

public class BinaryCookieException : Exception
{
    public BinaryCookieException()
    {
    }

    public BinaryCookieException(string message) : base(message)
    {
    }

    public BinaryCookieException(string message, Exception inner)
        : base(message, inner)
    {
    }
}