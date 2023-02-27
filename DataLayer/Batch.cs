using System;
using System.Collections.Generic;

namespace AssessmentAPI.DataLayer
{
    public partial class Batch
    {
        public Batch()
        {
            Attributes = new HashSet<Attributes>();
        }

        public string BatchId { get; set; }
        public string Status { get; set; }
        public int? BusinessUnitId { get; set; }
        public DateTime? BatchPublishedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ReadUsers { get; set; }
        public string ReadGroups { get; set; }

        public virtual ICollection<Attributes> Attributes { get; set; }
    }
}
