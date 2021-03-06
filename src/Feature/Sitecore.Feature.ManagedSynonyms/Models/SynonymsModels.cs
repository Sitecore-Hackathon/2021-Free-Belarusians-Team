using System;
using System.Collections.Generic;

namespace Sitecore.Feature.ManagedSynonyms.Models
{
    public class ResponseHeader
    {
        public int Status { get; set; }
        public int QTime { get; set; }
    }

    public class InitArgs
    {
        public bool IgnoreCase { get; set; }
    }

    public class SynonymMappings
    {
        public InitArgs InitArgs { get; set; }
        public DateTime InitializedOn { get; set; }
        public DateTime UpdatedSinceInit { get; set; }
        public IDictionary<string, IEnumerable<string>> ManagedMap { get; set; }
    }

    public class SynonymResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public SynonymMappings SynonymMappings { get; set; }
    }
}