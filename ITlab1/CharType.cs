using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITlab1
{
    class CharType : BaseType
    {
        public override bool Validation(string value) => char.TryParse(value, out _);
    }
}
