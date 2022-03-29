﻿using StardewModdingAPI;

namespace SDVFactory
{
    internal class Logger
    {
        private IMonitor Monitor;

        public Logger(IMonitor monitor)
        {
            Monitor = monitor;
        }

        /// <summary>
        /// Logs tracing info intended for developers.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Trace(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Trace);
        /// <summary>
        /// Logs troubleshooting info which may be relevant to the player.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Debug(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Debug);
        /// <summary>
        /// Logs info which is relevant to the player.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Info(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Info);
        /// <summary>
        /// Logs an issue or problem the player should be aware of, when the game is likely to be continuable.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Warn(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Warn);
        /// <summary>
        /// Logs an issue or problem the player needs to be aware of, when the game is likely to become unstable or fail.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Error(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Error);
        /// <summary>
        /// Logs important, highlighted information the player needs to be aware of, and would likely want to act upon. Use sparingly.
        /// </summary>
        /// <param name="message">String parameters to be printed to console. Arrays or variable parameters are joined without separators.</param>
        public void Alert(params string[] message) => Monitor.Log(string.Join("", message), LogLevel.Alert);
    }
}
