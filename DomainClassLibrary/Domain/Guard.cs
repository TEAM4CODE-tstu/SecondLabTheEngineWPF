public static class Guard
{
    public static void Requires(bool condition, string message)
    {
        if (!condition)
            throw new ArgumentException(message);
    }
}