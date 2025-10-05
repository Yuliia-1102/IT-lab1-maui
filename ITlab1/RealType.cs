using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITlab1
{
    class RealType : BaseType
    {
        public override bool Validation(string value) => double.TryParse(value, out _);
    }
}
