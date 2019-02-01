using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Structures.Objectmodel
{
    //[TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Shrinkage
    {
        [Description("Low plasticity")]
        Low,
        [Description("Medium plasticity")]
        Medium,
        [Description("High plasticity")]
        High
    }
}
