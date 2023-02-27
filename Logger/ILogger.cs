using System;
using System.Collections.Generic;

namespace AssessmentAPI.Logger
{
    public interface ILogger
    {
        /// <summary>
        /// Log information severity to Application Insights
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        void LogInfo(string message,params (string, string)[] properties);

        /// <summary>
        /// Log error severity to Application Insights
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        void LogError(Exception ex, string message, params (string, string)[] properties);

        /// <summary>
        /// Log warning severity to Application Insights
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        string LogWarning(string message, params (string, string)[] properties);

    }
}
