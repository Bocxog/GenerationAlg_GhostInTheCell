using System;using System.Linq;

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
        //LogReportLevel.EachMoveCost |
        //LogReportLevel.InnerInlineMoveCost |
        //LogReportLevel.BestMoveCost |
        0;
}