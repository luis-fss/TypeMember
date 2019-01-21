using System;
using System.Diagnostics;
using NUnit.Framework;

namespace UnitTests.Util
{
    public class HiResTimerValidator
    {
        [Test]
        //[Ignore("no need to test it all the time.")]
        public void validate_high_resolution_timer()
        {
            const int iterations = 5;
            const int threadSleepTime = 1;

            // Call the object and methods to JIT before the test run
            HiResTimer.SetAutomaticOutPutType(OutPutType.Nothing);
            HiResTimer.Start(0);
            HiResTimer.Stop();
            HiResTimer.SetAutomaticOutPutType(OutPutType.Nothing);

            // Time the overall test duration
            var dtStartTime = DateTime.Now;

            // Use QueryPerfCounters to get the average time per iteration
            HiResTimer.Start(iterations);

            for (var i = 0; i < iterations; i++)
            {
                // Method to time
                System.Threading.Thread.Sleep(threadSleepTime);
            }

            HiResTimer.Stop();

            // Calculate time per iteration in nanoseconds
            var result = HiResTimer.Duration();

            // Show the average time per iteration results
            Debug.WriteLine("Iterations: {0}", iterations);
            Debug.WriteLine("Average time per iteration: ");
            Debug.WriteLine(result / 1000000000 + " seconds");
            Debug.WriteLine(result / 1000000 + " milliseconds");
            Debug.WriteLine(result + " nanoseconds");

            // Show the overall test duration results
            var dtEndTime = DateTime.Now;
            var duration = (dtEndTime - dtStartTime).TotalMilliseconds;
            Debug.WriteLine("\n");
            Debug.WriteLine("Duration of test run: ");
            Debug.WriteLine(duration / 1000 + " seconds");
            Debug.WriteLine(duration + " milliseconds");
        }
    }
}