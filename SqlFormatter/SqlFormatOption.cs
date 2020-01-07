using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlFormatter
{
    [DebuggerDisplay("{n}:{t}")]
    public class SqlFormatOption
    {
        [JsonProperty("t")]
        public string Title { get; set; }
        [JsonProperty("i")]
        public string Id { get; set; }
        [JsonProperty("n")]
        public string NodeId { get; set; }
        [JsonProperty("e")]
        public bool IsExpandable { get; set; }
        [JsonProperty("cc")]
        public bool IsCheckable { get; set; }
        [JsonProperty("r")]
        public bool IsRadio { get; set; }
        [JsonProperty("c")]
        public bool IsChecked { get; set; }
        [JsonProperty("ch")]
        public List<SqlFormatOption> Childs { get; set; } = new List<SqlFormatOption>();
        [JsonProperty("fot")]
        public string FormatOptionType { get; set; }
        [JsonProperty("fov")]
        public string FormatOptionValue { get; set; }
        [JsonProperty("foi")]
        public string FormatOptionId { get; set; }
    }
}
