﻿using System;using System.Collections.Generic;using System.Linq;


public interface IJob {
    void EvaluateInnerState(Graph graphToCopy);
    int GetPriorityValue();
    IMove GetMove();
}

public class AtackFactory : IJob {
    private DecisionHelper.FactoryStates FactoryState;
    private int StepsToPredict;
    public AtackFactory(DecisionHelper.FactoryStates factoryState, int stepsToPredict) {
        FactoryState = factoryState;
        StepsToPredict = stepsToPredict;
    }

    private Graph graph = null;
    private IMove move = null;
    private int priorityValue = int.MinValue;
    public void EvaluateInnerState(Graph graphToCopy) {
        graph = Graph.GetCopy(graphToCopy);
        move = EvaluateMove();
    }
    public int GetPriorityValue() {return priorityValue;}

    public IMove GetMove() {
        return move;
    }

    private IMove EvaluateMove() {
//        throw new NotImplementedException();
        var multiMove = new MultiMove();
        multiMove.AddMove(new Message($"Fight for {FactoryState.FactoryId}"));
        var moveTroops = new MoveTroops();
        multiMove.AddMove(moveTroops);
        for (int i = 0; i < StepsToPredict; i++) {
            //взять  мои фабрики на этом расстоянии
            var myFactoriesForAttack = graph.Factories[FactoryState.FactoryId].GetLinks()
                .Where(x => x.Distance <= i && graph.Factories[x.DestinationId].Side == Side.MyOwn)
                .Select(x => graph.Factories[x.DestinationId])
                .ToList();
            //если могут захватить - ура
            if (myFactoriesForAttack.Any() && myFactoriesForAttack.Sum(x => x.TroopsCanBeUsed) > FactoryState.Enemies[i]) {
                int troopsUsed, needed;
                needed = troopsUsed = FactoryState.Enemies[i] ?? int.MaxValue;
                foreach (var myFactory in myFactoriesForAttack) {
//TODO: sort factories by distance or smth else
                    var sendedTroops = Math.Min(needed + 1, myFactory.TroopsCanBeUsed); // TODO: захват нейтралов совместный. +1 не совсем точно. нужно только для захвата но не деф.
                    myFactory.TroopsCanBeUsed =
                        graph.Factories[myFactory.Id].TroopsCanBeUsed = myFactory.TroopsCanBeUsed - sendedTroops;

                    var shortestPath = GraphLinks.Links[myFactory.Id, FactoryState.FactoryId];
                    moveTroops.AddTroop(new Troop {
                        Dst = FactoryState.FactoryId,
                        DstInCommand = shortestPath.FirstFactoryId,
                        Side = Side.MyOwn,
                        Remaining = shortestPath.Distance + 1,
                        Src = myFactory.Id,
                        Size = sendedTroops
                    });

                    needed -= sendedTroops;
                    if (needed <= 0)
                        break;
                }

                priorityValue =
                    5*graph.Factories[FactoryState.FactoryId].Income*(graph.Factories[FactoryState.FactoryId].Side == Side.Neutral ? 1 : 2)
                    - troopsUsed/10
                    - (int) Math.Pow((int) (i/5), 2);

                break;
            }
        }
        return multiMove;
    }
}
public class UpgradeFactoryJob : IJob {
    private int FactoryId;
    private MultiMove move;
    public UpgradeFactoryJob(int factoryId) {
        FactoryId = factoryId;
        move = new MultiMove();
        move.AddMove(new UpgradeFactory(FactoryId));
    }

    public void EvaluateInnerState(Graph graphToCopy) { }
    public int GetPriorityValue() { return 10; }

    public IMove GetMove() {
        return move;
    }
}