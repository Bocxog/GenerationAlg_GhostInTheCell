using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmPowerChecker {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Please fill a parameters.");
                Console.ReadKey();
                return;
            }

            var path1 = args[0];
            var path2 = args[1];

            var first  = new AlgorithmSettings { PathToExecutable = path1 };
            var second = new AlgorithmSettings { PathToExecutable = path2 };

            var result = GetFightResult(first, second);

            Console.WriteLine(result);

            Console.Read();
        }

        static SingleFightResult GetFightResult(AlgorithmSettings first, AlgorithmSettings second) {
            using (Process process = new Process()) {
                process.StartInfo.FileName = Properties.Settings.Default.RunnerPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Synchronously read the standard output of the spawned process. 
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                // Write the redirected output to this application's window.
                Console.WriteLine(output);

                process.WaitForExit();
            }

            return SingleFightResult.Draw;
        }
    }
}
