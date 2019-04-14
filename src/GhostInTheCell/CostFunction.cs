using System;using System.Linq;

public static class CostFunction {
    public static decimal EvalCostFunction(this Graph graph) {
        var factoriesMy = graph.Factories.Where(x => x.Side == Side.MyOwn).ToList();
        var factoriesEnemy = graph.Factories.Where(x => x.Side == Side.Enemy).ToList();

        if (!factoriesEnemy.Any(x => x.Income > 0 || x.TroopsCount > 0))//TODO: всё войско может быть в пути. хотя считаем в конце всех ходов...
            return decimal.MaxValue;
        return factoriesMy.Sum(x => x.EvalMyFactoryCostFunction(graph))
               - factoriesEnemy.Sum(x => (x.Income + GlobalConfig.Weight_Factory_Bonus + x.FactoryPotentialUpgradeCostFunction())*(1 + x.TroopsCount * GlobalConfig.Weight_Troop)) // With Neutrals
            ;
    }

    public static decimal FactoryPotentialUpgradeCostFunction(this Factory factory) {
        return (factory.Income < 3 && factory.TroopsCount >= 10 ? GlobalConfig.Factory_UpgradeCost : 0);
    }

    public static decimal EvalMyFactoryCostFunction(this Factory factory, Graph graph) {
        decimal result = (factory.Income + factory.FactoryPotentialUpgradeCostFunction()) * (1 + factory.TroopsCount * GlobalConfig.Factory_TroopWeight);
        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,$"F:{factory.Id}", result);
//        if (factory.GetLinks().Where(x => x.PathType == GraphLinks.PathType.Direct).All(x => graph.Factories[x.DestinationId].Side == Side.MyOwn))
//            result -= 0.5f*Math.Min(1, ((float) factory.TroopsCount)/30);
//        else 
        if (!factory.GetLinks().Any(x => graph.Factories[x.DestinationId].Side == Side.Neutral && graph.Factories[x.DestinationId].Income > 0)) {//TODO: change if condition
            var enemyDistance = factory.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.Enemy).DefaultIfEmpty().Min(x => x.Distance); // TODO: add 2 step compare
            var penalty = factory.TroopsCount * (enemyDistance > 15 ? GlobalConfig.LinkDistance_PenaltyCoef_15 : enemyDistance > 10 ? GlobalConfig.LinkDistance_PenaltyCoef_10 : GlobalConfig.LinkDistance_PenaltyCoef_other);
            Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost, $"P:{penalty}");
            result -= penalty;
        }
        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,"+");
        return result;
    }
    public static int GetJobEstimatePriority(int incomeIncrease, int troopsUsed, int steps, int bonus = 0) {
        return 5 * incomeIncrease
               - troopsUsed / 10
               - (int)Math.Pow(steps, 2)/5
               + bonus;
    }
}
