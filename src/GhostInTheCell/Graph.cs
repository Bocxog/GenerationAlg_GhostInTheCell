using System;using System.Collections.Generic;using System.Linq;

public class GraphEstimator : Graph {}

public class Graph {
    public Factory[] Factories { get; private set; }
    public List<Troop> Troops { get; private set; }
    public List<Bomb> Bombs { get; private set; }
    public int BombsAvailable_MyOwn { get; set; }
    public int BombsAvailable_Enemy { get; set; }
    public int CurrentGameTick { get; set; }

    public static Graph GetCopy(Graph obj) {
        var copy = new Graph();
        copy.Factories = obj.Factories.Select(x => new Factory(x.Id) { Income = x.Income, Side = x.Side, TroopsCount = x.TroopsCount, TroopsCanBeUsed = x.TroopsCanBeUsed, InactivityDaysLeft = x.InactivityDaysLeft}).ToArray();
        copy.Troops = obj.Troops.Select(x => Troop.GetCopy(x)).ToList();
        copy.Bombs = obj.Bombs.Select(x => Bomb.GetCopy(x)).ToList();
        copy.BombsAvailable_Enemy = obj.BombsAvailable_Enemy;
        copy.BombsAvailable_MyOwn = obj.BombsAvailable_MyOwn;

        copy.CurrentGameTick = obj.CurrentGameTick;
        return copy;
    }

    /// <summary>
    /// Game Turn
    ///     One game turn is computed as follows:
    ///          Move existing troops and bombs
    ///          Execute user orders
    ///          Produce new cyborgs in all factories
    ///          Solve battles
    ///          Make the bombs explode
    ///          Check end conditions
    /// </summary>
    public void DoNextMove() {
        foreach (var factory in Factories.Where(x => x.Side != Side.Neutral)) {
            if (factory.InactivityDaysLeft <= 0) factory.TroopsCount += factory.Income;
            else factory.InactivityDaysLeft--;
        }

        foreach (var troop in Troops) {troop.Remaining--;}
        foreach (var bomb in Bombs)   {bomb.Remaining--;}

        //Solve battles
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

        //Bomb explode
        foreach (var bomb in Bombs.Where(x => x.Remaining == 0)) {
            var factory = Factories[bomb.Dst];

            factory.TroopsCount -= Math.Max(10, factory.TroopsCount / 2);


        }

        Bombs.RemoveAll(bomb => {
            if (bomb.Remaining > 0) return false;

            var factory = Factories[bomb.Dst];
            factory.TroopsCount -= Math.Min(factory.TroopsCount, Math.Max(10, factory.TroopsCount / 2));
            factory.InactivityDaysLeft = Constants.BOMB_EXPLODE_DURATION;

            return true;
        });
        //

        Troops = Troops.Where(x => x.Remaining > 0).ToList();
        CurrentGameTick++;

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
        Bombs = new List<Bomb>();
        BombsAvailable_Enemy = 2;
        BombsAvailable_MyOwn = 2;
    }

    public void ClearMovedEntities() {
        Troops.Clear();
        Bombs.Clear();
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

    public void AddBomb(Side side, int src, int dst, int remaining) {
        if (side == Side.MyOwn)
            Bombs.Add(new Bomb
            {
                Side = side,
                Dst = dst,
                Remaining = remaining,
                Src = src
            });
    }

    public int GetMaxCountSteps() {
        return
            Math.Max(
                Troops.Any() ? Troops.Max(x => x.Remaining) : 0,
                Bombs.Any() ? Bombs.Max(x => x.Remaining) + Constants.BOMB_EXPLODE_DURATION : 0
            );
    }

    protected Factory GetFactory(int idx) {
        var fact = Factories[idx];
        if (fact != null)
            return fact;
        fact = new Factory(idx);
        Factories[idx] = fact;
        return fact;
    }

    public void RefreshFactoryInfo(int id, int side, int income, int troopsCount, int bombDaysLeft) {
        var f = Factories[id];
        f.Income = income;
        f.Side = (Side) side;
        f.TroopsCount = troopsCount;
        f.InactivityDaysLeft = bombDaysLeft;
    }

    public void AddLink(int f1, int f2, int cost) {
        var Fact1 = GetFactory(f1);
        var Fact2 = GetFactory(f2);
        GraphLinks.Links[f1, f2] = new GraphLinks.ShortestPath { Distance = cost, FirstFactoryId = f2, PathType = GraphLinks.PathType.Direct };
        GraphLinks.Links[f2, f1] = new GraphLinks.ShortestPath { Distance = cost, FirstFactoryId = f1, PathType = GraphLinks.PathType.Direct };
    }
}

