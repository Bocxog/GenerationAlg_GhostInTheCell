﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheCell {
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Player {
        static void Main(string[] args) {
            string[] inputs;
            int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
            int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
            GraphLinks.Size = factoryCount;
            GraphLinks.Links = new int[factoryCount, factoryCount];
            var graph = new Graph();
            for (int i = 0; i < linkCount; i++) {
                inputs = Console.ReadLine().Split(' ');
                int factory1 = int.Parse(inputs[0]);
                int factory2 = int.Parse(inputs[1]);
                int distance = int.Parse(inputs[2]);
                graph.AddLink(factory1, factory2, distance);
            }

            // game loop
            while (true) {
                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
                graph.ClearTroops();
                for (int i = 0; i < entityCount; i++) {
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
                        graph.AddTroop((Side)arg1, arg4, arg2, arg3, arg5);
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

    public static class GraphLinks {
        public static int Size { get; set; }
        public static int[,] Links { get; set; }
    }

    public class Graph {
        public Factory[] Factories { get; private set; }
        public List<Troop> Troops { get; private set; }

        public static Graph GetCopy(Graph obj) {
            var copy = new Graph();
            copy.Factories = obj.Factories.Select(x => new Factory(x.Id) { Income = x.Income, Side = x.Side, TroopsCount = x.TroopsCount }).ToArray();
            copy.Troops = obj.Troops.Select(x => Troop.GetCopy(x)).ToList();
            return copy;
        }

        public Graph(Graph previous) {
            Factories = previous.Factories.ToArray();
            foreach (var factory in Factories.Where(x => x.Side != Side.Neutral)) { factory.TroopsCount += factory.Income; }
            var troops = previous.Troops.Select(x => Troop.GetCopy(x, 1)).ToList();
            Troops = troops.Where(x => x.Remaining > 0).ToList();
            foreach (var troopGroup in troops.Where(x => x.Remaining == 0).GroupBy(x => x.Dst)) {
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
            GraphLinks.Links[f1, f2] = cost;
            GraphLinks.Links[f2, f1] = cost;
        }
    }


    public enum Side {
        MyOwn = 1,
        Neutral = 0,
        Enemy = -1
    }

    public class Troop {
        public Side Side { get; set; }
        public int Size { get; set; }
        public int Src { get; set; }
        public int Dst { get; set; }
        public int Remaining { get; set; }


        public static Troop GetCopy(Troop x, int remainingDecreased = 0) {
            return new Troop { Dst = x.Dst, Side = x.Side, Size = x.Size, Src = x.Src, Remaining = x.Remaining - remainingDecreased };
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

        public IEnumerable<FactoryLink> GetLinks() {
            for (int i = 0; i < GraphLinks.Size; i++) {
                var distance = GraphLinks.Links[Id, i];
                if (Id != i && distance > 0)
                    yield return new FactoryLink { DestinationId = i, Distance = distance };
            }
        }

        public struct FactoryLink {
            public int Distance;
            public int DestinationId;
        }
    }

    public interface IMove {
        int StepsExecution();
        void ChangeGraph(Graph graph);
        string GetConsoleCommand();
    }

    public class MoveTroops : IMove {
        protected Troop Troop;
        public MoveTroops(Troop troop) {
            Troop = troop;
        }
        public string GetConsoleCommand() {
            return string.Format("MOVE {0} {1} {2}", Troop.Src, Troop.Dst, Troop.Size); //MOVE source destination cyborgCount
        }

        public int StepsExecution() {
            return Troop.Remaining;
        }

        public void ChangeGraph(Graph graph) {
            graph.Troops.Add(Troop);
        }

    }

    public class Hold : IMove {
        public void ChangeGraph(Graph graph) { }
        public string GetConsoleCommand() { return "WAIT"; }

        public int StepsExecution() {
            return 1;
        }
    }

    public static class Helper {
        public static float EvalCostFunction(this Graph graph) {
            var factoriesMy = graph.Factories.Where(x => x.Side == Side.MyOwn).ToList();
            var factoriesEnemy = graph.Factories.Where(x => x.Side == Side.Enemy).ToList();

            return factoriesMy.Sum(x => x.EvalMyFactoryCostFunction(graph))
                - factoriesEnemy.Sum(x => (x.Income + 0.01f) * (1 + x.TroopsCount * 0.03f)) // With Neutrals
                ;
        }

        public static float EvalMyFactoryCostFunction(this Factory factory, Graph graph) {
            float result = factory.Income;

            if (factory.GetLinks().All(x => graph.Factories[x.DestinationId].Side == Side.MyOwn || graph.Factories[x.DestinationId].Income == 0))
                result -= 0.5f * Math.Min(1, ((float)factory.TroopsCount) / 30);
            else if (!factory.GetLinks().Any(x => graph.Factories[x.DestinationId].Side == Side.Neutral && graph.Factories[x.DestinationId].Income > 0)) {
                var enemyDistance = factory.GetLinks().Where(x => graph.Factories[x.DestinationId].Side == Side.Enemy).DefaultIfEmpty().Min(x => x.Distance); // TODO: add 2 step compare
                var penalty = factory.TroopsCount * (enemyDistance > 15 ? 0.3f : enemyDistance > 10 ? 0.085f : 0);
                result -= penalty;
            }
            return result;
        }

        public static IMove GetNextBestMove(this Graph graph) {
            var moves = new List<IMove> { new Hold() };
            foreach (var factory in graph.Factories.Where(x => x.TroopsCount > 0 && x.Side == Side.MyOwn)) {
                foreach (var link in factory.GetLinks()) {
                    moves.AddRange(Enumerable.Range(1, factory.TroopsCount).Select(x => new MoveTroops(new Troop {
                        Dst = link.DestinationId,
                        Src = factory.Id,
                        Side = Side.MyOwn,
                        Size = x,
                        Remaining = link.Distance
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

}
