using System;using System.Linq;using System.Collections.Generic;

public static class DecisionHelper {
    public static string GetNextBestMove(this Graph graph) {
        return graph.ProposeMoves().GetBestCommand(graph);
    }

    public static void AddMovesForJobs(this Graph graph, List<IJob> jobs, MultiMove resultMove) {}

    public static IEnumerable<IMove> ProposeMoves(this Graph graph) {
        var stepsToPredict = Math.Max(graph.GetMaxCountSteps(), GraphLinks.MaxDistance);

        bool canSendBomb = graph.BombsAvailable_MyOwn > 0;
        var bombFactoriesCandidate = new List<Factory>();

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
            if (canSendBomb)
            {
                var sendedBombTo = graph.Bombs.Any(x=>x.Side == Side.MyOwn) ? graph.Bombs.Select(x => x.Dst).First() : -1;
                bombFactoriesCandidate = holdState.Factories.Where(x => x.Side == Side.Enemy && x.Income > 1 && x.Id != sendedBombTo).ToList();
            }
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
                    }
                }
                move.ChangeGraph(testGraph);
                testGraph.DoSteps(testGraph.GetMaxCountSteps());
                // Если изменился статус фабрики - запоминаем
                if (testGraph.Factories[factoryTarget.Id].Side == Side.MyOwn)
                    factories.Add(new FactoryStates(factoryTarget.Id, stepsToPredict));
            }
        }
        //Select & send the bomb
        if (canSendBomb)
        {
            var bombTo = bombFactoriesCandidate
                .OrderByDescending(x => x.Income)
                .ThenByDescending(f => f.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.MyOwn).Min(x => x.Distance)) // Самая удаленная от нас
                .FirstOrDefault();
            if (bombTo != null)
            {
                var bombLinkSources = bombTo.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.MyOwn).OrderBy(x => x.Distance);
                if (bombLinkSources.Any())
                {
                    var bombLinkSettings = bombLinkSources.FirstOrDefault();
                    multiMove.AddMove(
                        new SendBomb(new Bomb
                        {
                            Src = bombLinkSettings.DestinationId,
                            Dst = bombTo.Id,
                            Side = Side.MyOwn,
                            Remaining = bombLinkSettings.Distance + 1,
                        }));
                }
            }
        }

        {
            var jobs = new List<IJob>();

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
                //multiMove.AddMove(new Message($"FS #{factories.Count}: "  + string.Join(", ", factories.Select(x => $"({x.FactoryId})" + string.Join(".", x.Enemies.Select(t=> t ?? -1))))));

                factories.ForEach(x => { jobs.Add(new AtackFactory(x, stepsToPredict));});
            }

            jobs.AddRange(
                graph.Factories
                    .Where(x => x.Side == Side.MyOwn && x.Income < 3) //TODO: condition can be changed, income check when bomb
                    .Select(x => {
                        if (x.TroopsCount >= 10 && x.TroopsCanBeUsed >= 9)
                            return (IJob)new UpgradeFactoryJob(x.Id);
                        else
                            return (IJob)new WaitToUpgradeFactoryJob(x.Id);
                    })
                );

            //Evaluate jobs
            var jobGraph = Graph.GetCopy(graph);
            while (true) {
                jobs.ForEach(x => { x.EvaluateInnerState(jobGraph); });
                var first = jobs.OrderByDescending(x => x.GetPriorityValue()).FirstOrDefault();
                if (first == null || first.GetPriorityValue() == int.MinValue) break;
                var jobMove = first.GetMove();
                Logger.ErrorLog("Best job Cost: " + first.GetPriorityValue() + " ACT: " + jobMove.GetConsoleCommand(), LogReportLevel.BestJobCost);

                jobMove.ChangeGraph(jobGraph);
                multiMove.AddMove(jobMove);
                jobs.Remove(first);
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
        decimal bestEstimate = decimal.MinValue;
        string bestMove = null;
        int bombsUsed = 0;
        var stepsToPredict = graph.GetMaxCountSteps();
        foreach (var move in moves) {
            //TODO: учитывать реальный инком фабрики а не 0 после бомбы.
            var estimate = move.GetEstimate(Graph.GetCopy(graph), Math.Max(GraphLinks.MaxDistance, Math.Max(stepsToPredict, move.StepsExecution())));
            Logger.ErrorLog("Move Cost: "+ estimate+" ACT:" + move.GetConsoleCommand(),LogReportLevel.EachMoveCost);
            if (estimate > bestEstimate) {
                bestEstimate = estimate;
                bestMove = move.GetConsoleCommand();
                if (move is MultiMove) {
                    bombsUsed = ((MultiMove)move).GetBombMoves();
                }
                else
                    bombsUsed = move is SendBomb ? 1 : 0;
            }
        }

        graph.BombsAvailable_MyOwn -= bombsUsed;
        Logger.ErrorLog("Best Move Cost: " + bestEstimate + " ACT:" + bestMove, LogReportLevel.BestMoveCost);
        return bestMove;
//        return $"MSG {i};" + string.Join(";", s) + ";" + bestMove;
    }

    public static Graph DoSteps(this Graph graph, int steps) {
        for (int i = 0; i < steps; i++)
            graph.DoNextMove();
        return graph;
    }

    public static decimal GetEstimate(this IMove move, Graph graph, int steps) {
        move.ChangeGraph(graph);

        //TODO: implement best enemy move and check
//        graph.DoSteps(steps);

        return graph.DoSteps(steps).EvalCostFunction();


    }
}