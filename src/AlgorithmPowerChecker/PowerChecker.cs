using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlgorithmPowerChecker {
    public class PowerChecker {
        public static int GetPowerResult(string firstParameters, string secondParameters, int competitionsCount) {
            int sum = 0;
            var myCollection = new object();            

            Task[] tasks = new Task[competitionsCount];
            for (int i = 0; i < competitionsCount; i++) {
                tasks[i] = Task.Factory.StartNew(() => {
                    var result = GetFightResult(firstParameters, secondParameters);
                    lock (myCollection) {
                        sum += (int)result;
                        Console.WriteLine(sum);
                    }
                });
            }
            Task.WaitAll(tasks);

            return sum;
        }

        static SingleFightResult GetFightResult(string firstParameters, string secondParameters) {
            using (Process process = new Process()) {
                process.StartInfo.FileName = @"C:\Program Files\Java\jdk1.8.0_201\bin\java.exe";// Properties.Settings.Default.RunnerPath;
                process.StartInfo.Arguments =
                    string.Join(" ",
                    Properties.Settings.Default.RunnerPath,
                    Properties.Settings.Default.AlgorithmExePath,
                    firstParameters,
                    Properties.Settings.Default.AlgorithmExePath,
                    secondParameters
                    );
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                // Synchronously read the standard output of the spawned process. 
                //StreamReader ErrorReader = process.StandardError;
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();
                var lines = output.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );
                if (lines.Length < 2)
                    throw new FormatException("The result can't be recognized. Output: " + output);

                var firstScore  = int.Parse(lines[lines.Length - 2]);
                var secondScore = int.Parse(lines[lines.Length - 1]);// lines.Last();
                // Write the redirected output to this application's window.

                process.WaitForExit();
                return firstScore > secondScore ? SingleFightResult.FirstWin :
                    (firstScore == secondScore ? SingleFightResult.Draw : SingleFightResult.SecondWin);
            }

        }
    }
}
