namespace PcmHacking
{
    partial class PcmExplorerMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabs = new System.Windows.Forms.TabControl();
            this.statusTab = new System.Windows.Forms.TabPage();
            this.userLog = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.testPid = new System.Windows.Forms.Button();
            this.pid = new System.Windows.Forms.TextBox();
            this.message = new System.Windows.Forms.TextBox();
            this.sendMessage = new System.Windows.Forms.Button();
            this.dumpRamButton = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.statusTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.statusTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(12, 167);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(730, 271);
            this.tabs.TabIndex = 8;
            // 
            // statusTab
            // 
            this.statusTab.Controls.Add(this.userLog);
            this.statusTab.Location = new System.Drawing.Point(4, 22);
            this.statusTab.Name = "statusTab";
            this.statusTab.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.statusTab.Size = new System.Drawing.Size(722, 245);
            this.statusTab.TabIndex = 0;
            this.statusTab.Text = "Status";
            this.statusTab.UseVisualStyleBackColor = true;
            // 
            // userLog
            // 
            this.userLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userLog.Location = new System.Drawing.Point(3, 3);
            this.userLog.Multiline = true;
            this.userLog.Name = "userLog";
            this.userLog.ReadOnly = true;
            this.userLog.Size = new System.Drawing.Size(716, 239);
            this.userLog.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 22);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.debugTab.Size = new System.Drawing.Size(722, 245);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLog.Location = new System.Drawing.Point(3, 3);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugLog.Size = new System.Drawing.Size(716, 239);
            this.debugLog.TabIndex = 0;
            // 
            // deviceDescription
            // 
            this.deviceDescription.AutoSize = true;
            this.deviceDescription.Location = new System.Drawing.Point(234, 18);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(88, 13);
            this.deviceDescription.TabIndex = 7;
            this.deviceDescription.Text = "[selected device]";
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(12, 12);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(216, 25);
            this.selectButton.TabIndex = 6;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // testPid
            // 
            this.testPid.Location = new System.Drawing.Point(12, 43);
            this.testPid.Name = "testPid";
            this.testPid.Size = new System.Drawing.Size(216, 25);
            this.testPid.TabIndex = 9;
            this.testPid.Text = "&Test Pid";
            this.testPid.UseVisualStyleBackColor = true;
            this.testPid.Click += new System.EventHandler(this.testPid_Click);
            // 
            // pid
            // 
            this.pid.Location = new System.Drawing.Point(234, 46);
            this.pid.Name = "pid";
            this.pid.Size = new System.Drawing.Size(126, 20);
            this.pid.TabIndex = 10;
            // 
            // message
            // 
            this.message.Location = new System.Drawing.Point(234, 78);
            this.message.Multiline = true;
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(342, 103);
            this.message.TabIndex = 12;
            // 
            // sendMessage
            // 
            this.sendMessage.Location = new System.Drawing.Point(12, 75);
            this.sendMessage.Name = "sendMessage";
            this.sendMessage.Size = new System.Drawing.Size(216, 25);
            this.sendMessage.TabIndex = 11;
            this.sendMessage.Text = "&Send Message";
            this.sendMessage.UseVisualStyleBackColor = true;
            this.sendMessage.Click += new System.EventHandler(this.sendMessage_Click);
            // 
            // dumpRamButton
            // 
            this.dumpRamButton.Location = new System.Drawing.Point(12, 106);
            this.dumpRamButton.Name = "dumpRamButton";
            this.dumpRamButton.Size = new System.Drawing.Size(216, 25);
            this.dumpRamButton.TabIndex = 13;
            this.dumpRamButton.Text = "&Dump RAM";
            this.dumpRamButton.UseVisualStyleBackColor = true;
            this.dumpRamButton.Click += new System.EventHandler(this.dumpRamButton_Click);
            // 
            // PcmExplorerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 450);
            this.Controls.Add(this.dumpRamButton);
            this.Controls.Add(this.message);
            this.Controls.Add(this.sendMessage);
            this.Controls.Add(this.pid);
            this.Controls.Add(this.testPid);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.deviceDescription);
            this.Controls.Add(this.selectButton);
            this.Name = "PcmExplorerMainForm";
            this.Text = "PCM Explorer";
            this.Load += new System.EventHandler(this.PcmExplorerMainForm_Load);
            this.tabs.ResumeLayout(false);
            this.statusTab.ResumeLayout(false);
            this.statusTab.PerformLayout();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage statusTab;
        private System.Windows.Forms.TextBox userLog;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox debugLog;
        private System.Windows.Forms.Label deviceDescription;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button testPid;
        private System.Windows.Forms.TextBox pid;
        private System.Windows.Forms.TextBox message;
        private System.Windows.Forms.Button sendMessage;
        private System.Windows.Forms.Button dumpRamButton;
    }
}

