using System;using System.Collections.Generic;

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
}

public class Bomb
{
    public Side Side { get; set; }
    public int Src { get; set; }
    public int Dst { get; set; }
    public int? DstInCommand { get; set; }
    public int Remaining { get; set; }

    internal static Bomb GetCopy(Bomb src)
    {
        return new Bomb { Dst = src.Dst, DstInCommand = src.DstInCommand, Side = src.Side, Src = src.Src, Remaining = src.Remaining };
    }
}

public class Factory
{
    public Factory(int id)
    {
        Id = id;
    }
    public int Id { get; set; }
    public int Income { get; set; }
    public Side Side { get; set; }
    public int TroopsCount { get; set; }
    public int InactivityDaysLeft { get; set; } /*Due to bomb explosion*/

    public int TroopsCanBeUsed { get; set; }

    public IEnumerable<FactoryLink> GetLinks()
    {
        for (int i = 0; i < GraphLinks.Size; i++)
        {
            var path = GraphLinks.Links[Id, i];
            if (Id != i && path.PathType != GraphLinks.PathType.NotConnected)
                yield return new FactoryLink { DestinationId = i, Distance = path.Distance, FirstFactoryId = path.FirstFactoryId, PathType = path.PathType };
        }
    }

    public struct FactoryLink
    {
        public int Distance;
        public int DestinationId;
        public int FirstFactoryId;
        public GraphLinks.PathType PathType;
    }
}


