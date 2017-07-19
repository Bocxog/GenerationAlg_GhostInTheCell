public static class GraphLinks {
    public static int Size { get; set; }
    public static int[,] Links { get; set; }
}

public enum Side {
    MyOwn = 1,
    Neutral = 0,
    Enemy = -1
}
