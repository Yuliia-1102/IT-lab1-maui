using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ITlab1
{
    internal class IntegerInvlType : BaseType
    {
        public override bool Validation(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            var m = Regex.Match(value, @"^\s*([+-]?\d+)\s+([+-]?\d+)\s*$");
            if (!m.Success) return false;

            if (!int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start))
                return false;
            if (!int.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var end)) 
                return false;

            return start < end;
        }
    }
}
