namespace PcmHacking
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.interfaceBox = new System.Windows.Forms.GroupBox();
            this.reinitializeButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.operationsBox = new System.Windows.Forms.GroupBox();
            this.writeParametersButton = new System.Windows.Forms.Button();
            this.testWriteButton = new System.Windows.Forms.Button();
            this.quickComparisonButton = new System.Windows.Forms.Button();
            this.writeOsAndCalibration = new System.Windows.Forms.Button();
            this.exitKernelButton = new System.Windows.Forms.Button();
            this.writeFullContentsButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.writeCalibrationButton = new System.Windows.Forms.Button();
            this.readFullContentsButton = new System.Windows.Forms.Button();
            this.modifyVinButton = new System.Windows.Forms.Button();
            this.readPropertiesButton = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.resultsTab = new System.Windows.Forms.TabPage();
            this.userLog = new System.Windows.Forms.TextBox();
            this.helpTab = new System.Windows.Forms.TabPage();
            this.helpWebBrowser = new System.Windows.Forms.WebBrowser();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.releasedUnder = new System.Windows.Forms.Label();
            this.releasedAt = new System.Windows.Forms.LinkLabel();
            this.releasedByAntus = new System.Windows.Forms.Label();
            this.releasedAtPcmHacking = new System.Windows.Forms.LinkLabel();
            this.releasedEtc = new System.Windows.Forms.Label();
            this.interfaceBox.SuspendLayout();
            this.operationsBox.SuspendLayout();
            this.tabs.SuspendLayout();
            this.resultsTab.SuspendLayout();
            this.helpTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // interfaceBox
            // 
            this.interfaceBox.Controls.Add(this.reinitializeButton);
            this.interfaceBox.Controls.Add(this.selectButton);
            this.interfaceBox.Controls.Add(this.deviceDescription);
            this.interfaceBox.Location = new System.Drawing.Point(9, 10);
            this.interfaceBox.Margin = new System.Windows.Forms.Padding(2);
            this.interfaceBox.Name = "interfaceBox";
            this.interfaceBox.Padding = new System.Windows.Forms.Padding(2);
            this.interfaceBox.Size = new System.Drawing.Size(224, 128);
            this.interfaceBox.TabIndex = 0;
            this.interfaceBox.TabStop = false;
            this.interfaceBox.Text = "Device";
            // 
            // reinitializeButton
            // 
            this.reinitializeButton.Location = new System.Drawing.Point(4, 96);
            this.reinitializeButton.Margin = new System.Windows.Forms.Padding(2);
            this.reinitializeButton.Name = "reinitializeButton";
            this.reinitializeButton.Size = new System.Drawing.Size(216, 25);
            this.reinitializeButton.TabIndex = 2;
            this.reinitializeButton.Text = "Re-&Initialize Device";
            this.reinitializeButton.UseVisualStyleBackColor = true;
            this.reinitializeButton.Click += new System.EventHandler(this.reinitializeButton_Click);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(4, 66);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(216, 25);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.Location = new System.Drawing.Point(4, 30);
            this.deviceDescription.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(214, 13);
            this.deviceDescription.TabIndex = 0;
            this.deviceDescription.Text = "Device name will be displayed here";
            // 
            // operationsBox
            // 
            this.operationsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.operationsBox.Controls.Add(this.writeParametersButton);
            this.operationsBox.Controls.Add(this.testWriteButton);
            this.operationsBox.Controls.Add(this.quickComparisonButton);
            this.operationsBox.Controls.Add(this.writeOsAndCalibration);
            this.operationsBox.Controls.Add(this.exitKernelButton);
            this.operationsBox.Controls.Add(this.writeFullContentsButton);
            this.operationsBox.Controls.Add(this.cancelButton);
            this.operationsBox.Controls.Add(this.writeCalibrationButton);
            this.operationsBox.Controls.Add(this.readFullContentsButton);
            this.operationsBox.Controls.Add(this.modifyVinButton);
            this.operationsBox.Controls.Add(this.readPropertiesButton);
            this.operationsBox.Location = new System.Drawing.Point(9, 153);
            this.operationsBox.Margin = new System.Windows.Forms.Padding(2);
            this.operationsBox.Name = "operationsBox";
            this.operationsBox.Padding = new System.Windows.Forms.Padding(2);
            this.operationsBox.Size = new System.Drawing.Size(224, 382);
            this.operationsBox.TabIndex = 1;
            this.operationsBox.TabStop = false;
            this.operationsBox.Text = "Operations";
            // 
            // writeParametersButton
            // 
            this.writeParametersButton.Location = new System.Drawing.Point(4, 194);
            this.writeParametersButton.Margin = new System.Windows.Forms.Padding(2);
            this.writeParametersButton.Name = "writeParametersButton";
            this.writeParametersButton.Size = new System.Drawing.Size(216, 25);
            this.writeParametersButton.TabIndex = 6;
            this.writeParametersButton.Text = "Write Parameters (Clone)";
            this.writeParametersButton.UseVisualStyleBackColor = true;
            this.writeParametersButton.Click += new System.EventHandler(this.writeParametersButton_Click);
            // 
            // testWriteButton
            // 
            this.testWriteButton.Location = new System.Drawing.Point(4, 136);
            this.testWriteButton.Margin = new System.Windows.Forms.Padding(2);
            this.testWriteButton.Name = "testWriteButton";
            this.testWriteButton.Size = new System.Drawing.Size(216, 25);
            this.testWriteButton.TabIndex = 4;
            this.testWriteButton.Text = "&Test Write";
            this.testWriteButton.UseVisualStyleBackColor = true;
            this.testWriteButton.Click += new System.EventHandler(this.testWriteButton_Click);
            // 
            // quickComparisonButton
            // 
            this.quickComparisonButton.Location = new System.Drawing.Point(4, 107);
            this.quickComparisonButton.Margin = new System.Windows.Forms.Padding(2);
            this.quickComparisonButton.Name = "quickComparisonButton";
            this.quickComparisonButton.Size = new System.Drawing.Size(216, 25);
            this.quickComparisonButton.TabIndex = 3;
            this.quickComparisonButton.Text = "&Quick Comparison";
            this.quickComparisonButton.UseVisualStyleBackColor = true;
            this.quickComparisonButton.Click += new System.EventHandler(this.quickComparisonButton_Click);
            // 
            // writeOsAndCalibration
            // 
            this.writeOsAndCalibration.Location = new System.Drawing.Point(4, 223);
            this.writeOsAndCalibration.Margin = new System.Windows.Forms.Padding(2);
            this.writeOsAndCalibration.Name = "writeOsAndCalibration";
            this.writeOsAndCalibration.Size = new System.Drawing.Size(216, 25);
            this.writeOsAndCalibration.TabIndex = 7;
            this.writeOsAndCalibration.Text = "Write &OS && Calibration";
            this.writeOsAndCalibration.UseVisualStyleBackColor = true;
            this.writeOsAndCalibration.Visible = false;
            this.writeOsAndCalibration.Click += new System.EventHandler(this.writeOsAndCalibration_Click);
            // 
            // exitKernelButton
            // 
            this.exitKernelButton.Location = new System.Drawing.Point(4, 301);
            this.exitKernelButton.Margin = new System.Windows.Forms.Padding(2);
            this.exitKernelButton.Name = "exitKernelButton";
            this.exitKernelButton.Size = new System.Drawing.Size(216, 25);
            this.exitKernelButton.TabIndex = 9;
            this.exitKernelButton.Text = "&Halt Kernel";
            this.exitKernelButton.UseVisualStyleBackColor = true;
            this.exitKernelButton.Click += new System.EventHandler(this.testKernelButton_Click);
            // 
            // writeFullContentsButton
            // 
            this.writeFullContentsButton.Location = new System.Drawing.Point(4, 252);
            this.writeFullContentsButton.Margin = new System.Windows.Forms.Padding(2);
            this.writeFullContentsButton.Name = "writeFullContentsButton";
            this.writeFullContentsButton.Size = new System.Drawing.Size(216, 25);
            this.writeFullContentsButton.TabIndex = 8;
            this.writeFullContentsButton.Text = "Write &Full Contents";
            this.writeFullContentsButton.UseVisualStyleBackColor = true;
            this.writeFullContentsButton.Visible = false;
            this.writeFullContentsButton.Click += new System.EventHandler(this.writeFullContentsButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.Location = new System.Drawing.Point(4, 349);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(216, 25);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // writeCalibrationButton
            // 
            this.writeCalibrationButton.Location = new System.Drawing.Point(4, 165);
            this.writeCalibrationButton.Margin = new System.Windows.Forms.Padding(2);
            this.writeCalibrationButton.Name = "writeCalibrationButton";
            this.writeCalibrationButton.Size = new System.Drawing.Size(216, 25);
            this.writeCalibrationButton.TabIndex = 5;
            this.writeCalibrationButton.Text = "&Write Calibration";
            this.writeCalibrationButton.UseVisualStyleBackColor = true;
            this.writeCalibrationButton.Click += new System.EventHandler(this.writeCalibrationButton_Click);
            // 
            // readFullContentsButton
            // 
            this.readFullContentsButton.Location = new System.Drawing.Point(4, 48);
            this.readFullContentsButton.Margin = new System.Windows.Forms.Padding(2);
            this.readFullContentsButton.Name = "readFullContentsButton";
            this.readFullContentsButton.Size = new System.Drawing.Size(216, 25);
            this.readFullContentsButton.TabIndex = 1;
            this.readFullContentsButton.Text = "&Read Full Contents";
            this.readFullContentsButton.UseVisualStyleBackColor = true;
            this.readFullContentsButton.Click += new System.EventHandler(this.readFullContentsButton_Click);
            // 
            // modifyVinButton
            // 
            this.modifyVinButton.Location = new System.Drawing.Point(4, 78);
            this.modifyVinButton.Margin = new System.Windows.Forms.Padding(2);
            this.modifyVinButton.Name = "modifyVinButton";
            this.modifyVinButton.Size = new System.Drawing.Size(216, 25);
            this.modifyVinButton.TabIndex = 2;
            this.modifyVinButton.Text = "Modify &VIN";
            this.modifyVinButton.UseVisualStyleBackColor = true;
            this.modifyVinButton.Click += new System.EventHandler(this.modifyVinButton_Click);
            // 
            // readPropertiesButton
            // 
            this.readPropertiesButton.Location = new System.Drawing.Point(4, 18);
            this.readPropertiesButton.Margin = new System.Windows.Forms.Padding(2);
            this.readPropertiesButton.Name = "readPropertiesButton";
            this.readPropertiesButton.Size = new System.Drawing.Size(216, 25);
            this.readPropertiesButton.TabIndex = 0;
            this.readPropertiesButton.Text = "Read &Properties";
            this.readPropertiesButton.UseVisualStyleBackColor = true;
            this.readPropertiesButton.Click += new System.EventHandler(this.readPropertiesButton_Click);
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.resultsTab);
            this.tabs.Controls.Add(this.helpTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(238, 10);
            this.tabs.Margin = new System.Windows.Forms.Padding(2);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(538, 525);
            this.tabs.TabIndex = 2;
            // 
            // resultsTab
            // 
            this.resultsTab.Controls.Add(this.userLog);
            this.resultsTab.Location = new System.Drawing.Point(4, 22);
            this.resultsTab.Margin = new System.Windows.Forms.Padding(2);
            this.resultsTab.Name = "resultsTab";
            this.resultsTab.Padding = new System.Windows.Forms.Padding(2);
            this.resultsTab.Size = new System.Drawing.Size(530, 499);
            this.resultsTab.TabIndex = 0;
            this.resultsTab.Text = "Results";
            this.resultsTab.UseVisualStyleBackColor = true;
            // 
            // userLog
            // 
            this.userLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userLog.Location = new System.Drawing.Point(4, 5);
            this.userLog.Margin = new System.Windows.Forms.Padding(2);
            this.userLog.Multiline = true;
            this.userLog.Name = "userLog";
            this.userLog.ReadOnly = true;
            this.userLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.userLog.Size = new System.Drawing.Size(524, 490);
            this.userLog.TabIndex = 0;
            // 
            // helpTab
            // 
            this.helpTab.Controls.Add(this.helpWebBrowser);
            this.helpTab.Location = new System.Drawing.Point(4, 22);
            this.helpTab.Margin = new System.Windows.Forms.Padding(2);
            this.helpTab.Name = "helpTab";
            this.helpTab.Size = new System.Drawing.Size(530, 499);
            this.helpTab.TabIndex = 2;
            this.helpTab.Text = "Help";
            this.helpTab.UseVisualStyleBackColor = true;
            // 
            // helpWebBrowser
            // 
            this.helpWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.helpWebBrowser.Margin = new System.Windows.Forms.Padding(2);
            this.helpWebBrowser.MinimumSize = new System.Drawing.Size(15, 16);
            this.helpWebBrowser.Name = "helpWebBrowser";
            this.helpWebBrowser.Size = new System.Drawing.Size(530, 499);
            this.helpWebBrowser.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 22);
            this.debugTab.Margin = new System.Windows.Forms.Padding(2);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(2);
            this.debugTab.Size = new System.Drawing.Size(530, 499);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug Log";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLog.Location = new System.Drawing.Point(2, 2);
            this.debugLog.Margin = new System.Windows.Forms.Padding(2);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugLog.Size = new System.Drawing.Size(526, 495);
            this.debugLog.TabIndex = 0;
            // 
            // releasedUnder
            // 
            this.releasedUnder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedUnder.AutoSize = true;
            this.releasedUnder.Location = new System.Drawing.Point(6, 544);
            this.releasedUnder.Name = "releasedUnder";
            this.releasedUnder.Size = new System.Drawing.Size(166, 13);
            this.releasedUnder.TabIndex = 3;
            this.releasedUnder.Text = "Released under GPLv3 license at";
            // 
            // releasedAt
            // 
            this.releasedAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedAt.AutoSize = true;
            this.releasedAt.Location = new System.Drawing.Point(169, 544);
            this.releasedAt.Name = "releasedAt";
            this.releasedAt.Size = new System.Drawing.Size(216, 13);
            this.releasedAt.TabIndex = 4;
            this.releasedAt.TabStop = true;
            this.releasedAt.Text = "https://github.com/LegacyNsfw/PcmHacks";
            // 
            // releasedByAntus
            // 
            this.releasedByAntus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedByAntus.AutoSize = true;
            this.releasedByAntus.Location = new System.Drawing.Point(382, 544);
            this.releasedByAntus.Name = "releasedByAntus";
            this.releasedByAntus.Size = new System.Drawing.Size(62, 13);
            this.releasedByAntus.TabIndex = 5;
            this.releasedByAntus.Text = "By antus @";
            // 
            // releasedAtPcmHacking
            // 
            this.releasedAtPcmHacking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedAtPcmHacking.AutoSize = true;
            this.releasedAtPcmHacking.Location = new System.Drawing.Point(444, 544);
            this.releasedAtPcmHacking.Name = "releasedAtPcmHacking";
            this.releasedAtPcmHacking.Size = new System.Drawing.Size(83, 13);
            this.releasedAtPcmHacking.TabIndex = 6;
            this.releasedAtPcmHacking.TabStop = true;
            this.releasedAtPcmHacking.Text = "pcmhacking.net";
            // 
            // releasedEtc
            // 
            this.releasedEtc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedEtc.AutoSize = true;
            this.releasedEtc.Location = new System.Drawing.Point(523, 544);
            this.releasedEtc.Name = "releasedEtc";
            this.releasedEtc.Size = new System.Drawing.Size(237, 13);
            this.releasedEtc.TabIndex = 7;
            this.releasedEtc.Text = "and NSFW. J2534 support by Envyous Customs.";
            // 
            // MainForm
            // 
            this.AcceptButton = this.readPropertiesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.releasedEtc);
            this.Controls.Add(this.releasedAtPcmHacking);
            this.Controls.Add(this.releasedByAntus);
            this.Controls.Add(this.releasedAt);
            this.Controls.Add(this.releasedUnder);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.operationsBox);
            this.Controls.Add(this.interfaceBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "PCM Hammer for GM \'0411";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.interfaceBox.ResumeLayout(false);
            this.operationsBox.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.resultsTab.ResumeLayout(false);
            this.resultsTab.PerformLayout();
            this.helpTab.ResumeLayout(false);
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox interfaceBox;
        private System.Windows.Forms.GroupBox operationsBox;
        private System.Windows.Forms.Button writeCalibrationButton;
        private System.Windows.Forms.Button readFullContentsButton;
        private System.Windows.Forms.Button modifyVinButton;
        private System.Windows.Forms.Button readPropertiesButton;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage resultsTab;
        private System.Windows.Forms.TextBox userLog;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox debugLog;
        private System.Windows.Forms.Button reinitializeButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Label deviceDescription;
        private System.Windows.Forms.Label releasedUnder;
        private System.Windows.Forms.LinkLabel releasedAt;
        private System.Windows.Forms.Label releasedByAntus;
        private System.Windows.Forms.LinkLabel releasedAtPcmHacking;
        private System.Windows.Forms.Label releasedEtc;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage helpTab;
        private System.Windows.Forms.WebBrowser helpWebBrowser;
        private System.Windows.Forms.Button exitKernelButton;
        private System.Windows.Forms.Button writeFullContentsButton;
        private System.Windows.Forms.Button writeOsAndCalibration;
        private System.Windows.Forms.Button testWriteButton;
        private System.Windows.Forms.Button quickComparisonButton;
        private System.Windows.Forms.Button writeParametersButton;
    }
}

