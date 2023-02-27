using System;
using System.Collections.Generic;

namespace AssessmentAPI.DataLayer
{
    public partial class Attributes
    {
        public int AttributeId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string BatchId { get; set; }
        public int? FileId { get; set; }

        public virtual Batch Batch { get; set; }
        public virtual Files File { get; set; }
    }
}
