using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace VPNControl
{
    class ProcessExecutor
    {
        public string output;

        public int Run(string module, string arguments)
        {
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo();
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.FileName = module;
            processInfo.Arguments = arguments;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            output = process.StandardOutput.ReadToEnd();
            output += process.StandardError.ReadToEnd();

            int exitCode = process.ExitCode;

            process.Close();

            return exitCode;
        }
    }
}