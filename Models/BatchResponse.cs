using System;
using System.Text.Json.Serialization;

namespace AssessmentAPI.Models
{
    public class BatchResponse
    {
        /// <summary>
        /// Batch ID
        /// </summary>
        [JsonPropertyName("batchID")]
        public Guid BatchId { get; set; }
    }
}
