using System;
using System.Collections.Generic;

namespace AssessmentAPI.DataLayer
{
    public partial class Files
    {
        public Files()
        {
            Attributes = new HashSet<Attributes>();
        }

        public int FileId { get; set; }
        public int? FileSize { get; set; }
        public string Mimetype { get; set; }
        public string Hash { get; set; }
        public string Links { get; set; }
        public string BatchId { get; set; }
        public string FileName { get; set; }

        public virtual ICollection<Attributes> Attributes { get; set; }
    }
}
