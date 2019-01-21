using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UnitTests.Util
{
    public static class HiResTimer
    {
        private static long _start;
        private static long _stop;
        private static long _frequency;
        static readonly Decimal Multiplier = new Decimal(1.0e9);
        private static OutPutType _outPutType;
        private static DateTime _dtStartTime;
        private static DateTime _dtEndTime;
        private static bool _isNew;
        private static int _iterations;

        private static void CleanUp()
        {
            var outPutType = _outPutType;
            _outPutType = OutPutType.Nothing;
            Stop();
            _outPutType = outPutType;
        }

        static HiResTimer()
        {
            // Call the methods to JIT before any test run
            _outPutType = OutPutType.Nothing;
            _isNew = false;
            Start(0);
            Stop();
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public static long Ticks
        {
            get
            {
                long t;
                QueryPerformanceCounter(out t);
                return t;
            }
        }

        public static long TicksPerSecond
        {
            get
            {
                long freq;
                QueryPerformanceFrequency(out freq);
                return freq;
            }
        }

        public static int Start(int iterations)
        {
            if (QueryPerformanceFrequency(out _frequency) == false)
            {
                // Frequency not supported
                throw new Win32Exception();
            }

            //CleanUp();

            if (_isNew)
                _outPutType = OutPutType.Verbose;

            _iterations = iterations;

            _dtStartTime = DateTime.Now;

            QueryPerformanceCounter(out _start);

            return iterations;
        }

        public static void Stop()
        {
            QueryPerformanceCounter(out _stop);

            _dtEndTime = DateTime.Now;

            _isNew = true;

            BuildOutPut(Duration(), _iterations, _outPutType);
        }

        public static double Duration()
        {
            return ((((_stop - _start) * (double)Multiplier) / _frequency) / _iterations);
        }

        public static void SetAutomaticOutPutType(OutPutType outPutType = OutPutType.Verbose)
        {
            _isNew = false;
            _outPutType = outPutType;
        }

        private static void BuildOutPut(double result, int iterations, OutPutType outPutType)
        {
            if (outPutType == OutPutType.Nothing) return;

            // Show the average time per iteration results
            Debug.WriteLine("##############################");
            Debug.WriteLine("Iterations: {0}", iterations);
            Debug.WriteLine("Average time per iteration: ");
            Debug.WriteLine("\t{0} seconds", result / 1000000000);
            Debug.WriteLine("\t{0} milliseconds", result / 1000000);
            Debug.WriteLine("\t{0} nanoseconds", result);

            if (outPutType != OutPutType.Verbose) return;

            // Show the overall test duration results
            var duration = (_dtEndTime - _dtStartTime).TotalMilliseconds;
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Duration of test run: ");
            Debug.WriteLine("\t{0} seconds", duration / 1000);
            Debug.WriteLine("\t{0} milliseconds\n", duration);
        }
    }

    public enum OutPutType
    {
        Nothing,
        Minimal,
        Verbose
    }
}