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
    protected Troop Troop;

    public MoveTroops(Troop troop) {
        Troop = troop;
    }

    public string GetConsoleCommand() {
        return string.Format("MOVE {0} {1} {2}", Troop.Src, Troop.Dst, Troop.Size); //MOVE source destination cyborgCount
    }

    public int StepsExecution() {
        return Troop.Remaining;
    }

    public void ChangeGraph(Graph graph) {
        graph.Troops.Add(Troop);
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