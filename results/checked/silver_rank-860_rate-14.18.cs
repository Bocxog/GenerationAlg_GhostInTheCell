using System;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq;
public static class CostFunction {
    public static float EvalCostFunction(this Graph graph) {
        var factoriesMy = graph.Factories.Where(x => x.Side == Side.MyOwn).ToList();
        var factoriesEnemy = graph.Factories.Where(x => x.Side == Side.Enemy).ToList();
        
        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,$"MF:{factoriesMy.Count}",$"EF:{factoriesEnemy.Count} ");
        if (!factoriesEnemy.Any(x => x.Income > 0 || x.TroopsCount > 0))//TODO: всё войско может быть в пути. хотя считаем в конце всех ходов...
            return float.MaxValue;
        return factoriesMy.Sum(x => x.EvalMyFactoryCostFunction(graph))
               - factoriesEnemy.Sum(x => (x.Income + 0.01f + x.FactoryPotentialUpgradeCostFunction())*(1 + x.TroopsCount*0.03f)) // With Neutrals
            ;
    }
    public static float FactoryPotentialUpgradeCostFunction(this Factory factory) {
        return (factory.Income < 3 && factory.TroopsCount >= 10 ? 0.7f : 0);
    }
    public static float EvalMyFactoryCostFunction(this Factory factory, Graph graph) {
        float result = (factory.Income + factory.FactoryPotentialUpgradeCostFunction()) * (1 + factory.TroopsCount * 0.03f);
        Logger.ErrorLogInline(LogReportLevel.InnerInlineMoveCost,$"F:{factory.Id}", result);
//        if (factory.GetLinks().Where(x => x.PathType == GraphLinks.PathType.Direct).All(x => graph.Factories[x.DestinationId].Side == Side.MyOwn))
//            result -= 0.5f*Math.Min(1, ((float) factory.TroopsCount)/30);
//        else 
        if (!factory.GetLinks().Any(x => graph.Factories[x.DestinationId].Side == Side.Neutral && graph.Factories[x.DestinationId].Income > 0)) {//TODO: change if condition
            var enemyDistance = factory.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.Enemy).DefaultIfEmpty().Min(x => x.Distance); // TODO: add 2 step compare
            var penalty = factory.TroopsCount*(enemyDistance > 15 ? 0.3f : enemyDistance > 10 ? 0.085f : 0);
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
public class Troop {
    public Side Side { get; set; }
    public int Size { get; set; }
    public int Src { get; set; }
    public int Dst { get; set; }
    public int? DstInCommand { get; set; }
    public int Remaining { get; set; }
    public static Troop GetCopy(Troop x, int remainingDecreased = 0) {
        return new Troop { Dst = x.Dst,DstInCommand = x.DstInCommand, Side = x.Side, Size = x.Size, Src = x.Src, Remaining = x.Remaining - remainingDecreased };
    }
    //    public Troop() { }
    //    public Troop(Troop x) {
    //        Dst = x.Dst;
    //        Side = x.Side;
    //        Size = x.Size;
    //        Src = x.Src;
    //        Remaining = x.Remaining;
    //    }
}
public class Factory {
    public Factory(int id) {
        Id = id;
    }
    public int Id { get; set; }
    public int Income { get; set; }
    public Side Side { get; set; }
    public int TroopsCount { get; set; }
    public int TroopsCanBeUsed { get; set; }
    public IEnumerable<FactoryLink> GetLinks() {
        for (int i = 0; i < GraphLinks.Size; i++) {
            var path = GraphLinks.Links[Id, i];
            if (Id != i && path.PathType != GraphLinks.PathType.NotConnected)
                yield return new FactoryLink { DestinationId = i, Distance = path.Distance, FirstFactoryId = path.FirstFactoryId, PathType = path.PathType};
        }
    }
    public struct FactoryLink {
        public int Distance;
        public int DestinationId;
        public int FirstFactoryId;
        public GraphLinks.PathType PathType;
    }
}
public class GraphEstimator : Graph {}
public class Graph {
    public Factory[] Factories { get; private set; }
    public List<Troop> Troops { get; private set; }
    public static Graph GetCopy(Graph obj) {
        var copy = new Graph();
        copy.Factories = obj.Factories.Select(x => new Factory(x.Id) { Income = x.Income, Side = x.Side, TroopsCount = x.TroopsCount, TroopsCanBeUsed = x.TroopsCanBeUsed}).ToArray();
        copy.Troops = obj.Troops.Select(x => Troop.GetCopy(x)).ToList();
        return copy;
    }
    public void DoNextMove() {
        foreach (var factory in Factories.Where(x => x.Side != Side.Neutral)) { factory.TroopsCount += factory.Income; }
        foreach (var troop in Troops) {troop.Remaining--;}
        foreach (var troopGroup in Troops.Where(x => x.Remaining == 0).GroupBy(x => x.Dst)) {
            var factory = Factories[troopGroup.First().Dst];
            int troopToFactory =
                troopGroup.Where(x => x.Side == Side.MyOwn).Sum(x => x.Size) -
                troopGroup.Where(x => x.Side == Side.Enemy).Sum(x => x.Size);
            if (troopToFactory == 0)
                continue;
            Side troopRestSide = troopToFactory > 0 ? Side.MyOwn : Side.Enemy;
            troopToFactory = Math.Abs(troopToFactory);
            var sign = 1;
            //            if (factory.Side == Side.Neutral  ? troopRestSide == Side.Enemy : (factory.Side != troopRestSide)) 
            if (factory.Side != troopRestSide)
                sign = -1;
            factory.TroopsCount += troopToFactory * sign;
            if (factory.TroopsCount < 0) {
                factory.Side = troopRestSide;
                factory.TroopsCount = Math.Abs(factory.TroopsCount);
            }
        }
        Troops = Troops.Where(x => x.Remaining > 0).ToList();
        //        foreach (var factory in Factories) {
        //            if (factory.Side == Side.Neutral) {
        //                if (factory.TroopsCount != 0)
        //                    factory.Side = factory.TroopsCount > 0 ? Side.MyOwn : Side.Enemy;
        //            }
        //            else if (factory.TroopsCount < 0)
        //                factory.Side = factory.Side == Side.Enemy ? Side.MyOwn : Side.Enemy;
        //        }
    }
    public Graph() {
        Troops = new List<Troop>();
        Factories = new Factory[GraphLinks.Size];
    }
    public void ClearTroops() {
        Troops.Clear();
    }
    public void AddTroop(Side side, int size, int src, int dst, int remaining) {
        Troops.Add(new Troop {
            Side = side,
            Dst = dst,
            Remaining = remaining,
            Size = size,
            Src = src
        });
    }
    public int GetTroopSteps() {
        return Troops.Any() ? Troops.Max(x => x.Remaining) : 0;
    }
    protected Factory GetFactory(int idx) {
        var fact = Factories[idx];
        if (fact != null)
            return fact;
        fact = new Factory(idx);
        Factories[idx] = fact;
        return fact;
    }
    public void RefreshFactoryInfo(int id, int side, int income, int troopsCount) {
        var f = Factories[id];
        f.Income = income;
        f.Side = (Side)side;
        f.TroopsCount = troopsCount;
    }
    public void AddLink(int f1, int f2, int cost) {
        var Fact1 = GetFactory(f1);
        var Fact2 = GetFactory(f2);
        GraphLinks.Links[f1, f2] = new GraphLinks.ShortestPath {Distance = cost, FirstFactoryId = f2, PathType = GraphLinks.PathType.Direct};
        GraphLinks.Links[f2, f1] = new GraphLinks.ShortestPath { Distance = cost, FirstFactoryId = f1, PathType = GraphLinks.PathType.Direct };
    }
}
public interface IJob {
    void EvaluateInnerState(Graph graphToCopy);
    int GetPriorityValue();
    IMove GetMove();
}
public class AtackFactory : IJob {
    private DecisionHelper.FactoryStates FactoryState;
    private int StepsToPredict;
    public AtackFactory(DecisionHelper.FactoryStates factoryState, int stepsToPredict) {
        FactoryState = factoryState;
        StepsToPredict = stepsToPredict;
    }
    private Graph graph = null;
    private IMove move = null;
    private int priorityValue = int.MinValue;
    public void EvaluateInnerState(Graph graphToCopy) {
        graph = Graph.GetCopy(graphToCopy);
        move = EvaluateMove();
    }
    public int GetPriorityValue() {
        Logger.ErrorLog("Cost: " + priorityValue + " ACT: " + move.GetConsoleCommand(), LogReportLevel.InnerJobCost);
        return priorityValue;
    }
    public IMove GetMove() {
        return move;
    }
    private IMove EvaluateMove() {
//        throw new NotImplementedException();
        var multiMove = new MultiMove();
        multiMove.AddMove(new Message($"Fight for {FactoryState.FactoryId}"));
        var moveTroops = new MoveTroops();
        multiMove.AddMove(moveTroops);
        for (int i = 0; i < StepsToPredict; i++) {
            //взять  мои фабрики на этом расстоянии
            var myFactoriesForAttack = graph.Factories[FactoryState.FactoryId].GetLinks()
                .Where(x => x.Distance <= i && graph.Factories[x.DestinationId].Side == Side.MyOwn)
                .Select(x => graph.Factories[x.DestinationId])
                .ToList();
            //если могут захватить - ура
            if (myFactoriesForAttack.Any() && myFactoriesForAttack.Sum(x => x.TroopsCanBeUsed) > FactoryState.Enemies[i]) {
                int troopsUsed, needed;
                needed = troopsUsed = FactoryState.Enemies[i] ?? int.MaxValue;
                foreach (var myFactory in myFactoriesForAttack) {
//TODO: sort factories by distance or smth else
                    var sendedTroops = Math.Min(needed + 1, myFactory.TroopsCanBeUsed); // TODO: захват нейтралов совместный. +1 не совсем точно. нужно только для захвата но не деф.
                    myFactory.TroopsCanBeUsed =
                        graph.Factories[myFactory.Id].TroopsCanBeUsed = myFactory.TroopsCanBeUsed - sendedTroops;
                    var shortestPath = GraphLinks.Links[myFactory.Id, FactoryState.FactoryId];
                    moveTroops.AddTroop(new Troop {
                        Dst = FactoryState.FactoryId,
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
                priorityValue = CostFunction.GetJobEstimatePriority(
                    graph.Factories[FactoryState.FactoryId].Income*(graph.Factories[FactoryState.FactoryId].Side == Side.Neutral ? 1 : 2),
                    troopsUsed, i);
                break;
            }
        }
        return multiMove;
    }
}
public class UpgradeFactoryJob : IJob {
    private int FactoryId;
    private MultiMove move;
    private bool moveAvailable;
    public UpgradeFactoryJob(int factoryId) {
        FactoryId = factoryId;
        move = new MultiMove();
        move.AddMove(new UpgradeFactory(FactoryId));
    }
    public void EvaluateInnerState(Graph graphToCopy) {
        moveAvailable = graphToCopy.Factories[FactoryId].TroopsCount >= 10;
    }
    public int GetPriorityValue() {
        if (!moveAvailable) return int.MinValue;
        var result = CostFunction.GetJobEstimatePriority(1, 10, 10, 15);
        Logger.ErrorLog("Cost: " + result + " ACT: " + move.GetConsoleCommand(), LogReportLevel.InnerJobCost);
        return result;
    }
    public IMove GetMove() {
        return move;
    }
}
public interface IMove {
    int StepsExecution();
    void ChangeGraph(Graph graph);
    string GetConsoleCommand();
    IMove GetCopy();
}
public class MultiMove : IMove {
    public ICollection<IMove> moves;
    public MultiMove() {
        moves = new List<IMove> {new Hold()};
    }
    public void AddMove(IMove move) { moves.Add(move);}
    public void RemoveLastMove() { moves.Remove(moves.Last());}
    public void RemoveMove(IMove move) { moves.Remove(move);}
    public int StepsExecution() {
        return moves.Any() ? moves.Max(x => x.StepsExecution()) : 0;
    }
    public void ChangeGraph(Graph graph) {
        foreach (var move in moves) {
            move.ChangeGraph(graph);
        }
    }
    public string GetConsoleCommand() {
        return string.Join(";", moves.Select(x => x.GetConsoleCommand()));
    }
    public IMove GetCopy() {
        var result = new MultiMove();
        foreach (var move in moves) {
            result.AddMove(move.GetCopy());
        }
        return result;
    }
}
public class UpgradeFactory : IMove {
    private readonly int FactoryId;
    public UpgradeFactory(int factoryId) {
        FactoryId = factoryId;
    }
    public int StepsExecution() {return 11;}
    public void ChangeGraph(Graph graph) {
        var factory = graph.Factories[FactoryId];
        if (factory.TroopsCount < 10)
            throw new NotSupportedException("Invalid command");
        factory.TroopsCount -= 10;
        factory.TroopsCanBeUsed -= 10;
        factory.Income++;
    }
    public string GetConsoleCommand() {
        return "INC " + FactoryId;
    }
    public IMove GetCopy() {
        return new UpgradeFactory(FactoryId);
    }
}
public class MoveTroops : IMove {
    protected ICollection<Troop> Troops;
    public MoveTroops() {
        Troops = new List<Troop>();
    }
    public MoveTroops(Troop troop) {
        Troops = new List<Troop> {troop};
    }
    public MoveTroops(IEnumerable<Troop> troops) {
        Troops = troops.ToList();
    }
    public void AddTroop(Troop troop) { Troops.Add(troop);}
    public string GetConsoleCommand() {
        return string.Join(";", Troops.Select(x => string.Format("MOVE {0} {1} {2}", x.Src, x.DstInCommand ?? x.Dst, x.Size))); //MOVE source destination cyborgCount
    }
    public IMove GetCopy() {
        return new MoveTroops(Troops.Select(x=> Troop.GetCopy(x)));
    }
    public int StepsExecution() {
        return Troops.Any() ? Troops.Max(x => x.Remaining) : 0;
    }
    public void ChangeGraph(Graph graph) {
        var troopsToGraph = Troops.Select(x => Troop.GetCopy(x)).ToList();
        graph.Troops.AddRange(troopsToGraph);
        foreach (var troop in troopsToGraph) {
            var factory = graph.Factories[troop.Src];
            factory.TroopsCanBeUsed -= troop.Size;
            factory.TroopsCount -= troop.Size;
        }
    }
}
public class Hold : IMove {
    public void ChangeGraph(Graph graph) {}
    public virtual string GetConsoleCommand() {
        return "WAIT";
    }
    public virtual IMove GetCopy() {
        return new Hold();
    }
    public int StepsExecution() {
        return 1;
    }
}
public class Message : Hold {
    private readonly string _message;
    public Message(string message) {
        this._message = message.Replace(';', '_');
    }
    public override IMove GetCopy() {
        return new Message(_message);
    }
    public override string GetConsoleCommand() {
        return "MSG " + _message;
    }
}
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
            GraphLinks.MaxDistance = 0;
            for (int i = 0; i < factoryCount; i++) 
                for (int j = 0; j < factoryCount; j++)
                    if (GraphLinks.Links[i, j].PathType != GraphLinks.PathType.NotConnected)
                        GraphLinks.MaxDistance = Math.Max(GraphLinks.MaxDistance, GraphLinks.Links[i, j].Distance);
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
                    if (entityId == 3)
                        Console.Error.WriteLine("3FI: "+arg3);
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
        var a = Graph.GetCopy(graph);
        return graph.ProposeMoves().GetBestCommand(a);
    }
    public static void AddMovesForJobs(this Graph graph, List<IJob> jobs, MultiMove resultMove) {}
    public static IEnumerable<IMove> ProposeMoves(this Graph graph) {
        var stepsToPredict = Math.Max(graph.GetTroopSteps(), GraphLinks.MaxDistance);
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
                    .Where(x => x.TroopsCount >= 10 && x.Side == Side.MyOwn && x.Income < 3 && x.TroopsCanBeUsed >= 9) //TODO: condition can be changed, income check when bomb
                    .Select(factoryToUpgrade => new UpgradeFactoryJob(factoryToUpgrade.Id))
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
        float bestEstimate = float.MinValue;
        string bestMove = null;
        var stepsToPredict = graph.GetTroopSteps();
        foreach (var move in moves) {
            //TODO: учитывать реальный инком фабрики а не 0 после бомбы.
            var estimate = move.GetEstimate(Graph.GetCopy(graph), Math.Max(GraphLinks.MaxDistance, Math.Max(stepsToPredict, move.StepsExecution())));
            Logger.ErrorLog("Move Cost: "+ estimate+" ACT:" + move.GetConsoleCommand(),LogReportLevel.EachMoveCost);
            if (estimate > bestEstimate) {
                bestEstimate = estimate;
                bestMove = move.GetConsoleCommand();
            }
        }
        Logger.ErrorLog("Best Move Cost: " + bestEstimate + " ACT:" + bestMove, LogReportLevel.BestMoveCost);
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
//Console.Error.Write("GT:"+graph.Troops.Count+".S:"+steps);
//foreach (var tr in graph.Troops)Console.Error.WriteLine($"   {tr.Src} - > {tr.Dst}({tr.DstInCommand}) No-{tr.Size} d-"+tr.Remaining);
        //TODO: implement best enemy move and check
//        graph.DoSteps(steps);
if (move is MultiMove)
                Console.Error.Write(" MC:"+((MultiMove)move).moves.Count+" ");
        return graph.DoSteps(steps).EvalCostFunction();
    }
}
public static class GraphLinks {
    public static int MaxDistance { get; set; }
    public static int Size { get; set; }
    public static ShortestPath[,] Links { get; set; }
    public struct ShortestPath {
        public int Distance;
        public int FirstFactoryId;
        public PathType PathType;
    }
    public enum PathType {
        Direct = 0,
        WithMiddle = 1,
        NotConnected
    }
}
public enum Side {
    MyOwn = 1,
    Neutral = 0,
    Enemy = -1
}
[Flags]
public enum LogReportLevel {
    None = 0,
    InnerJobCost = 1 << 0,    
    BestJobCost = 1 << 1,    
    EachMoveCost = 1 << 2,    
    BestMoveCost = 1 << 3,
    InnerInlineMoveCost = 1 << 4,    
//    InnerJobCostValue = 1 << 5,    
//    InnerJobCostValue = 1 << 6,    
//    InnerJobCostValue = 1 << 7,    
    All = InnerJobCost | BestJobCost | EachMoveCost | BestMoveCost | InnerInlineMoveCost
}
public static class Logger {
    public static void ErrorLog(string text, LogReportLevel level) {
        if ((level & LogLevel) == 0) return;
        Console.Error.WriteLine(text);
    }
    public static void ErrorLogInline(LogReportLevel level, params object[] text) {
        if ((level & LogLevel) == 0) return;
        Console.Error.Write(string.Join("|",text.Select(x=>x.ToString())));
    }
    private static LogReportLevel LogLevel =
        //LogReportLevel.InnerJobCost |
        //LogReportLevel.BestJobCost |
        LogReportLevel.EachMoveCost |
        //LogReportLevel.InnerInlineMoveCost |
        LogReportLevel.BestMoveCost |
        0;
}