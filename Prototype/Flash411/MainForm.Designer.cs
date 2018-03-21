namespace Flash411
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
            this.interfaceBox = new System.Windows.Forms.GroupBox();
            this.interfaceTypeList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.interfacePortList = new System.Windows.Forms.ComboBox();
            this.operationsBox = new System.Windows.Forms.GroupBox();
            this.reinitializeButton = new System.Windows.Forms.Button();
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
            this.interfaceBox.SuspendLayout();
            this.operationsBox.SuspendLayout();
            this.tabs.SuspendLayout();
            this.resultsTab.SuspendLayout();
            this.debugTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // interfaceBox
            // 
            this.interfaceBox.Controls.Add(this.interfaceTypeList);
            this.interfaceBox.Controls.Add(this.label2);
            this.interfaceBox.Controls.Add(this.label1);
            this.interfaceBox.Controls.Add(this.interfacePortList);
            this.interfaceBox.Location = new System.Drawing.Point(9, 10);
            this.interfaceBox.Margin = new System.Windows.Forms.Padding(2);
            this.interfaceBox.Name = "interfaceBox";
            this.interfaceBox.Padding = new System.Windows.Forms.Padding(2);
            this.interfaceBox.Size = new System.Drawing.Size(224, 117);
            this.interfaceBox.TabIndex = 0;
            this.interfaceBox.TabStop = false;
            this.interfaceBox.Text = "Interface";
            // 
            // interfaceTypeList
            // 
            this.interfaceTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.interfaceTypeList.FormattingEnabled = true;
            this.interfaceTypeList.Location = new System.Drawing.Point(7, 36);
            this.interfaceTypeList.Margin = new System.Windows.Forms.Padding(2);
            this.interfaceTypeList.Name = "interfaceTypeList";
            this.interfaceTypeList.Size = new System.Drawing.Size(216, 21);
            this.interfaceTypeList.TabIndex = 3;
            this.interfaceTypeList.SelectedIndexChanged += new System.EventHandler(this.interfaceTypeList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 19);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Type";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 63);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Port";
            // 
            // interfacePortList
            // 
            this.interfacePortList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.interfacePortList.Enabled = false;
            this.interfacePortList.FormattingEnabled = true;
            this.interfacePortList.Location = new System.Drawing.Point(7, 79);
            this.interfacePortList.Margin = new System.Windows.Forms.Padding(2);
            this.interfacePortList.Name = "interfacePortList";
            this.interfacePortList.Size = new System.Drawing.Size(216, 21);
            this.interfacePortList.TabIndex = 1;
            this.interfacePortList.SelectedIndexChanged += new System.EventHandler(this.interfacePortList_SelectedIndexChanged);
            // 
            // operationsBox
            // 
            this.operationsBox.Controls.Add(this.reinitializeButton);
            this.operationsBox.Controls.Add(this.writeFullContentsButton);
            this.operationsBox.Controls.Add(this.readFullContentsButton);
            this.operationsBox.Controls.Add(this.modifyVinButton);
            this.operationsBox.Controls.Add(this.readPropertiesButton);
            this.operationsBox.Location = new System.Drawing.Point(9, 132);
            this.operationsBox.Margin = new System.Windows.Forms.Padding(2);
            this.operationsBox.Name = "operationsBox";
            this.operationsBox.Padding = new System.Windows.Forms.Padding(2);
            this.operationsBox.Size = new System.Drawing.Size(224, 174);
            this.operationsBox.TabIndex = 1;
            this.operationsBox.TabStop = false;
            this.operationsBox.Text = "Operations";
            // 
            // reinitializeButton
            // 
            this.reinitializeButton.Location = new System.Drawing.Point(5, 137);
            this.reinitializeButton.Margin = new System.Windows.Forms.Padding(2);
            this.reinitializeButton.Name = "reinitializeButton";
            this.reinitializeButton.Size = new System.Drawing.Size(214, 25);
            this.reinitializeButton.TabIndex = 4;
            this.reinitializeButton.Text = "Re-&Initialize Device";
            this.reinitializeButton.UseVisualStyleBackColor = true;
            this.reinitializeButton.Click += new System.EventHandler(this.reinitializeButton_Click);
            // 
            // writeFullContentsButton
            // 
            this.writeFullContentsButton.Location = new System.Drawing.Point(5, 108);
            this.writeFullContentsButton.Margin = new System.Windows.Forms.Padding(2);
            this.writeFullContentsButton.Name = "writeFullContentsButton";
            this.writeFullContentsButton.Size = new System.Drawing.Size(214, 25);
            this.writeFullContentsButton.TabIndex = 3;
            this.writeFullContentsButton.Text = "&Write Full Contents";
            this.writeFullContentsButton.UseVisualStyleBackColor = true;
            this.writeFullContentsButton.Click += new System.EventHandler(this.writeFullContentsButton_Click);
            // 
            // readFullContentsButton
            // 
            this.readFullContentsButton.Location = new System.Drawing.Point(5, 48);
            this.readFullContentsButton.Margin = new System.Windows.Forms.Padding(2);
            this.readFullContentsButton.Name = "readFullContentsButton";
            this.readFullContentsButton.Size = new System.Drawing.Size(214, 25);
            this.readFullContentsButton.TabIndex = 1;
            this.readFullContentsButton.Text = "&Read Full Contents";
            this.readFullContentsButton.UseVisualStyleBackColor = true;
            this.readFullContentsButton.Click += new System.EventHandler(this.readFullContentsButton_Click);
            // 
            // modifyVinButton
            // 
            this.modifyVinButton.Location = new System.Drawing.Point(5, 78);
            this.modifyVinButton.Margin = new System.Windows.Forms.Padding(2);
            this.modifyVinButton.Name = "modifyVinButton";
            this.modifyVinButton.Size = new System.Drawing.Size(214, 25);
            this.modifyVinButton.TabIndex = 2;
            this.modifyVinButton.Text = "Modify &VIN";
            this.modifyVinButton.UseVisualStyleBackColor = true;
            // 
            // readPropertiesButton
            // 
            this.readPropertiesButton.Location = new System.Drawing.Point(5, 18);
            this.readPropertiesButton.Margin = new System.Windows.Forms.Padding(2);
            this.readPropertiesButton.Name = "readPropertiesButton";
            this.readPropertiesButton.Size = new System.Drawing.Size(214, 25);
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
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(238, 10);
            this.tabs.Margin = new System.Windows.Forms.Padding(2);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(530, 360);
            this.tabs.TabIndex = 2;
            // 
            // resultsTab
            // 
            this.resultsTab.Controls.Add(this.userLog);
            this.resultsTab.Location = new System.Drawing.Point(4, 22);
            this.resultsTab.Margin = new System.Windows.Forms.Padding(2);
            this.resultsTab.Name = "resultsTab";
            this.resultsTab.Padding = new System.Windows.Forms.Padding(2);
            this.resultsTab.Size = new System.Drawing.Size(522, 334);
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
            this.userLog.Size = new System.Drawing.Size(516, 327);
            this.userLog.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 22);
            this.debugTab.Margin = new System.Windows.Forms.Padding(2);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(2);
            this.debugTab.Size = new System.Drawing.Size(522, 334);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug Log";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debugLog.Location = new System.Drawing.Point(4, 5);
            this.debugLog.Margin = new System.Windows.Forms.Padding(2);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugLog.Size = new System.Drawing.Size(516, 327);
            this.debugLog.TabIndex = 0;
            // 
            // startServerButton
            // 
            this.startServerButton.Location = new System.Drawing.Point(14, 341);
            this.startServerButton.Margin = new System.Windows.Forms.Padding(2);
            this.startServerButton.Name = "startServerButton";
            this.startServerButton.Size = new System.Drawing.Size(214, 25);
            this.startServerButton.TabIndex = 5;
            this.startServerButton.Text = "Enter &HTTP Server Mode";
            this.startServerButton.UseVisualStyleBackColor = true;
            this.startServerButton.Click += new System.EventHandler(this.startServerButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 379);
            this.Controls.Add(this.startServerButton);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.operationsBox);
            this.Controls.Add(this.interfaceBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Prototype";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.interfaceBox.ResumeLayout(false);
            this.interfaceBox.PerformLayout();
            this.operationsBox.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.resultsTab.ResumeLayout(false);
            this.resultsTab.PerformLayout();
            this.debugTab.ResumeLayout(false);
            this.debugTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox interfaceBox;
        private System.Windows.Forms.ComboBox interfaceTypeList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox interfacePortList;
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
    }
}

