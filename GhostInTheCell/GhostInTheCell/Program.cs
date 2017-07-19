
using System;using System.Linq;using System.IO;using System.Text;using System.Collections;using System.Collections.Generic;


/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        GraphLinks.Size = factoryCount;
        GraphLinks.Links = new int[factoryCount,factoryCount];
        var graph = new Graph();
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);
            graph.AddLink(factory1,factory2,distance);
        }

        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            graph.ClearTroops();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);

                if (entityType == "FACTORY") {
                    graph.RefreshFactoryInfo(entityId, arg1, arg3, arg2);
                } else if (entityType == "TROOP") {
                    graph.AddTroop((Side) arg1, arg4, arg2, arg3, arg5);
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(graph.GetNextBestMove().GetConsoleCommand());
            // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
//            Console.WriteLine("WAIT");
        }
    }
}

public static class Helper {
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

    public static IMove GetNextBestMove(this Graph graph) {
        var moves = new List<IMove> {new Hold()};
        foreach (var factory in graph.Factories.Where(x => x.TroopsCount > 0 && x.Side == Side.MyOwn)) {
            foreach (var link in factory.GetLinks()) {
                moves.AddRange(Enumerable.Range(1, factory.TroopsCount).Select(x => new MoveTroops(new Troop {
                    Dst = link.DestinationId,
                    Src = factory.Id,
                    Side = Side.MyOwn,
                    Size = x,
                    Remaining = link.Distance + 1
                })));
            }
//            for (int i = 0; i < GraphLinks.Size; i++) {
//                var distance = GraphLinks.Links[factory.Id, i];
//                if (factory.Id != i && distance > 0)
//                    moves.AddRange(Enumerable.Range(1, factory.TroopsCount).Select(x => new MoveTroops(new Troop {
//                        Dst = i,
//                        Src = factory.Id,
//                        Side = Side.MyOwn,
//                        Size = x,
//                        Remaining = distance
//                    })));
//            }
        }

        float bestEstimate = float.MinValue;
        IMove bestMove = null;
        var stepsToPredict = graph.Troops.Any() ? graph.Troops.Max(x => x.Remaining) : 0;

        foreach (var move in moves) {
            var estimate = move.GetEstimate(Graph.GetCopy(graph), Math.Max(stepsToPredict, move.StepsExecution()));
            if (estimate > bestEstimate) {
                bestEstimate = estimate;
                bestMove = move;
            }
        }
        return bestMove;
    }


    public static float GetEstimate(this IMove move, Graph graph, int steps) {
        move.ChangeGraph(graph);
        for (int i = 0; i < steps; i++)
            graph = new Graph(graph);

        return graph.EvalCostFunction();


    }
}