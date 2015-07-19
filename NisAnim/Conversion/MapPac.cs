using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("Map Pac File")]
    [FileNamePattern("(map)(.*?)\\.(pac)$")]
    public class MapPac : Pac
    {
        public MapPac(string filePath)
            : base(filePath)
        {
            //
        }
    }
}
