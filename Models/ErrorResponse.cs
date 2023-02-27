using System.Text.Json.Serialization;

namespace AssessmentAPI.Models
{
    public class ErrorResponse
    {
        /// <summary>
        /// Correlation ID
        /// </summary>
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Errors Array
        /// </summary>
        [JsonPropertyName("errors")]
        public Error[] Errors { get; set; }

        public class Error
        {
            /// <summary>
            /// Source of the error
            /// </summary>
            [JsonPropertyName("source")]
            public string Source { get; set; }

            /// <summary>
            /// Description of the error
            /// </summary>
            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

    }
}
