using System;using System.Collections.Generic;using System.Linq;


public interface IMove {
    int StepsExecution();
    void ChangeGraph(Graph graph);
    string GetConsoleCommand();

    IMove GetCopy();
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

    public int GetBombMoves() {
        return moves.Count(x => x is SendBomb);
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

    public int StepsExecution() {return 12;}

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



public class SendBomb : IMove {
    protected ICollection<Bomb> Bombs;

    public SendBomb() {
        Bombs = new List<Bomb>();
    }

    public SendBomb(Bomb bomb) {
        Bombs = new List<Bomb> {bomb};
    }

    public SendBomb(IEnumerable<Bomb> bombs) {
        Bombs = bombs.ToList();
    }

    //public void AddTroop(Troop troop) { Bombs.Add(troop);}

    public string GetConsoleCommand() {
        return string.Join(";", Bombs.Select(x => string.Format("BOMB {0} {1}", x.Src, x.DstInCommand ?? x.Dst))); //BOMB source destination
    }

    public IMove GetCopy() {
        return new SendBomb(Bombs.Select(x=> Bomb.GetCopy(x)));
    }

    public int StepsExecution() {
        return Bombs.Any() ? Bombs.Max(x => x.Remaining) + Constants.BOMB_EXPLODE_DURATION : 0;
    }

    public void ChangeGraph(Graph graph) {        
        var bombsToGraph = Bombs.Select(x => Bomb.GetCopy(x)).ToList();
        graph.Bombs.AddRange(bombsToGraph);

        //graph.BombsAvailable_Enemy -= bombsToGraph.Count(x => x.Side == Side.Enemy);
        graph.BombsAvailable_MyOwn -= bombsToGraph.Count(x => x.Side == Side.MyOwn);
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