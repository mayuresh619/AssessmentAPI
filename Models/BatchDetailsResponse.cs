using System;
using System.Text.Json.Serialization;

namespace AssessmentAPI.Models
{
    public class BatchDetailsResponse
    {
        /// <summary>
        /// Batch Id
        /// </summary>
        [JsonPropertyName("batchId")]
        public string BatchId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Attribute Array
        /// </summary>
        [JsonPropertyName("attritube")]
        public Attritube1[] Attritube { get; set; }
        
        /// <summary>
        /// Business Unit Name
        /// </summary>
        [JsonPropertyName("businessUnit")]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Publish date
        /// </summary>
        [JsonPropertyName("batchPublishedDate")]
        public DateTime BatchPublishedDate { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        [JsonPropertyName("expiryDate")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Files array
        /// </summary>
        [JsonPropertyName("files")]
        public File[] Files { get; set; }


        public class Attritube1
        {
            /// <summary>
            /// Key
            /// </summary>
            [JsonPropertyName("key")]
            public string Key { get; set; }

            /// <summary>
            /// Value
            /// </summary>
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        public class File
        {
            /// <summary>
            /// File Name
            /// </summary>
            [JsonPropertyName("filename")]
            public string FileName { get; set; }

            /// <summary>
            /// File Size
            /// </summary>
            [JsonPropertyName("fileSize")]
            public int FileSize { get; set; }

            /// <summary>
            /// MIME Type
            /// </summary>
            [JsonPropertyName("mimeType")]
            public string MimeType { get; set; }

            /// <summary>
            /// Hash
            /// </summary>
            [JsonPropertyName("hash")]
            public string Hash { get; set; }

            /// <summary>
            /// Attribute Array
            /// </summary>
            [JsonPropertyName("attritube")]
            public Attritube1[] Attritube { get; set; }

            /// <summary>
            /// Links
            /// </summary>
            [JsonPropertyName("links")]
            public string Links { get; set; }
        }

    }
}
