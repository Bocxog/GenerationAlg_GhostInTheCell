using System;using System.Linq;

public static class CostFunction {
    public static float EvalCostFunction(this Graph graph) {
        var factoriesMy = graph.Factories.Where(x => x.Side == Side.MyOwn).ToList();
        var factoriesEnemy = graph.Factories.Where(x => x.Side == Side.Enemy).ToList();

        if (!factoriesEnemy.Any(x => x.Income > 0 || x.TroopsCount > 0))
            return float.MaxValue;
        return factoriesMy.Sum(x => x.EvalMyFactoryCostFunction(graph))
               - factoriesEnemy.Sum(x => (x.Income + 0.01f)*(1 + x.TroopsCount*0.03f)) // With Neutrals
            ;
    }

    public static float EvalMyFactoryCostFunction(this Factory factory, Graph graph) {
        float result = factory.Income;

        if (factory.GetLinks().All(x => graph.Factories[x.DestinationId].Side == Side.MyOwn || graph.Factories[x.DestinationId].Income == 0))
            result -= 0.5f*Math.Min(1, ((float) factory.TroopsCount)/30);
        else if (!factory.GetLinks().Any(x => graph.Factories[x.DestinationId].Side == Side.Neutral && graph.Factories[x.DestinationId].Income > 0)) {
            var enemyDistance = factory.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.Enemy).DefaultIfEmpty().Min(x => x.Distance); // TODO: add 2 step compare
            var penalty = factory.TroopsCount*(enemyDistance > 15 ? 0.3f : enemyDistance > 10 ? 0.085f : 0);
            result -= penalty;
        }
        return result;
    }
}
