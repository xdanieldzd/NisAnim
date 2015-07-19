using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NisAnim.IO
{
    public class FileNamePatternAttribute : Attribute
    {
        public string Pattern;

        public FileNamePatternAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
