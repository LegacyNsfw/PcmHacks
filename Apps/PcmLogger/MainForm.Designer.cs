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
            this.configurationTab = new System.Windows.Forms.TabPage();
            this.logFilePath = new System.Windows.Forms.Label();
            this.openDirectory = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.setDirectory = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.profilesTab = new System.Windows.Forms.TabPage();
            this.openButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.profileList = new System.Windows.Forms.ListBox();
            this.saveAsButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.parametersTab = new System.Windows.Forms.TabPage();
            this.parameterGrid = new System.Windows.Forms.DataGridView();
            this.statusTab = new System.Windows.Forms.TabPage();
            this.logValues = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.startStopLogging = new System.Windows.Forms.Button();
            this.enabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitsColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabs.SuspendLayout();
            this.configurationTab.SuspendLayout();
            this.profilesTab.SuspendLayout();
            this.parametersTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parameterGrid)).BeginInit();
            this.statusTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // loggerProgress
            // 
            this.loggerProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerProgress.Enabled = false;
            this.loggerProgress.Location = new System.Drawing.Point(325, 13);
            this.loggerProgress.Margin = new System.Windows.Forms.Padding(4);
            this.loggerProgress.MarqueeAnimationSpeed = 0;
            this.loggerProgress.Name = "loggerProgress";
            this.loggerProgress.Size = new System.Drawing.Size(692, 28);
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
            this.tabs.Controls.Add(this.configurationTab);
            this.tabs.Controls.Add(this.profilesTab);
            this.tabs.Controls.Add(this.parametersTab);
            this.tabs.Controls.Add(this.statusTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(16, 49);
            this.tabs.Margin = new System.Windows.Forms.Padding(4);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(1001, 558);
            this.tabs.TabIndex = 8;
            // 
            // configurationTab
            // 
            this.configurationTab.Controls.Add(this.logFilePath);
            this.configurationTab.Controls.Add(this.openDirectory);
            this.configurationTab.Controls.Add(this.selectButton);
            this.configurationTab.Controls.Add(this.setDirectory);
            this.configurationTab.Controls.Add(this.deviceDescription);
            this.configurationTab.Location = new System.Drawing.Point(4, 25);
            this.configurationTab.Name = "configurationTab";
            this.configurationTab.Size = new System.Drawing.Size(993, 529);
            this.configurationTab.TabIndex = 3;
            this.configurationTab.Text = "Configuration";
            this.configurationTab.UseVisualStyleBackColor = true;
            // 
            // logFilePath
            // 
            this.logFilePath.AutoSize = true;
            this.logFilePath.BackColor = System.Drawing.Color.Transparent;
            this.logFilePath.Location = new System.Drawing.Point(153, 85);
            this.logFilePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.logFilePath.Name = "logFilePath";
            this.logFilePath.Size = new System.Drawing.Size(65, 17);
            this.logFilePath.TabIndex = 7;
            this.logFilePath.Text = "Directory";
            // 
            // openDirectory
            // 
            this.openDirectory.Location = new System.Drawing.Point(6, 115);
            this.openDirectory.Margin = new System.Windows.Forms.Padding(4);
            this.openDirectory.Name = "openDirectory";
            this.openDirectory.Size = new System.Drawing.Size(139, 28);
            this.openDirectory.TabIndex = 9;
            this.openDirectory.Text = "&Open Log Folder";
            this.openDirectory.UseVisualStyleBackColor = true;
            this.openDirectory.Click += new System.EventHandler(this.openDirectory_Click);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(4, 4);
            this.selectButton.Margin = new System.Windows.Forms.Padding(4);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(288, 31);
            this.selectButton.TabIndex = 0;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // setDirectory
            // 
            this.setDirectory.Location = new System.Drawing.Point(5, 79);
            this.setDirectory.Margin = new System.Windows.Forms.Padding(4);
            this.setDirectory.Name = "setDirectory";
            this.setDirectory.Size = new System.Drawing.Size(140, 28);
            this.setDirectory.TabIndex = 6;
            this.setDirectory.Text = "Set Log &Folder";
            this.setDirectory.UseVisualStyleBackColor = true;
            this.setDirectory.Click += new System.EventHandler(this.setDirectory_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.AutoSize = true;
            this.deviceDescription.Location = new System.Drawing.Point(300, 11);
            this.deviceDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(114, 17);
            this.deviceDescription.TabIndex = 1;
            this.deviceDescription.Text = "[selected device]";
            // 
            // profilesTab
            // 
            this.profilesTab.Controls.Add(this.openButton);
            this.profilesTab.Controls.Add(this.newButton);
            this.profilesTab.Controls.Add(this.profileList);
            this.profilesTab.Controls.Add(this.saveAsButton);
            this.profilesTab.Controls.Add(this.saveButton);
            this.profilesTab.Location = new System.Drawing.Point(4, 25);
            this.profilesTab.Name = "profilesTab";
            this.profilesTab.Size = new System.Drawing.Size(993, 529);
            this.profilesTab.TabIndex = 4;
            this.profilesTab.Text = "Profiles";
            this.profilesTab.UseVisualStyleBackColor = true;
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(109, 3);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(100, 31);
            this.openButton.TabIndex = 4;
            this.openButton.Text = "&Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(3, 3);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(100, 31);
            this.newButton.TabIndex = 3;
            this.newButton.Text = "&New";
            this.newButton.UseVisualStyleBackColor = true;
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // profileList
            // 
            this.profileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileList.FormattingEnabled = true;
            this.profileList.ItemHeight = 16;
            this.profileList.Location = new System.Drawing.Point(3, 40);
            this.profileList.Name = "profileList";
            this.profileList.Size = new System.Drawing.Size(987, 484);
            this.profileList.TabIndex = 2;
            // 
            // saveAsButton
            // 
            this.saveAsButton.Location = new System.Drawing.Point(321, 3);
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(100, 31);
            this.saveAsButton.TabIndex = 1;
            this.saveAsButton.Text = "Save &As";
            this.saveAsButton.UseVisualStyleBackColor = true;
            this.saveAsButton.Click += new System.EventHandler(this.saveAsButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(215, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 31);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "&Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // parametersTab
            // 
            this.parametersTab.Controls.Add(this.parameterGrid);
            this.parametersTab.Location = new System.Drawing.Point(4, 25);
            this.parametersTab.Name = "parametersTab";
            this.parametersTab.Size = new System.Drawing.Size(993, 529);
            this.parametersTab.TabIndex = 2;
            this.parametersTab.Text = "Parameters";
            this.parametersTab.UseVisualStyleBackColor = true;
            // 
            // parameterGrid
            // 
            this.parameterGrid.AllowUserToAddRows = false;
            this.parameterGrid.AllowUserToDeleteRows = false;
            this.parameterGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.parameterGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.parameterGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.enabledColumn,
            this.nameColumn,
            this.unitsColumn});
            this.parameterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterGrid.Location = new System.Drawing.Point(0, 0);
            this.parameterGrid.Name = "parameterGrid";
            this.parameterGrid.RowTemplate.Height = 24;
            this.parameterGrid.Size = new System.Drawing.Size(993, 529);
            this.parameterGrid.TabIndex = 0;
            // 
            // statusTab
            // 
            this.statusTab.Controls.Add(this.logValues);
            this.statusTab.Location = new System.Drawing.Point(4, 25);
            this.statusTab.Margin = new System.Windows.Forms.Padding(4);
            this.statusTab.Name = "statusTab";
            this.statusTab.Padding = new System.Windows.Forms.Padding(4);
            this.statusTab.Size = new System.Drawing.Size(993, 529);
            this.statusTab.TabIndex = 0;
            this.statusTab.Text = "Data";
            this.statusTab.UseVisualStyleBackColor = true;
            // 
            // logValues
            // 
            this.logValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logValues.Location = new System.Drawing.Point(4, 4);
            this.logValues.Margin = new System.Windows.Forms.Padding(4);
            this.logValues.Multiline = true;
            this.logValues.Name = "logValues";
            this.logValues.ReadOnly = true;
            this.logValues.Size = new System.Drawing.Size(985, 521);
            this.logValues.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 25);
            this.debugTab.Margin = new System.Windows.Forms.Padding(4);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(4);
            this.debugTab.Size = new System.Drawing.Size(993, 529);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLog.Location = new System.Drawing.Point(4, 4);
            this.debugLog.Margin = new System.Windows.Forms.Padding(4);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugLog.Size = new System.Drawing.Size(985, 521);
            this.debugLog.TabIndex = 0;
            // 
            // startStopLogging
            // 
            this.startStopLogging.Enabled = false;
            this.startStopLogging.Location = new System.Drawing.Point(13, 13);
            this.startStopLogging.Margin = new System.Windows.Forms.Padding(4);
            this.startStopLogging.Name = "startStopLogging";
            this.startStopLogging.Size = new System.Drawing.Size(287, 28);
            this.startStopLogging.TabIndex = 4;
            this.startStopLogging.Text = "Start &Logging";
            this.startStopLogging.UseVisualStyleBackColor = true;
            this.startStopLogging.Click += new System.EventHandler(this.startStopLogging_Click);
            // 
            // enabledColumn
            // 
            this.enabledColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.enabledColumn.FillWeight = 1F;
            this.enabledColumn.HeaderText = "Enabled";
            this.enabledColumn.MinimumWidth = 50;
            this.enabledColumn.Name = "enabledColumn";
            this.enabledColumn.Width = 66;
            // 
            // nameColumn
            // 
            this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.MinimumWidth = 100;
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.ReadOnly = true;
            // 
            // unitsColumn
            // 
            this.unitsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.unitsColumn.FillWeight = 1F;
            this.unitsColumn.HeaderText = "Units";
            this.unitsColumn.MinimumWidth = 100;
            this.unitsColumn.Name = "unitsColumn";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1033, 622);
            this.Controls.Add(this.loggerProgress);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.startStopLogging);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "PCM Logger";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabs.ResumeLayout(false);
            this.configurationTab.ResumeLayout(false);
            this.configurationTab.PerformLayout();
            this.profilesTab.ResumeLayout(false);
            this.parametersTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.parameterGrid)).EndInit();
            this.statusTab.ResumeLayout(false);
            this.statusTab.PerformLayout();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startStopLogging;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage statusTab;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox logValues;
        private System.Windows.Forms.TextBox debugLog;
        private System.Windows.Forms.ProgressBar loggerProgress;
        private System.Windows.Forms.Label logFilePath;
        private System.Windows.Forms.Button setDirectory;
        private System.Windows.Forms.Button openDirectory;
        private System.Windows.Forms.TabPage parametersTab;
        private System.Windows.Forms.DataGridView parameterGrid;
        private System.Windows.Forms.TabPage configurationTab;
        private System.Windows.Forms.TabPage profilesTab;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.ListBox profileList;
        private System.Windows.Forms.Button saveAsButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enabledColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn unitsColumn;
    }
}

