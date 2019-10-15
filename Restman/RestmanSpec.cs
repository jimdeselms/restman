using System.Collections.Generic;

namespace Restman
{
    class RestmanSpec
    {
        public Dictionary<string, Dictionary<string, string>> variableSets { get; set; }
        public Dictionary<string, RequestSpec> requests { get; set; }
    }

    class RequestSpec
    {
        public string method { get; set; }
        public string url { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public string bifoqlQuery { get; set; }
        public IReadOnlyList<string> ordinalArgs { get; set; }
        public string body { get; set; }
    }
}