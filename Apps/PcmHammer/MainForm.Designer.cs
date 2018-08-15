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
            this.cancelButton = new System.Windows.Forms.Button();
            this.writeFullContentsButton = new System.Windows.Forms.Button();
            this.readFullContentsButton = new System.Windows.Forms.Button();
            this.modifyVinButton = new System.Windows.Forms.Button();
            this.readPropertiesButton = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.resultsTab = new System.Windows.Forms.TabPage();
            this.userLog = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.startServerButton = new System.Windows.Forms.Button();
            this.releasedUnder = new System.Windows.Forms.Label();
            this.releasedAt = new System.Windows.Forms.LinkLabel();
            this.releasedByAntus = new System.Windows.Forms.Label();
            this.releasedAtPcmHacking = new System.Windows.Forms.LinkLabel();
            this.releasedEtc = new System.Windows.Forms.Label();
            this.helpTab = new System.Windows.Forms.TabPage();
            this.licenseTab = new System.Windows.Forms.TabPage();
            this.licenseText = new System.Windows.Forms.TextBox();
            this.helpWebBrowser = new System.Windows.Forms.WebBrowser();
            this.interfaceBox.SuspendLayout();
            this.operationsBox.SuspendLayout();
            this.tabs.SuspendLayout();
            this.resultsTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.helpTab.SuspendLayout();
            this.licenseTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // interfaceBox
            // 
            this.interfaceBox.Controls.Add(this.reinitializeButton);
            this.interfaceBox.Controls.Add(this.selectButton);
            this.interfaceBox.Controls.Add(this.deviceDescription);
            this.interfaceBox.Location = new System.Drawing.Point(12, 12);
            this.interfaceBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.interfaceBox.Name = "interfaceBox";
            this.interfaceBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.interfaceBox.Size = new System.Drawing.Size(299, 158);
            this.interfaceBox.TabIndex = 0;
            this.interfaceBox.TabStop = false;
            this.interfaceBox.Text = "Device";
            // 
            // reinitializeButton
            // 
            this.reinitializeButton.Location = new System.Drawing.Point(8, 118);
            this.reinitializeButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.reinitializeButton.Name = "reinitializeButton";
            this.reinitializeButton.Size = new System.Drawing.Size(285, 31);
            this.reinitializeButton.TabIndex = 2;
            this.reinitializeButton.Text = "Re-&Initialize Device";
            this.reinitializeButton.UseVisualStyleBackColor = true;
            this.reinitializeButton.Click += new System.EventHandler(this.reinitializeButton_Click);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(7, 84);
            this.selectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(285, 28);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.Location = new System.Drawing.Point(7, 37);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(285, 16);
            this.deviceDescription.TabIndex = 0;
            this.deviceDescription.Text = "Device name will be displayed here";
            // 
            // operationsBox
            // 
            this.operationsBox.Controls.Add(this.cancelButton);
            this.operationsBox.Controls.Add(this.writeFullContentsButton);
            this.operationsBox.Controls.Add(this.readFullContentsButton);
            this.operationsBox.Controls.Add(this.modifyVinButton);
            this.operationsBox.Controls.Add(this.readPropertiesButton);
            this.operationsBox.Location = new System.Drawing.Point(12, 188);
            this.operationsBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.operationsBox.Name = "operationsBox";
            this.operationsBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.operationsBox.Size = new System.Drawing.Size(299, 210);
            this.operationsBox.TabIndex = 1;
            this.operationsBox.TabStop = false;
            this.operationsBox.Text = "Operations";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(7, 169);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(285, 31);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // writeFullContentsButton
            // 
            this.writeFullContentsButton.Location = new System.Drawing.Point(7, 133);
            this.writeFullContentsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.writeFullContentsButton.Name = "writeFullContentsButton";
            this.writeFullContentsButton.Size = new System.Drawing.Size(285, 31);
            this.writeFullContentsButton.TabIndex = 3;
            this.writeFullContentsButton.Text = "&Write Full Contents";
            this.writeFullContentsButton.UseVisualStyleBackColor = true;
            this.writeFullContentsButton.Click += new System.EventHandler(this.writeFullContentsButton_Click);
            // 
            // readFullContentsButton
            // 
            this.readFullContentsButton.Location = new System.Drawing.Point(7, 59);
            this.readFullContentsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.readFullContentsButton.Name = "readFullContentsButton";
            this.readFullContentsButton.Size = new System.Drawing.Size(285, 31);
            this.readFullContentsButton.TabIndex = 1;
            this.readFullContentsButton.Text = "&Read Full Contents";
            this.readFullContentsButton.UseVisualStyleBackColor = true;
            this.readFullContentsButton.Click += new System.EventHandler(this.readFullContentsButton_Click);
            // 
            // modifyVinButton
            // 
            this.modifyVinButton.Location = new System.Drawing.Point(7, 96);
            this.modifyVinButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modifyVinButton.Name = "modifyVinButton";
            this.modifyVinButton.Size = new System.Drawing.Size(285, 31);
            this.modifyVinButton.TabIndex = 2;
            this.modifyVinButton.Text = "Modify &VIN";
            this.modifyVinButton.UseVisualStyleBackColor = true;
            this.modifyVinButton.Click += new System.EventHandler(this.modifyVinButton_Click);
            // 
            // readPropertiesButton
            // 
            this.readPropertiesButton.Location = new System.Drawing.Point(7, 22);
            this.readPropertiesButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.readPropertiesButton.Name = "readPropertiesButton";
            this.readPropertiesButton.Size = new System.Drawing.Size(285, 31);
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
            this.tabs.Controls.Add(this.licenseTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(317, 12);
            this.tabs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(769, 496);
            this.tabs.TabIndex = 6;
            // 
            // resultsTab
            // 
            this.resultsTab.Controls.Add(this.userLog);
            this.resultsTab.Location = new System.Drawing.Point(4, 25);
            this.resultsTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.resultsTab.Name = "resultsTab";
            this.resultsTab.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.resultsTab.Size = new System.Drawing.Size(761, 467);
            this.resultsTab.TabIndex = 0;
            this.resultsTab.Text = "Results";
            this.resultsTab.UseVisualStyleBackColor = true;
            // 
            // userLog
            // 
            this.userLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userLog.Location = new System.Drawing.Point(5, 6);
            this.userLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.userLog.Multiline = true;
            this.userLog.Name = "userLog";
            this.userLog.ReadOnly = true;
            this.userLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.userLog.Size = new System.Drawing.Size(749, 452);
            this.userLog.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 25);
            this.debugTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.debugTab.Size = new System.Drawing.Size(761, 467);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug Log";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLog.Location = new System.Drawing.Point(3, 2);
            this.debugLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugLog.Size = new System.Drawing.Size(755, 463);
            this.debugLog.TabIndex = 0;
            // 
            // startServerButton
            // 
            this.startServerButton.Location = new System.Drawing.Point(19, 473);
            this.startServerButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.startServerButton.Name = "startServerButton";
            this.startServerButton.Size = new System.Drawing.Size(285, 31);
            this.startServerButton.TabIndex = 2;
            this.startServerButton.Text = "Enter &HTTP Server Mode";
            this.startServerButton.UseVisualStyleBackColor = true;
            this.startServerButton.Visible = false;
            this.startServerButton.Click += new System.EventHandler(this.startServerButton_Click);
            // 
            // releasedUnder
            // 
            this.releasedUnder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedUnder.AutoSize = true;
            this.releasedUnder.Location = new System.Drawing.Point(8, 519);
            this.releasedUnder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.releasedUnder.Name = "releasedUnder";
            this.releasedUnder.Size = new System.Drawing.Size(220, 17);
            this.releasedUnder.TabIndex = 7;
            this.releasedUnder.Text = "Released under GPLv3 license at";
            // 
            // releasedAt
            // 
            this.releasedAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedAt.AutoSize = true;
            this.releasedAt.Location = new System.Drawing.Point(225, 519);
            this.releasedAt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.releasedAt.Name = "releasedAt";
            this.releasedAt.Size = new System.Drawing.Size(270, 17);
            this.releasedAt.TabIndex = 8;
            this.releasedAt.TabStop = true;
            this.releasedAt.Text = "https://github.com/LegacyNsfw/PcmHacks";
            // 
            // releasedByAntus
            // 
            this.releasedByAntus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedByAntus.AutoSize = true;
            this.releasedByAntus.Location = new System.Drawing.Point(509, 519);
            this.releasedByAntus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.releasedByAntus.Name = "releasedByAntus";
            this.releasedByAntus.Size = new System.Drawing.Size(81, 17);
            this.releasedByAntus.TabIndex = 9;
            this.releasedByAntus.Text = "By antus @";
            // 
            // releasedAtPcmHacking
            // 
            this.releasedAtPcmHacking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedAtPcmHacking.AutoSize = true;
            this.releasedAtPcmHacking.Location = new System.Drawing.Point(592, 519);
            this.releasedAtPcmHacking.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.releasedAtPcmHacking.Name = "releasedAtPcmHacking";
            this.releasedAtPcmHacking.Size = new System.Drawing.Size(107, 17);
            this.releasedAtPcmHacking.TabIndex = 1;
            this.releasedAtPcmHacking.TabStop = true;
            this.releasedAtPcmHacking.Text = "pcmhacking.net";
            // 
            // releasedEtc
            // 
            this.releasedEtc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.releasedEtc.AutoSize = true;
            this.releasedEtc.Location = new System.Drawing.Point(697, 519);
            this.releasedEtc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.releasedEtc.Name = "releasedEtc";
            this.releasedEtc.Size = new System.Drawing.Size(314, 17);
            this.releasedEtc.TabIndex = 2;
            this.releasedEtc.Text = "and NSFW. J2534 support by Envyous Customs.";
            // 
            // helpTab
            // 
            this.helpTab.Controls.Add(this.helpWebBrowser);
            this.helpTab.Location = new System.Drawing.Point(4, 25);
            this.helpTab.Name = "helpTab";
            this.helpTab.Size = new System.Drawing.Size(761, 467);
            this.helpTab.TabIndex = 2;
            this.helpTab.Text = "Help";
            this.helpTab.UseVisualStyleBackColor = true;
            // 
            // licenseTab
            // 
            this.licenseTab.Controls.Add(this.licenseText);
            this.licenseTab.Location = new System.Drawing.Point(4, 25);
            this.licenseTab.Name = "licenseTab";
            this.licenseTab.Size = new System.Drawing.Size(761, 467);
            this.licenseTab.TabIndex = 3;
            this.licenseTab.Text = "License";
            this.licenseTab.UseVisualStyleBackColor = true;
            // 
            // licenseText
            // 
            this.licenseText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.licenseText.Location = new System.Drawing.Point(0, 0);
            this.licenseText.Multiline = true;
            this.licenseText.Name = "licenseText";
            this.licenseText.ReadOnly = true;
            this.licenseText.Size = new System.Drawing.Size(761, 467);
            this.licenseText.TabIndex = 1;
            // 
            // helpWebBrowser
            // 
            this.helpWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.helpWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.helpWebBrowser.Name = "helpWebBrowser";
            this.helpWebBrowser.Size = new System.Drawing.Size(761, 467);
            this.helpWebBrowser.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 540);
            this.Controls.Add(this.releasedEtc);
            this.Controls.Add(this.releasedAtPcmHacking);
            this.Controls.Add(this.releasedByAntus);
            this.Controls.Add(this.releasedAt);
            this.Controls.Add(this.releasedUnder);
            this.Controls.Add(this.startServerButton);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.operationsBox);
            this.Controls.Add(this.interfaceBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "PCM Hammer for GM \'0411";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.interfaceBox.ResumeLayout(false);
            this.operationsBox.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.resultsTab.ResumeLayout(false);
            this.resultsTab.PerformLayout();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.helpTab.ResumeLayout(false);
            this.licenseTab.ResumeLayout(false);
            this.licenseTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox interfaceBox;
        private System.Windows.Forms.GroupBox operationsBox;
        private System.Windows.Forms.Button writeFullContentsButton;
        private System.Windows.Forms.Button readFullContentsButton;
        private System.Windows.Forms.Button modifyVinButton;
        private System.Windows.Forms.Button readPropertiesButton;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage resultsTab;
        private System.Windows.Forms.TextBox userLog;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox debugLog;
        private System.Windows.Forms.Button reinitializeButton;
        private System.Windows.Forms.Button startServerButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Label deviceDescription;
        private System.Windows.Forms.Label releasedUnder;
        private System.Windows.Forms.LinkLabel releasedAt;
        private System.Windows.Forms.Label releasedByAntus;
        private System.Windows.Forms.LinkLabel releasedAtPcmHacking;
        private System.Windows.Forms.Label releasedEtc;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage helpTab;
        private System.Windows.Forms.TabPage licenseTab;
        private System.Windows.Forms.TextBox licenseText;
        private System.Windows.Forms.WebBrowser helpWebBrowser;
    }
}

