using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Stopwatch class for keeping track of duration in the code
    /// </summary>
    public class InternalStopwatch
    {
        private string _subject;
        private DateTime _startTime;

        /// <summary>
        /// Constructor that initalizes the stopwatch
        /// </summary>
        public InternalStopwatch()
        {
            _subject = string.Empty;
        }

        /// <summary>
        /// Constructor that initalizes the stopwatch with a subject and starts immediately
        /// </summary>
        /// <param name="subject">Subject of the stopwatch</param>
        public InternalStopwatch(string subject)
        {
            _subject = subject;
            Start();
        }

        /// <summary>
        /// Start the stopwatch and display subject text
        /// </summary>
        public void Start()
        {
            _startTime = DateTime.Now;
            Globals.Logger.User($"{_subject}...");
        }

        /// <summary>
        /// Start the stopwatch and display subject text
        /// </summary>
        /// <param name="subject">Text to show on stopwatch start</param>
        public void Start(string subject)
        {
            _subject = subject;
            Start();
        }

        /// <summary>
        /// End the stopwatch and display subject text
        /// </summary>
        public void Stop()
        {
            Globals.Logger.User($"{_subject} completed in {DateTime.Now.Subtract(_startTime):G}");
        }
    }
}
