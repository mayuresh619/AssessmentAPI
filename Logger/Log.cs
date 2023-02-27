using AssessmentAPI.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssessmentAPI.Logger
{
    public class Log : ILogger
    {
        #region Private Variables
        TelemetryClient _client;
        IConfiguration _config;
        string _correlationId;
        Dictionary<string, string> _properties;
        #endregion

        #region Constructor
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="configuration">Configuration Service</param>
        public Log(IConfiguration configuration)
        {
            _config = configuration;
            _client = new TelemetryClient();
            _client.InstrumentationKey = _config.GetSection(BatchConstants.APPLICATIONINSIGHTS).GetValue<string>(BatchConstants.INSTRUMENTATION_KEY);
            _properties = new Dictionary<string, string>();
            _correlationId = Guid.NewGuid().ToString();
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Log information severity to Application Insights
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        public void LogInfo(string message, params (string, string)[] properties)
        {
            if(!_config.GetValue<bool>(BatchConstants.APPLICATIONINSIGHTS_TRACE_INFO))
            {
                return;
            }

            clearDictionaryProperties();
            foreach ((string, string) item in properties)
            {
                _properties.Add(item.Item1, item.Item2);
            }
            _client.TrackTrace(message, SeverityLevel.Information, _properties);

        }

        /// <summary>
        /// Log error severity to Application Insights
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        public void LogError(Exception ex, string message, params (string, string)[] properties)
        {
            if (!_config.GetValue<bool>(BatchConstants.APPLICATIONINSIGHTS_TRACE_ERROR))
            {
                return;
            }

            clearDictionaryProperties();

            if (ex != null)
            {
                _properties.Add("Error Message", ex.Message);
                _properties.Add("Stack Trace", ex.StackTrace);
                _properties.Add("Source", ex.Source);
                _properties.Add("Help Link", ex.HelpLink);
            }

            foreach ((string, string) item in properties)
            {
                _properties.Add(item.Item1, item.Item2);
            }
            _client.TrackTrace(message, SeverityLevel.Error, _properties);

        }

        /// <summary>
        /// Log warning severity to Application Insights
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="properties">Custom properties</param>
        /// <returns>Correlation Id</returns>
        public string LogWarning(string message, params (string, string)[] properties)
        {
            if (!_config.GetValue<bool>(BatchConstants.APPLICATIONINSIGHTS_TRACE_WARNING))
            {
                return _correlationId;
            }

            clearDictionaryProperties();

            foreach ((string, string) item in properties)
            {
                _properties.Add(item.Item1, item.Item2);
            }
            _client.TrackTrace(message, SeverityLevel.Warning, _properties);
            return _correlationId;

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// To clear Dictionary properties on every call
        /// </summary>
        private void clearDictionaryProperties()
        {
            _properties.Clear();
            _properties.Add("Correlation ID", _correlationId);
            _properties.Add("Application Name", BatchConstants.APPLICATION_NAME);
        } 
        #endregion

    }
}
