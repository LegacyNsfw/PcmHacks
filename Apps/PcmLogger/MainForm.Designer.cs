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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.loggerProgress = new System.Windows.Forms.ProgressBar();
            this.tabs = new System.Windows.Forms.TabControl();
            this.configurationTab = new System.Windows.Forms.TabPage();
            this.logFilePath = new System.Windows.Forms.Label();
            this.openDirectory = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.setDirectory = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.profilesTab = new System.Windows.Forms.TabPage();
            this.removeProfileButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.profileList = new System.Windows.Forms.ListBox();
            this.saveAsButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.parametersTab = new System.Windows.Forms.TabPage();
            this.parametersSplitter = new System.Windows.Forms.SplitContainer();
            this.parameterSearch = new System.Windows.Forms.TextBox();
            this.parameterGrid = new System.Windows.Forms.DataGridView();
            this.enabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitsColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.logValues = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.dashboardTab = new System.Windows.Forms.TabPage();
            this.startStopSaving = new System.Windows.Forms.Button();
            this.disclaimer = new System.Windows.Forms.Label();
            this.tabs.SuspendLayout();
            this.configurationTab.SuspendLayout();
            this.profilesTab.SuspendLayout();
            this.parametersTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parametersSplitter)).BeginInit();
            this.parametersSplitter.Panel1.SuspendLayout();
            this.parametersSplitter.Panel2.SuspendLayout();
            this.parametersSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parameterGrid)).BeginInit();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // loggerProgress
            // 
            this.loggerProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerProgress.Enabled = false;
            this.loggerProgress.Location = new System.Drawing.Point(244, 11);
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
            this.tabs.Controls.Add(this.configurationTab);
            this.tabs.Controls.Add(this.profilesTab);
            this.tabs.Controls.Add(this.parametersTab);
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(12, 40);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(751, 453);
            this.tabs.TabIndex = 8;
            // 
            // configurationTab
            // 
            this.configurationTab.Controls.Add(this.disclaimer);
            this.configurationTab.Controls.Add(this.logFilePath);
            this.configurationTab.Controls.Add(this.openDirectory);
            this.configurationTab.Controls.Add(this.selectButton);
            this.configurationTab.Controls.Add(this.setDirectory);
            this.configurationTab.Controls.Add(this.deviceDescription);
            this.configurationTab.Location = new System.Drawing.Point(4, 22);
            this.configurationTab.Margin = new System.Windows.Forms.Padding(2);
            this.configurationTab.Name = "configurationTab";
            this.configurationTab.Size = new System.Drawing.Size(743, 427);
            this.configurationTab.TabIndex = 3;
            this.configurationTab.Text = "Configuration";
            this.configurationTab.UseVisualStyleBackColor = true;
            // 
            // logFilePath
            // 
            this.logFilePath.AutoSize = true;
            this.logFilePath.BackColor = System.Drawing.Color.Transparent;
            this.logFilePath.Location = new System.Drawing.Point(115, 69);
            this.logFilePath.Name = "logFilePath";
            this.logFilePath.Size = new System.Drawing.Size(49, 13);
            this.logFilePath.TabIndex = 7;
            this.logFilePath.Text = "Directory";
            // 
            // openDirectory
            // 
            this.openDirectory.Location = new System.Drawing.Point(4, 93);
            this.openDirectory.Name = "openDirectory";
            this.openDirectory.Size = new System.Drawing.Size(104, 23);
            this.openDirectory.TabIndex = 9;
            this.openDirectory.Text = "&Open Log Folder";
            this.openDirectory.UseVisualStyleBackColor = true;
            this.openDirectory.Click += new System.EventHandler(this.openDirectory_Click);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(3, 3);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(216, 25);
            this.selectButton.TabIndex = 0;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // setDirectory
            // 
            this.setDirectory.Location = new System.Drawing.Point(4, 64);
            this.setDirectory.Name = "setDirectory";
            this.setDirectory.Size = new System.Drawing.Size(105, 23);
            this.setDirectory.TabIndex = 6;
            this.setDirectory.Text = "Set Log &Folder";
            this.setDirectory.UseVisualStyleBackColor = true;
            this.setDirectory.Click += new System.EventHandler(this.setDirectory_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.AutoSize = true;
            this.deviceDescription.Location = new System.Drawing.Point(225, 9);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(88, 13);
            this.deviceDescription.TabIndex = 1;
            this.deviceDescription.Text = "[selected device]";
            // 
            // profilesTab
            // 
            this.profilesTab.Controls.Add(this.removeProfileButton);
            this.profilesTab.Controls.Add(this.openButton);
            this.profilesTab.Controls.Add(this.newButton);
            this.profilesTab.Controls.Add(this.profileList);
            this.profilesTab.Controls.Add(this.saveAsButton);
            this.profilesTab.Controls.Add(this.saveButton);
            this.profilesTab.Location = new System.Drawing.Point(4, 22);
            this.profilesTab.Margin = new System.Windows.Forms.Padding(2);
            this.profilesTab.Name = "profilesTab";
            this.profilesTab.Size = new System.Drawing.Size(743, 427);
            this.profilesTab.TabIndex = 4;
            this.profilesTab.Text = "Profiles";
            this.profilesTab.UseVisualStyleBackColor = true;
            // 
            // removeProfileButton
            // 
            this.removeProfileButton.Enabled = false;
            this.removeProfileButton.Location = new System.Drawing.Point(666, 3);
            this.removeProfileButton.Margin = new System.Windows.Forms.Padding(2);
            this.removeProfileButton.Name = "removeProfileButton";
            this.removeProfileButton.Size = new System.Drawing.Size(75, 25);
            this.removeProfileButton.TabIndex = 5;
            this.removeProfileButton.Text = "&Remove";
            this.removeProfileButton.UseVisualStyleBackColor = true;
            this.removeProfileButton.Click += new System.EventHandler(this.removeProfileButton_Click);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(82, 2);
            this.openButton.Margin = new System.Windows.Forms.Padding(2);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 25);
            this.openButton.TabIndex = 4;
            this.openButton.Text = "&Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(2, 2);
            this.newButton.Margin = new System.Windows.Forms.Padding(2);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(75, 25);
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
            this.profileList.Location = new System.Drawing.Point(2, 32);
            this.profileList.Margin = new System.Windows.Forms.Padding(2);
            this.profileList.Name = "profileList";
            this.profileList.Size = new System.Drawing.Size(739, 394);
            this.profileList.TabIndex = 2;
            this.profileList.SelectedIndexChanged += new System.EventHandler(this.profileList_SelectedIndexChanged);
            // 
            // saveAsButton
            // 
            this.saveAsButton.Location = new System.Drawing.Point(241, 2);
            this.saveAsButton.Margin = new System.Windows.Forms.Padding(2);
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(75, 25);
            this.saveAsButton.TabIndex = 1;
            this.saveAsButton.Text = "Save &As";
            this.saveAsButton.UseVisualStyleBackColor = true;
            this.saveAsButton.Click += new System.EventHandler(this.saveAsButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(161, 2);
            this.saveButton.Margin = new System.Windows.Forms.Padding(2);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 25);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "&Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // parametersTab
            // 
            this.parametersTab.Controls.Add(this.parametersSplitter);
            this.parametersTab.Location = new System.Drawing.Point(4, 22);
            this.parametersTab.Margin = new System.Windows.Forms.Padding(2);
            this.parametersTab.Name = "parametersTab";
            this.parametersTab.Size = new System.Drawing.Size(743, 427);
            this.parametersTab.TabIndex = 2;
            this.parametersTab.Text = "Parameters";
            this.parametersTab.UseVisualStyleBackColor = true;
            // 
            // parametersSplitter
            // 
            this.parametersSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parametersSplitter.Location = new System.Drawing.Point(0, 0);
            this.parametersSplitter.Name = "parametersSplitter";
            // 
            // parametersSplitter.Panel1
            // 
            this.parametersSplitter.Panel1.Controls.Add(this.parameterSearch);
            this.parametersSplitter.Panel1.Controls.Add(this.parameterGrid);
            this.parametersSplitter.Panel1MinSize = 200;
            // 
            // parametersSplitter.Panel2
            // 
            this.parametersSplitter.Panel2.Controls.Add(this.logValues);
            this.parametersSplitter.Size = new System.Drawing.Size(743, 427);
            this.parametersSplitter.SplitterDistance = 400;
            this.parametersSplitter.TabIndex = 1;
            // 
            // parameterSearch
            // 
            this.parameterSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parameterSearch.Location = new System.Drawing.Point(3, 3);
            this.parameterSearch.Name = "parameterSearch";
            this.parameterSearch.Size = new System.Drawing.Size(394, 20);
            this.parameterSearch.TabIndex = 1;
            this.parameterSearch.TextChanged += new System.EventHandler(this.parameterSearch_TextChanged);
            this.parameterSearch.Enter += new System.EventHandler(this.parameterSearch_Enter);
            this.parameterSearch.Leave += new System.EventHandler(this.parameterSearch_Leave);
            // 
            // parameterGrid
            // 
            this.parameterGrid.AllowUserToAddRows = false;
            this.parameterGrid.AllowUserToDeleteRows = false;
            this.parameterGrid.AllowUserToResizeRows = false;
            this.parameterGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parameterGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.parameterGrid.CausesValidation = false;
            this.parameterGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.parameterGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.enabledColumn,
            this.nameColumn,
            this.unitsColumn});
            this.parameterGrid.Location = new System.Drawing.Point(0, 28);
            this.parameterGrid.Margin = new System.Windows.Forms.Padding(2);
            this.parameterGrid.Name = "parameterGrid";
            this.parameterGrid.RowHeadersVisible = false;
            this.parameterGrid.RowTemplate.Height = 24;
            this.parameterGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.parameterGrid.ShowCellErrors = false;
            this.parameterGrid.ShowEditingIcon = false;
            this.parameterGrid.ShowRowErrors = false;
            this.parameterGrid.Size = new System.Drawing.Size(400, 399);
            this.parameterGrid.TabIndex = 0;
            this.parameterGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.parameterGrid_CellContentClick);
            this.parameterGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.parameterGrid_CellValueChanged);
            // 
            // enabledColumn
            // 
            this.enabledColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.enabledColumn.FillWeight = 1F;
            this.enabledColumn.HeaderText = "Enabled";
            this.enabledColumn.MinimumWidth = 50;
            this.enabledColumn.Name = "enabledColumn";
            this.enabledColumn.Width = 52;
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
            // logValues
            // 
            this.logValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logValues.Location = new System.Drawing.Point(0, 0);
            this.logValues.Multiline = true;
            this.logValues.Name = "logValues";
            this.logValues.ReadOnly = true;
            this.logValues.Size = new System.Drawing.Size(339, 427);
            this.logValues.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 22);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(3);
            this.debugTab.Size = new System.Drawing.Size(743, 427);
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
            this.debugLog.Size = new System.Drawing.Size(737, 421);
            this.debugLog.TabIndex = 0;
            // 
            // dashboardTab
            // 
            this.dashboardTab.Location = new System.Drawing.Point(4, 22);
            this.dashboardTab.Name = "dashboardTab";
            this.dashboardTab.Padding = new System.Windows.Forms.Padding(3);
            this.dashboardTab.Size = new System.Drawing.Size(743, 427);
            this.dashboardTab.TabIndex = 0;
            this.dashboardTab.Text = "Dashboard";
            this.dashboardTab.UseVisualStyleBackColor = true;
            // 
            // startStopSaving
            // 
            this.startStopSaving.Enabled = false;
            this.startStopSaving.Location = new System.Drawing.Point(12, 11);
            this.startStopSaving.Name = "startStopSaving";
            this.startStopSaving.Size = new System.Drawing.Size(215, 23);
            this.startStopSaving.TabIndex = 4;
            this.startStopSaving.Text = "Start &Recording";
            this.startStopSaving.UseVisualStyleBackColor = true;
            this.startStopSaving.Click += new System.EventHandler(this.startStopSaving_Click);
            // 
            // disclaimer
            // 
            this.disclaimer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.disclaimer.Location = new System.Drawing.Point(4, 145);
            this.disclaimer.Name = "disclaimer";
            this.disclaimer.Size = new System.Drawing.Size(739, 140);
            this.disclaimer.TabIndex = 10;
            this.disclaimer.Text = resources.GetString("disclaimer.Text");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 505);
            this.Controls.Add(this.loggerProgress);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.startStopSaving);
            this.Name = "MainForm";
            this.Text = "(window title is set programmatically)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabs.ResumeLayout(false);
            this.configurationTab.ResumeLayout(false);
            this.configurationTab.PerformLayout();
            this.profilesTab.ResumeLayout(false);
            this.parametersTab.ResumeLayout(false);
            this.parametersSplitter.Panel1.ResumeLayout(false);
            this.parametersSplitter.Panel1.PerformLayout();
            this.parametersSplitter.Panel2.ResumeLayout(false);
            this.parametersSplitter.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parametersSplitter)).EndInit();
            this.parametersSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.parameterGrid)).EndInit();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startStopSaving;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage dashboardTab;
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
        private System.Windows.Forms.SplitContainer parametersSplitter;
        private System.Windows.Forms.TextBox parameterSearch;
        private System.Windows.Forms.Button removeProfileButton;
        private System.Windows.Forms.Label disclaimer;
    }
}

