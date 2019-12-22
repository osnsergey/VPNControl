using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace VPNControl
{
    class StateMonitor
    {
        private Thread thread_;
        private StatusListener listener_;
        private int check_timeout_ = 60000;
        private volatile bool paused_ = false;

        public StateMonitor(StatusListener listener)
        {
            listener_ = listener;
            thread_ = new Thread(new ThreadStart(this.RunThread));
        }

        public StateMonitor(StatusListener listener, int check_timeout)
        {
            check_timeout_ = check_timeout;
            listener_ = listener;
            thread_ = new Thread(new ThreadStart(this.RunThread));
        }

        public void Start()
        {
            thread_.Start();
        }

        // Override in base class
        public void RunThread()
        {
            while (true)
            {
                if(!paused_) CheckStatus();
                Thread.Sleep(check_timeout_);
            }
        }

        public void Stop()
        {
            thread_.Abort();
        }

        public void Pause()
        {
            paused_ = true;
        }

        public void Unpause()
        {
            paused_ = false;
        }

        private void CheckStatus()
        {
            if (listener_ == null) return;

            ProcessExecutor process = new ProcessExecutor();
            process.Run("vpncli.exe", "status");

            if (process.output.Contains("state: Connected"))
                listener_.OnConnected();
            else
            if (process.output.Contains("state: Disconnected"))
                listener_.OnDisconnected();
        }
    }
}
