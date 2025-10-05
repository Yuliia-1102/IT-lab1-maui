using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITlab1
{
    class IntegerType : BaseType
    {
        public override bool Validation(string value) => int.TryParse(value, out _);
    }
}
