using System;

namespace mid2chart
{
	/// <summary>
	/// custom stopwatch class.
	/// </summary>
	public static class Stopwatch {
		static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		static double totalTime;
		static string step;
		static bool running = false;
		static bool started = false;

		/// <summary>
		/// starts the stopwatch. i mean it doesn't actually start counting until you call Step(string step).
		/// </summary>
		/// <param name="name">session name</param>
		/// <param name="args">Console.WriteLine arguments</param>
		public static void Start(string name,params object[] args) {
			totalTime = 0;
			running = false;
			started = true;
			Console.WriteLine(name,args);
		}

        /// <summary>
        /// starts a step. stops previous ones if any.
        /// </summary>
        /// <param name="step">step name</param>
        /// <param name="args">Console.WriteLine arguments</param>
        public static void Step(string step,params object[] args) {
			EndStep();
			running = true;
            Stopwatch.step = string.Format(step,args);
            sw.Reset();
		}

		/// <summary>
		/// stops a step.
		/// </summary>
		public static void EndStep() {
			sw.Stop();
			if (running) {
				totalTime += sw.Elapsed.TotalSeconds;
				Console.WriteLine("> "+step+": "+sw.Elapsed.TotalSeconds.ToString("F8")+"s");
				running = false;
			}
		}

		/// <summary>
		/// stops the stopwatch. i mean it finishes the entire session, you can just stop a step by calling EndStep().
		/// </summary>
		public static void Stop() {
			EndStep();
			if (!started) return;
			started = false;
			Console.WriteLine("Done! Total time: "+totalTime.ToString("F8")+"s");
		}
	}
}
