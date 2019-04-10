using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmPowerChecker {
    public enum SingleFightResult {
        FirstWin   = 1,
        SecondLose = 1,

        SecondWin = -1,
        FirstLost = -1,

        Draw = 0
    }
}
