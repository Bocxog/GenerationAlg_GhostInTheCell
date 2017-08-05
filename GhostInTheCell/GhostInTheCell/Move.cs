using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IMove {
    int StepsExecution();
    void ChangeGraph(Graph graph);
    string GetConsoleCommand();
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
        return Troops.Max(x=> x.Remaining);
    }

    public void ChangeGraph(Graph graph) {
        graph.Troops.AddRange(Troops);
    }
}

public class Hold : IMove {
    public void ChangeGraph(Graph graph) {}

    public string GetConsoleCommand() {
        return "WAIT";
    }

    public int StepsExecution() {
        return 1;
    }
}