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





    //    public static float EvalCostFunction(this Graph graph) {
    //        var factoriesMy = graph.Factories.Where(x => x.Side == Side.MyOwn).ToList();
    //        var factoriesEnemy = graph.Factories.Where(x => x.Side == Side.Enemy).ToList();

    //        if (!factoriesEnemy.Any(x => x.Income > 0 || x.TroopsCount > 0))//TODO: всё войско может быть в пути. хотя считаем в конце всех ходов...
    //            return float.MaxValue;
    //        return factoriesMy.Sum(x => x.EvalMyFactoryCostFunction(graph))
    //               - factoriesEnemy.Sum(x => (x.Income + 0.01f + x.FactoryPotentialUpgradeCostFunction())*(1 + x.TroopsCount*0.03f)) // With Neutrals
    //            ;
    //    }

    //    public static float FactoryPotentialUpgradeCostFunction(this Factory factory) {
    //        return (factory.Income < 3 && factory.TroopsCount >= 10 ? 0.7f : 0);
    //    }

    //    public static float EvalMyFactoryCostFunction(this Factory factory, Graph graph) {
    //        float result = (factory.Income + factory.FactoryPotentialUpgradeCostFunction()) * (1 + factory.TroopsCount * 0.03f);
    //        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,$"F:{factory.Id}", result);
    ////        if (factory.GetLinks().Where(x => x.PathType == GraphLinks.PathType.Direct).All(x => graph.Factories[x.DestinationId].Side == Side.MyOwn))
    ////            result -= 0.5f*Math.Min(1, ((float) factory.TroopsCount)/30);
    ////        else 
    //        if (!factory.GetLinks().Any(x => graph.Factories[x.DestinationId].Side == Side.Neutral && graph.Factories[x.DestinationId].Income > 0)) {//TODO: change if condition
    //            var enemyDistance = factory.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.Enemy).DefaultIfEmpty().Min(x => x.Distance); // TODO: add 2 step compare
    //            var penalty = factory.TroopsCount*(enemyDistance > 15 ? 0.3f : enemyDistance > 10 ? 0.085f : 0);
    //            Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost, $"P:{penalty}");
    //            result -= penalty;
    //        }
    //        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,"+");
    //        return result;
    //    }
    //    public static int GetJobEstimatePriority(int incomeIncrease, int troopsUsed, int steps, int bonus = 0) {
    //        return 5 * incomeIncrease
    //               - troopsUsed / 10
    //               - (int)Math.Pow(steps, 2)/5
    //               + bonus;
    //    }
}
