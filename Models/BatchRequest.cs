using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssessmentAPI.Models
{
    public class BatchRequest
    {
        /// <summary>
        /// Business Unit Name
        /// </summary>
        [JsonPropertyName("businessUnit")]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Acl
        /// </summary>
        [JsonPropertyName("acl")]
        public Acl Acl1 { get; set; }

        /// <summary>
        /// Attritube Array
        /// </summary>
        [JsonPropertyName("attritubes")]
        public Attritube[] Attritubes { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        [JsonPropertyName("expiryDate")]
        public DateTime ExpiryDate { get; set; }


        public class Acl
        {
            /// <summary>
            /// Read users
            /// </summary>
            [JsonPropertyName("readUsers")]
            public string[] ReadUsers { get; set; }

            /// <summary>
            /// Read groups
            /// </summary>
            [JsonPropertyName("readGroups")]
            public string[] ReadGroups { get; set; }
        }

        public class Attritube
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

    }
}
