using System;

namespace TCCGame.Core.Time
{
    public interface IClock
    {
        long UnixSeconds { get; }
    }

    public sealed class SystemClock : IClock
    {
        public long UnixSeconds =>
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
