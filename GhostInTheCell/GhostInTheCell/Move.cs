using System.Collections.Generic;using System.Linq;


public interface IMove {
    int StepsExecution();
    void ChangeGraph(Graph graph);
    string GetConsoleCommand();
}

public class MultiMove : IMove {
    protected ICollection<IMove> moves;

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

    public int StepsExecution() {
        return Troops.Any() ? Troops.Max(x => x.Remaining) : 0;
    }

    public void ChangeGraph(Graph graph) {
        graph.Troops.AddRange(Troops);
    }
}

public class Hold : IMove {
    public void ChangeGraph(Graph graph) {}

    public virtual string GetConsoleCommand() {
        return "WAIT";
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

    public override string GetConsoleCommand() {
        return "MSG " + _message;
    }
}