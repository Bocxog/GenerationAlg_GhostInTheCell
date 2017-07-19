using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

