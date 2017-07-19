using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

