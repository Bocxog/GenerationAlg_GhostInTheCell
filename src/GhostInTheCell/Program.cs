using System;using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.    
 **/
class Player
{
    static void Main(string[] args) {

        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories

        if (args.Any())
            GlobalConfig.FillGlobalConstants(args);
        else if (factoryCount <= 9) {
            GlobalConfig.FillGlobalConstants(new[] { "17.5725|47.1822|28.1432|23.7933|64.3886|12.7707|36.8099|11.6059" });
        } else if (factoryCount <= 11) {
            GlobalConfig.FillGlobalConstants(new[] { "-8.397|78.3473|71.5912|50.429|19.2747|85.19|77.1327|72.7151" });
        } else
            GlobalConfig.FillGlobalConstants(new[] { "1.633|15.7159|25.474|88.3882|75.0557|9.5088|81.0026|83.2856" });

        GraphLinks.Size = factoryCount;
        GraphLinks.Links = new GraphLinks.ShortestPath[factoryCount, factoryCount];
        var graph = new Graph();
        for (int i = 0; i < linkCount; i++) {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);
            graph.AddLink(factory1, factory2, distance);
        }

        {
            for (int i = 0; i < factoryCount; i++) {
                for (int j = 0; j < factoryCount; j++) {
                    if (i != j && GraphLinks.Links[i, j].Distance == 0)
                        GraphLinks.Links[i, j].PathType = GraphLinks.PathType.NotConnected;
                }
            }

            var addedLink = false;
            do {
                for (int i = 0; i < factoryCount; i++) {
                    for (int j = 0; j < factoryCount; j++) {
                        if (i != j && GraphLinks.Links[i, j].PathType == GraphLinks.PathType.NotConnected) {
                            var minDist = 0;
                            var minFactory = -1;
                            for (int k = 0; k < factoryCount; k++) {
                                if (k != i && k != j &&
                                    GraphLinks.Links[i, k].PathType != GraphLinks.PathType.NotConnected &&
                                    GraphLinks.Links[k, j].PathType != GraphLinks.PathType.NotConnected) {
                                    var dist = GraphLinks.Links[i, k].Distance + GraphLinks.Links[k, j].Distance;
                                    if (minDist == 0 || minDist > dist) {
                                        minDist = dist;
                                        minFactory = k;
                                    }

                                }
                            }

                            if (minDist != 0) {
                                addedLink = true;
                                GraphLinks.Links[i, j] = new GraphLinks.ShortestPath { Distance = minDist, PathType = GraphLinks.PathType.WithMiddle, FirstFactoryId = GraphLinks.Links[i, minFactory].FirstFactoryId };
                                GraphLinks.Links[j, i] = new GraphLinks.ShortestPath { Distance = minDist, PathType = GraphLinks.PathType.WithMiddle, FirstFactoryId = GraphLinks.Links[j, minFactory].FirstFactoryId };
                            }
                        }
                    }
                }

            } while (addedLink);
            GraphLinks.MaxDistance = 0;
            for (int i = 0; i < factoryCount; i++) 
                for (int j = 0; j < factoryCount; j++)
                    if (GraphLinks.Links[i, j].PathType != GraphLinks.PathType.NotConnected)
                        GraphLinks.MaxDistance = Math.Max(GraphLinks.MaxDistance, GraphLinks.Links[i, j].Distance);
        }

        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            graph.ClearMovedEntities();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);

                if (entityType == "FACTORY") {
                    graph.RefreshFactoryInfo(entityId, arg1, arg3, arg2, arg4);
                }
                else if (entityType == "TROOP")
                {
                    graph.AddTroop((Side)arg1, arg4, arg2, arg3, arg5);
                }
                else if (entityType == "BOMB") {
                    graph.AddBomb((Side)arg1, arg2, arg3, arg4);
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine(graph.GetNextBestMove());

            graph.CurrentGameTick++;
        }
    }
}