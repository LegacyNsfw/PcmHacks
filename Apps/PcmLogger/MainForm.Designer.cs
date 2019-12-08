namespace PcmHacking
{
    partial class MainForm : MainFormBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Label deviceDescription;

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
            this.loggerProgress = new System.Windows.Forms.ProgressBar();
            this.tabs = new System.Windows.Forms.TabControl();
            this.statusTab = new System.Windows.Forms.TabPage();
            this.logValues = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.profilePath = new System.Windows.Forms.Label();
            this.selectProfileButton = new System.Windows.Forms.Button();
            this.startStopLogging = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.logFilePath = new System.Windows.Forms.Label();
            this.setDirectory = new System.Windows.Forms.Button();
            this.openDirectory = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.statusTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // loggerProgress
            // 
            this.loggerProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerProgress.Enabled = false;
            this.loggerProgress.Location = new System.Drawing.Point(237, 72);
            this.loggerProgress.MarqueeAnimationSpeed = 0;
            this.loggerProgress.Name = "loggerProgress";
            this.loggerProgress.Size = new System.Drawing.Size(519, 23);
            this.loggerProgress.Step = 0;
            this.loggerProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.loggerProgress.TabIndex = 5;
            this.loggerProgress.Visible = false;
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.statusTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(12, 130);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(751, 363);
            this.tabs.TabIndex = 8;
            // 
            // statusTab
            // 
            this.statusTab.Controls.Add(this.logValues);
            this.statusTab.Location = new System.Drawing.Point(4, 22);
            this.statusTab.Name = "statusTab";
            this.statusTab.Padding = new System.Windows.Forms.Padding(3);
            this.statusTab.Size = new System.Drawing.Size(743, 337);
            this.statusTab.TabIndex = 0;
            this.statusTab.Text = "Status";
            this.statusTab.UseVisualStyleBackColor = true;
            // 
            // logValues
            // 
            this.logValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logValues.Location = new System.Drawing.Point(3, 3);
            this.logValues.Multiline = true;
            this.logValues.Name = "logValues";
            this.logValues.ReadOnly = true;
            this.logValues.Size = new System.Drawing.Size(737, 331);
            this.logValues.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 22);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(3);
            this.debugTab.Size = new System.Drawing.Size(743, 337);
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
            this.debugLog.Size = new System.Drawing.Size(737, 331);
            this.debugLog.TabIndex = 0;
            // 
            // profilePath
            // 
            this.profilePath.AutoSize = true;
            this.profilePath.Location = new System.Drawing.Point(234, 48);
            this.profilePath.Name = "profilePath";
            this.profilePath.Size = new System.Drawing.Size(84, 13);
            this.profilePath.TabIndex = 3;
            this.profilePath.Text = "[selected profile]";
            // 
            // selectProfileButton
            // 
            this.selectProfileButton.Location = new System.Drawing.Point(12, 43);
            this.selectProfileButton.Name = "selectProfileButton";
            this.selectProfileButton.Size = new System.Drawing.Size(215, 23);
            this.selectProfileButton.TabIndex = 2;
            this.selectProfileButton.Text = "Select Log &Profile";
            this.selectProfileButton.UseVisualStyleBackColor = true;
            this.selectProfileButton.Click += new System.EventHandler(this.selectProfile_Click);
            // 
            // startStopLogging
            // 
            this.startStopLogging.Enabled = false;
            this.startStopLogging.Location = new System.Drawing.Point(12, 72);
            this.startStopLogging.Name = "startStopLogging";
            this.startStopLogging.Size = new System.Drawing.Size(215, 23);
            this.startStopLogging.TabIndex = 4;
            this.startStopLogging.Text = "Start &Logging";
            this.startStopLogging.UseVisualStyleBackColor = true;
            this.startStopLogging.Click += new System.EventHandler(this.startStopLogging_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.AutoSize = true;
            this.deviceDescription.Location = new System.Drawing.Point(234, 18);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(88, 13);
            this.deviceDescription.TabIndex = 1;
            this.deviceDescription.Text = "[selected device]";
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(12, 12);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(216, 25);
            this.selectButton.TabIndex = 0;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // logFilePath
            // 
            this.logFilePath.AutoSize = true;
            this.logFilePath.BackColor = System.Drawing.Color.Transparent;
            this.logFilePath.Location = new System.Drawing.Point(234, 106);
            this.logFilePath.Name = "logFilePath";
            this.logFilePath.Size = new System.Drawing.Size(49, 13);
            this.logFilePath.TabIndex = 7;
            this.logFilePath.Text = "Directory";
            // 
            // setDirectory
            // 
            this.setDirectory.Location = new System.Drawing.Point(12, 101);
            this.setDirectory.Name = "setDirectory";
            this.setDirectory.Size = new System.Drawing.Size(105, 23);
            this.setDirectory.TabIndex = 6;
            this.setDirectory.Text = "Set &Folder";
            this.setDirectory.UseVisualStyleBackColor = true;
            this.setDirectory.Click += new System.EventHandler(this.ChooseDirectory_Click);
            // 
            // openDirectory
            // 
            this.openDirectory.Location = new System.Drawing.Point(123, 101);
            this.openDirectory.Name = "openDirectory";
            this.openDirectory.Size = new System.Drawing.Size(104, 23);
            this.openDirectory.TabIndex = 9;
            this.openDirectory.Text = "&Open Folder";
            this.openDirectory.UseVisualStyleBackColor = true;
            this.openDirectory.Click += new System.EventHandler(this.openDirectory_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 505);
            this.Controls.Add(this.openDirectory);
            this.Controls.Add(this.setDirectory);
            this.Controls.Add(this.logFilePath);
            this.Controls.Add(this.loggerProgress);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.profilePath);
            this.Controls.Add(this.selectProfileButton);
            this.Controls.Add(this.startStopLogging);
            this.Controls.Add(this.deviceDescription);
            this.Controls.Add(this.selectButton);
            this.Name = "MainForm";
            this.Text = "PCM Logger";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabs.ResumeLayout(false);
            this.statusTab.ResumeLayout(false);
            this.statusTab.PerformLayout();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startStopLogging;
        private System.Windows.Forms.Button selectProfileButton;
        private System.Windows.Forms.Label profilePath;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage statusTab;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox logValues;
        private System.Windows.Forms.TextBox debugLog;
        private System.Windows.Forms.ProgressBar loggerProgress;
        private System.Windows.Forms.Label logFilePath;
        private System.Windows.Forms.Button setDirectory;
        private System.Windows.Forms.Button openDirectory;
    }
}

