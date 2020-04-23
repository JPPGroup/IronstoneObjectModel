using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.DesignCalculations.Calculations;

namespace Jpp.Ironstone.Structures.ObjectModel.Appraisal
{
    interface IAppraisalObject
    {
        Calculation Calculation { get; }
    }
}
