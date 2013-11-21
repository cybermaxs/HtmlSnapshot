using System;
using System.Diagnostics;
using System.Text;

namespace HtmlSnapshot
{
    public class Command
    {
        /// <summary>
        /// Class used to store string data.
        /// </summary>
        public class DataEventArgs : EventArgs
        {
            public string Data { get; private set; }

            public DataEventArgs(string data)
            {
                Data = data;
            }
        }

        // Process object used to run command.
        private Process _process;

        // Process start info.
        private ProcessStartInfo _startInfo;

        // Stores the contents of standard output.
        private StringBuilder _standardOutput;

        // Stores the contents of standard error.
        private StringBuilder _standardError;

        /// <summary>
        /// No timeout.
        /// </summary>
        public const int NoTimeOut = 0;

        /// <summary>
        /// Value that indicates whether process is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Value that indicates whether process has exited.
        /// </summary>
        public bool HasExited { get; private set; }

        /// <summary>
        /// Value that indicates whether process has timed out..
        /// </summary>
        public bool HasTimedOut { get; private set; }

        /// <summary>
        /// Process ID of the running command.
        /// </summary>
        public int ProcessId { get; private set; }

        /// <summary>
        /// Exit code of process. Only set if HasExited is True.
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Standard output of command.
        /// </summary>
        public string StandardOutput
        {
            get
            {
                return _standardOutput.ToString();
            }
        }

        /// <summary>
        /// Standard error of cmomand.
        /// </summary>
        public string StandardError
        {
            get
            {
                return _standardError.ToString();
            }
        }

        /// <summary>
        /// Raised when standard output receives data.
        /// </summary>
        public event EventHandler<DataEventArgs> OutputDataReceived = (sender, args) => { };

        /// <summary>
        /// Raised when standard error receievs data.
        /// </summary>
        public event EventHandler<DataEventArgs> ErrorDataReceived = (sender, args) => { };

        /// <summary>
        /// Raised when process has exited.
        /// </summary>
        public event EventHandler Exited = (sender, args) => { };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exe">Command to run.</param>
        /// <param name="arguments">Arguments to pass to exe.</param>
        /// <param name="workingDirectory">Working directory to run command in.</param>
        public Command(string exe, string arguments = "", string workingDirectory = "")
        {
            _standardOutput = new StringBuilder();
            _standardError = new StringBuilder();

            _startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,                // This is required to redirect stdin, stdout and stderr
                CreateNoWindow = true,                  // Don't create a window
                RedirectStandardOutput = true,          // Capture standard output
                RedirectStandardError = true,           // Capture standard error
                RedirectStandardInput = true,           // Enable sending commands to standard input
            };

            _process = new Process()
            {
                StartInfo = _startInfo,
                EnableRaisingEvents = true,
            };

            _process.OutputDataReceived += _process_OutputDataReceived;
            _process.ErrorDataReceived += _process_ErrorDataReceived;
            _process.Exited += _process_Exited;
        }

        /// <summary>
        /// Run command synchronously.
        /// </summary>
        /// <param name="timeOutInMilliseconds">Timeout value in milliseconds (default is infinite timeout).</param>
        public void Run(int timeOutInMilliseconds = NoTimeOut)
        {
            if (!IsRunning && !HasExited)
            {
                BeginRun();

                if (timeOutInMilliseconds == NoTimeOut)
                {
                    _process.WaitForExit();
                }
                else
                {
                    this.HasTimedOut = !_process.WaitForExit(timeOutInMilliseconds);
                    if (this.HasTimedOut)
                    {
                        this.Kill();
                        this.ExitCode = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Run command asynchronously.
        /// </summary>
        private void BeginRun()
        {
            if (!IsRunning && !HasExited)
            {
                if (_process.Start())
                {
                    IsRunning = true;
                    ProcessId = _process.Id;

                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                }
            }
        }

        /// <summary>
        /// Kill process.
        /// </summary>
        public void Kill()
        {
            if (IsRunning && !HasExited)
            {
                // Only kill this process
                _process.Kill();
            }
        }

        // Handler for OutputDataReceived event of process.
        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _standardOutput.Append(e.Data);

            OutputDataReceived(this, new DataEventArgs(e.Data));
        }

        // Handler for ErrorDataReceived event of process.
        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _standardError.Append(e.Data);

            ErrorDataReceived(this, new DataEventArgs(e.Data));
        }

        // Handler for Exited event of process.
        private void _process_Exited(object sender, EventArgs e)
        {
            HasExited = true;
            IsRunning = false;
            ExitCode = _process.ExitCode;
            Exited(this, e);
        }

    }
}