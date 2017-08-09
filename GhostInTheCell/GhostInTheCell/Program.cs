using System;using System.Linq;using System.Collections.Generic;


/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args) {
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        GraphLinks.Size = factoryCount;
        GraphLinks.Links = new GraphLinks.ShortestPath[factoryCount, factoryCount];
        var graph = new Graph();
        for (int i = 0; i < linkCount; i++) {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);
            graph.AddLink(factory1, factory2, distance);
        }

        {
            for (int i = 0; i < factoryCount; i++) {
                for (int j = 0; j < factoryCount; j++) {
                    if (i != j && GraphLinks.Links[i, j].Distance == 0)
                        GraphLinks.Links[i, j].PathType = GraphLinks.PathType.NotConnected;
                }
            }

            var addedLink = false;
            do {
                for (int i = 0; i < factoryCount; i++) {
                    for (int j = 0; j < factoryCount; j++) {
                        if (i != j && GraphLinks.Links[i, j].PathType == GraphLinks.PathType.NotConnected) {
                            var minDist = 0;
                            var minFactory = -1;
                            for (int k = 0; k < factoryCount; k++) {
                                if (k != i && k != j &&
                                    GraphLinks.Links[i, k].PathType != GraphLinks.PathType.NotConnected &&
                                    GraphLinks.Links[k, j].PathType != GraphLinks.PathType.NotConnected) {
                                    var dist = GraphLinks.Links[i, k].Distance + GraphLinks.Links[k, j].Distance;
                                    if (minDist == 0 || minDist > dist) {
                                        minDist = dist;
                                        minFactory = k;
                                    }

                                }
                            }

                            if (minDist != 0) {
                                addedLink = true;
                                GraphLinks.Links[i, j] = new GraphLinks.ShortestPath { Distance = minDist, PathType = GraphLinks.PathType.WithMiddle, FirstFactoryId = GraphLinks.Links[i, minFactory].FirstFactoryId };
                                GraphLinks.Links[j, i] = new GraphLinks.ShortestPath { Distance = minDist, PathType = GraphLinks.PathType.WithMiddle, FirstFactoryId = GraphLinks.Links[j, minFactory].FirstFactoryId };
                            }
                        }
                    }
                }

            } while (addedLink);
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

            Console.WriteLine(graph.GetNextBestMove());
            // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
//            Console.WriteLine("WAIT");
        }
    }
}

public static class DecisionHelper {
    public static string GetNextBestMove(this Graph graph) {
        return graph.ProposeMoves().GetBestCommand(graph);


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
        }
//        return moves.GetBestCommand(graph);

    }

    public static IEnumerable<IMove> ProposeMoves(this Graph graph) {
        var stepsToPredict = graph.GetTroopSteps();
        var factories = new List<FactoryStates>();
        var multiMove = new MultiMove();
        yield return multiMove;

        { //Get List available for attack/defense
            var holdState = Graph.GetCopy(graph);
            graph.UpdateAvailableTroops(holdState, true);
            for (int i = 0; i < stepsToPredict; i++) {
                holdState.DoNextMove();
                graph.UpdateAvailableTroops(holdState);
            }
//            multiMove.AddMove(new Message("CBU:"+graph.Factories.Sum(x=>x.TroopsCanBeUsed)));

            // если не моя по истечению ходов то стоит ее проверить.
            foreach (var factoryTarget in holdState.Factories.Where(x => x.Side != Side.MyOwn && x.Income > 0)) {
                var testGraph = Graph.GetCopy(graph);
                var move = new MoveTroops();
                // шлем со всех туда. 
                foreach (var myFactory in testGraph.Factories.Where(x => x.Side == Side.MyOwn && x.Id != factoryTarget.Id)) {
                    var availableTroops = graph.Factories[myFactory.Id].TroopsCanBeUsed;
                    if (availableTroops <= 0) continue;

                    //TODO: слать с переправами
                    var linkTo = myFactory.GetLinks().FirstOrDefault(link => link.DestinationId == factoryTarget.Id);
                    if (linkTo.Distance > 0) {
                        // if any
                        move.AddTroop(new Troop {
                            Dst = factoryTarget.Id,
                            Side = myFactory.Side,
                            Src = myFactory.Id,
                            Remaining = linkTo.Distance + 1,
                            Size = availableTroops
                        });
                        myFactory.TroopsCount -= availableTroops;
                    }
                }
                move.ChangeGraph(testGraph);
                testGraph.DoSteps(testGraph.GetTroopSteps());
                // Если изменился статус фабрики - запоминаем
                if (testGraph.Factories[factoryTarget.Id].Side == Side.MyOwn)
                    factories.Add(new FactoryStates(factoryTarget.Id, stepsToPredict));
            }
        }
        {
            var result = new MoveTroops();
            // Get moves to minimum attack
            if (factories.Any()) {
                multiMove.AddMove(new Message("Init Atack/Defense"));

                var holdState = Graph.GetCopy(graph);
                for (int i = 0; i < stepsToPredict; i++) {
                    holdState.DoNextMove();
                    foreach (var factoryState in factories) {
                        var factory = holdState.Factories[factoryState.FactoryId];
                        if (factory.Side == Side.MyOwn)
                            factoryState.Enemies[i] = null;
                        else
                            factoryState.Enemies[i] = factory.TroopsCount;
                    }
                }
                // fill nulls be next first value
                foreach (var factoryState in factories) {
                    for (int i = stepsToPredict - 2; i >= 0; i--) {
                        if (!factoryState.Enemies[i].HasValue)
                            factoryState.Enemies[i] = factoryState.Enemies[i + 1];
                    }
                }


                foreach (var factoryState in factories.OrderBy(x => {
                    var side = graph.Factories[x.FactoryId].Side;
                    return side == Side.MyOwn ? 0 : side == Side.Neutral ? 2 : 4;
                })) {
                    multiMove.AddMove(new Message($"Fight for {factoryState.FactoryId}"));

                    for (int i = 0; i < stepsToPredict; i++) {
                        //взять  мои фабрики на этом расстоянии
                        var myFactoriesForAttack = graph.Factories[factoryState.FactoryId].GetLinks()
                            .Where(x => x.Distance <= i && graph.Factories[x.DestinationId].Side == Side.MyOwn)
                            .Select(x => graph.Factories[x.DestinationId])
                            .ToList();
                        //если могут захватить - ура
                        if (myFactoriesForAttack.Any() && myFactoriesForAttack.Sum(x => x.TroopsCanBeUsed) > factoryState.Enemies[i]) {
                            var needed = factoryState.Enemies[i] ?? int.MaxValue;
                            for (int j = 0; j < myFactoriesForAttack.Count; j++) {
                                var myFactory = myFactoriesForAttack[j];
                                var sendedTroops = Math.Min(needed, myFactory.TroopsCanBeUsed); // TODO: захват нейтралов совместный.
                                myFactory.TroopsCanBeUsed =
                                    graph.Factories[myFactory.Id].TroopsCanBeUsed = myFactory.TroopsCanBeUsed - sendedTroops;

                                var shortestPath = GraphLinks.Links[myFactory.Id, factoryState.FactoryId];
                                result.AddTroop(new Troop {
                                    Dst = factoryState.FactoryId,
                                    DstInCommand = shortestPath.FirstFactoryId,
                                    Side = Side.MyOwn,
                                    Remaining = shortestPath.Distance + 1,
                                    Src = myFactory.Id,
                                    Size = sendedTroops
                                });

                                needed -= sendedTroops;
                                if (needed <= 0)
                                    break;
                            }

                            break;
                        }

                    }

                }
                multiMove.AddMove(result);
                yield return multiMove;

            }
            multiMove.AddMove(new Message("Init basic moves"));
            // Just move troops
            {
                // TODO: независимо для каждой фабрики выбирать
                foreach (var factory in graph.Factories.Where(x => x.Side == Side.MyOwn && x.TroopsCanBeUsed > 0)) {
                    foreach (var factoryLink in factory.GetLinks()) {
                        var checkedMove = new MoveTroops(new Troop {
                            Dst = factoryLink.DestinationId,
                            DstInCommand = factoryLink.FirstFactoryId,
                            Side = Side.MyOwn,
                            Remaining = factoryLink.Distance + 1,
                            Src = factory.Id,
                            Size = factory.TroopsCanBeUsed
                        });
                        multiMove.AddMove(checkedMove);
                        yield return multiMove;
                        multiMove.RemoveMove(checkedMove);
                    }
                }
//                var l = graph.Factories.Where(x => x.Side == Side.MyOwn && x.TroopsCanBeUsed > 0)
//                    .OrderByDescending(x => {
//                        var links = x.GetLinks().Where(t => graph.Factories[t.DestinationId].Side == Side.Enemy);
//                        if (!links.Any()) return 0;
//
//                        return links.Min(t => t.Distance);
//                    });
            }

            yield return multiMove;
        }
    }

    private static void UpdateAvailableTroops(this Graph graph, Graph holdState, bool force = false) {
        foreach (var factory in holdState.Factories) {
            var val = factory.Side != Side.MyOwn ? 0 : factory.TroopsCount;
            if (force || val < graph.Factories[factory.Id].TroopsCanBeUsed)
                graph.Factories[factory.Id].TroopsCanBeUsed = val;
        }
    }

    public struct FactoryStates {
        public FactoryStates(int factoryId, int size) {
            FactoryId = factoryId;
            Enemies = new int?[size];
        }
        public int FactoryId { get; set; }
        public int?[] Enemies { get; set; }
    }

    public static string GetBestCommand(this IEnumerable<IMove> moves, Graph graph) {
        float bestEstimate = float.MinValue;
        string bestMove = null;
        var stepsToPredict = graph.GetTroopSteps();
        foreach (var move in moves) {
            var estimate = move.GetEstimate(Graph.GetCopy(graph), Math.Max(stepsToPredict, move.StepsExecution()));
            if (estimate > bestEstimate) {
                bestEstimate = estimate;
                bestMove = move.GetConsoleCommand();
            }
        }
        return bestMove;
//        return $"MSG {i};" + string.Join(";", s) + ";" + bestMove;
    }

    public static Graph DoSteps(this Graph graph, int steps) {
        for (int i = 0; i < steps; i++)
            graph.DoNextMove();
        return graph;
    }

    public static float GetEstimate(this IMove move, Graph graph, int steps) {
        move.ChangeGraph(graph);

        //TODO: implement best enemy move and check
//        graph.DoSteps(steps);

        return graph.DoSteps(steps).EvalCostFunction();


    }
}