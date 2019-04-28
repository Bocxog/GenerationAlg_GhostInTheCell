using System;using System.Linq;using System.Collections.Generic;using System.Globalization;

public static class GlobalConfig {
    public static decimal LinkDistance_PenaltyCoef_15    = 0.3M;
    public static decimal LinkDistance_PenaltyCoef_10    = 0.085M;
    public static decimal LinkDistance_PenaltyCoef_other = 0.0M;

    public static decimal Weight_Troop = 0.7M;
    public static decimal Weight_Factory_Bonus = 0.1M;

    public static decimal Factory_UpgradeCost = 0.7M;

    public static decimal Factory_TroopWeight = 0.03M;

    internal static void FillGlobalConstants(string[] args) {
        if (args.Length == 0) return;

        var parameters = args[0].Split('|').Select(x=> decimal.Parse(x, CultureInfo.InvariantCulture)).ToArray();

        if (parameters.Count() == 3) {
            LinkDistance_PenaltyCoef_15    = parameters[0];
            LinkDistance_PenaltyCoef_10    = parameters[1];
            LinkDistance_PenaltyCoef_other = parameters[2];
            Weight_Troop                   = parameters[3];
            Weight_Factory_Bonus           = parameters[4];
            Factory_UpgradeCost            = parameters[5];
            Factory_TroopWeight            = parameters[6];
        }
    }
}

public static class Constants
{
    public const int BOMB_EXPLODE_DURATION = 5;
}
