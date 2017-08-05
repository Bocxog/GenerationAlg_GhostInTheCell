public static class GraphLinks {
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
