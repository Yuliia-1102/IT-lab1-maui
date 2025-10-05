using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ITlab1
{
    public class Row
    {
        public List<string> Values { get; set; } = new List<string>();

        [JsonIgnore]
        public Table Table { get; set; } = null!;
    }
}
