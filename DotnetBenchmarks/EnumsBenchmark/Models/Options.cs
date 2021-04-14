using System;
using System.ComponentModel;

namespace EnumsBenchmark.Models
{
    public enum Options
    {
        [Description("Read")]
        Read = 0,
        
        [Description( "Write")]
        Write = 1,
        
        [Description("Delete")]
        Delete = 2
    }
}