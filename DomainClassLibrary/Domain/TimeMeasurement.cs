using System.Diagnostics;

namespace Lab1.Infrastructure
{
    public class TimeMeasurement
    {
        public static (double Milliseconds, T Result) Measure<T>(Func<T> action)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = action();
            stopwatch.Stop();
            return (stopwatch.Elapsed.TotalMilliseconds, result);
        }
    }
}
