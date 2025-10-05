using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ITlab1
{
    public class Table
    {
        public string Name { get; set; }

        [JsonIgnore]
        public Database Database { get; set; } = null!;
        public List<Column> Columns { get; set; } = new List<Column>();
        public List<Row> Rows { get; set; } = new List<Row>();

        [JsonIgnore]
        public DataTable DataTable { get; } = new();
        public Table(string name)
        {
            Name = name;
            DataTable.TableName = name;
        }
    }
}
