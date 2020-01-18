using Newtonsoft.Json;

namespace ScheduleArchiveAdapter
{
    public class SchType
    {
        [JsonProperty("t")]
        public string Label { get; set; }
        [JsonProperty("v")]
        public string Val { get; set; }
    }
}