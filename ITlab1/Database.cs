using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITlab1
{
    public class Database
    {
        public string Name { get; }
        public List<Table> Tables { get; set; } = new List<Table>();
        public Database(string name)
        {
            Name = name;
        }
    }
}
