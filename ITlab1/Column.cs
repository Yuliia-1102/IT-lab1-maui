using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ITlab1
{
    public class Column
    {
        public string Name { get; set; }
        public string TypeName { get; set; }

        [JsonIgnore]
        public Table Table { get; set; } = null!;

        public BaseType ColumnType;
        public Column(string name, string typeName)
        {
            Name = name;
            TypeName = typeName;
            switch (typeName)
            {
                case "Integer":
                    ColumnType = new IntegerType();
                    break;
                case "IntegerInvl":
                    ColumnType = new IntegerInvlType();
                    break;
                case "Real":
                    ColumnType = new RealType();
                    break;
                case "Char":
                    ColumnType = new CharType();
                    break;
                case "String":
                    ColumnType = new StringType();
                    break;
                case "TxtPath":
                    ColumnType = new TxtPath();
                    break;
                default:
                    ColumnType = new StringType();
                    break;
            }
        }
    }
}
