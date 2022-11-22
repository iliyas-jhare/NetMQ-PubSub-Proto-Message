namespace Common;

public static class Configuration
{
    public const string ConnectionStr = "tcp://localhost:12345";

    public const int HighWatermark = 1000;

    public const int MessagesCount = 20;

    public static TimeSpan MessageInterval => TimeSpan.FromMilliseconds(500);

    public static TimeSpan MessageTimeout => TimeSpan.FromMilliseconds(50);
}
