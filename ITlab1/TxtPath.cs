using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITlab1
{
    public class TxtPath : BaseType
    {
        public override bool Validation(string value)
        {
            if (!File.Exists(value)) return false;

            var ext = Path.GetExtension(value);
            if (!string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase)) return false;
            try
            {
                using (var reader = new StreamReader(value))
                {
                    var buffer = new char[4096];
                    var bytesRead = reader.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (buffer[i] == '\0')
                            return false;
                        if (char.IsControl(buffer[i]) &&
                            buffer[i] != '\r' &&
                            buffer[i] != '\n' &&
                            buffer[i] != '\t')
                            return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
