using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace VPNControl
{
    public partial class MainForm : Form, StatusListener
    {
        private volatile bool vpn_open = false;
        private IniFile ini_file_;
        private string password;
        private string username;
        private string default_server;
        private string current_server;
        private string default_tooltip = "Disconnected";
        private string connecting_tooltip = "Connecting to ";
        private bool switch_server = false;

        private const int iconsCount = 20;
        private Icon[] icons = new Icon[iconsCount];
        private int currentIcon = 0;
        private volatile bool inProgress = false;
        private OneTimePassword otp;

        private StateMonitor smon;
        
        private static String[] SplitServerName(String srvName)
        {
            List<String> result = new List<String>();

            String[] splitValue = srvName.Split(new Char[] { '/' });
            
            foreach (String element in splitValue) result.Add(element);

            if (splitValue.Length == 1) result.Add("0");

            return result.ToArray();
        }

        public MainForm()
        {
            InitializeComponent();
            ini_file_ = new IniFile();
            ini_file_.Load("VPNControl.ini");

            int checkTimeout;
            smon = Int32.TryParse(ini_file_.GetKeyValue("Options", "checkTimeout"), out checkTimeout) ? 
                new StateMonitor(this, checkTimeout) : new StateMonitor(this);

            smon.Start();
        }

        public void OnConnected()
        {
            if (inProgress) return;

            if (notifyIcon1.Icon != Properties.Resources.vpn_open)
                notifyIcon1.Icon = Properties.Resources.vpn_open;
        }

        public void OnDisconnected()
        {
            if (inProgress) return;

            if (!vpn_open)
            {
                smon.Pause();
                return;
            }
            
            vpn_open = false;

            if (notifyIcon1.Icon != Properties.Resources.vpn_closed)
                notifyIcon1.Icon = Properties.Resources.vpn_closed;

            if(bool.Parse(ini_file_.GetKeyValue("Options", "autoReconnect")))
            {
                exec_vpn(current_server);
            }
        }

        static private System.Diagnostics.ProcessStartInfo build_startinfo(string module, string arguments)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden | System.Diagnostics.ProcessWindowStyle.Minimized;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.FileName = module;
            startInfo.Arguments = arguments;

            return startInfo;
        }

        private void exec_vpn(string vpnserver)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo = build_startinfo("vpncli.exe", !vpn_open ? "-s" : "disconnect");
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(myProcess_Exited);

            inProgress = true;
            smon.Pause();

            //Start progress icons
            currentIcon = 0;
            //Start timer in main thread
            this.Invoke(new MethodInvoker(delegate()
            {
                timer1.Start();
            }));

            //Start process
            process.Start();

            if (!vpn_open)
            {
                String[] srvName = SplitServerName(vpnserver);
                String realName = srvName.Length > 2 ? srvName[2] : vpnserver;
                notifyIcon1.Text = connecting_tooltip + realName;

                process.StandardInput.WriteLine("connect " + srvName[0]);
                process.StandardInput.WriteLine(srvName[1]);
                process.StandardInput.WriteLine(username != null && username.Length != 0 ? username : "");
                process.StandardInput.WriteLine(password);
                process.StandardInput.WriteLine(otp.GetCode().ToString("000000"));
                process.StandardInput.WriteLine();
            }
        }

        // Handle Exited event and display process information.
        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            System.Threading.Thread.Sleep(5000);
            System.Diagnostics.Process.Start(build_startinfo("ipconfig.exe", "/renew"));
            System.Threading.Thread.Sleep(5000);
            inProgress = false;
            //Stop timer in main thread
            this.Invoke(new MethodInvoker(delegate()
            {
                timer1.Stop();
            }));

            if (vpn_open == false)
            {
                //Set icon to 'Connected' state
                notifyIcon1.Icon = Properties.Resources.vpn_open;
                String[] srvName = SplitServerName(current_server);
                String realName = srvName.Length > 2 ? srvName[2] : current_server;
                notifyIcon1.Text = realName;
                vpn_open = true;
                smon.Unpause();
            }
            else
            {
                //Set icon to 'Initial/Disconnected' state
                notifyIcon1.Icon = Properties.Resources.vpn_closed;
                notifyIcon1.Text = default_tooltip;
                vpn_open = false;
                smon.Pause();
                if (switch_server)
                {
                    switch_server = false;
                    exec_vpn(current_server);
                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (inProgress) return;
            if (e.Button != MouseButtons.Left) return;

            switch_server = false;// current_server != default_server;
            current_server = default_server;
            exec_vpn(default_server);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inProgress) return;
            Application.Exit();
        }

        private void item_Click(object sender, EventArgs e)
        {
            if (inProgress) return;
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

            switch_server = current_server != menuItem.Name;
            current_server = menuItem.Name;
            exec_vpn(current_server);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            icons[0]  = Properties.Resources.vpn_progress1;
            icons[1]  = Properties.Resources.vpn_progress2;
            icons[2]  = Properties.Resources.vpn_progress3;
            icons[3]  = Properties.Resources.vpn_progress4;
            icons[4]  = Properties.Resources.vpn_progress5;
            icons[5]  = Properties.Resources.vpn_progress6;
            icons[6]  = Properties.Resources.vpn_progress7;
            icons[7]  = Properties.Resources.vpn_progress8;
            icons[8]  = Properties.Resources.vpn_progress9;
            icons[9]  = Properties.Resources.vpn_progress10;
            icons[10] = Properties.Resources.vpn_progress11;
            icons[11] = Properties.Resources.vpn_progress12;
            icons[12] = Properties.Resources.vpn_progress13;
            icons[13] = Properties.Resources.vpn_progress14;
            icons[14] = Properties.Resources.vpn_progress15;
            icons[15] = Properties.Resources.vpn_progress16;
            icons[16] = Properties.Resources.vpn_progress17;
            icons[17] = Properties.Resources.vpn_progress18;
            icons[18] = Properties.Resources.vpn_progress19;
            icons[19] = Properties.Resources.vpn_progress20;

            IniFile.IniSection sectOptions = ini_file_.GetSection("Options");

            foreach (IniFile.IniSection.IniKey k in sectOptions.Keys)
            {
                if (k.Name.StartsWith("server"))
                {
                    String[] splitValue = SplitServerName(k.Value);

                    ToolStripMenuItem item = new ToolStripMenuItem(splitValue.Length > 2 ? splitValue[2] : k.Value);
                    item.Name = k.Value;

                    item.Click += item_Click;

                    contextMenuStrip1.Items.Insert(0,item);
                }
            }

            password = ini_file_.GetKeyValue("Options", "password");
            username = ini_file_.GetKeyValue("Options", "username");
            default_server = ini_file_.GetKeyValue("Options", "default");
            otp = new OneTimePassword(ini_file_.GetKeyValue("Options", "secret"));
            notifyIcon1.Text = default_tooltip;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            notifyIcon1.Icon = icons[currentIcon];

            currentIcon++;
            if (currentIcon == iconsCount)
                currentIcon = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            smon.Stop();
        }
    }
}