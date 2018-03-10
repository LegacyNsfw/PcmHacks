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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.writeFullContentsButton = new System.Windows.Forms.Button();
            this.readFullContentsButton = new System.Windows.Forms.Button();
            this.modifyVinButton = new System.Windows.Forms.Button();
            this.readPropertiesButton = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.resultsTab = new System.Windows.Forms.TabPage();
            this.userLog = new System.Windows.Forms.TextBox();
            this.debugTab = new System.Windows.Forms.TabPage();
            this.debugLog = new System.Windows.Forms.TextBox();
            this.interfaceBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.interfaceBox.Location = new System.Drawing.Point(12, 12);
            this.interfaceBox.Name = "interfaceBox";
            this.interfaceBox.Size = new System.Drawing.Size(299, 144);
            this.interfaceBox.TabIndex = 0;
            this.interfaceBox.TabStop = false;
            this.interfaceBox.Text = "Interface";
            // 
            // interfaceTypeList
            // 
            this.interfaceTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.interfaceTypeList.Enabled = false;
            this.interfaceTypeList.FormattingEnabled = true;
            this.interfaceTypeList.Location = new System.Drawing.Point(6, 103);
            this.interfaceTypeList.Name = "interfaceTypeList";
            this.interfaceTypeList.Size = new System.Drawing.Size(286, 24);
            this.interfaceTypeList.TabIndex = 3;
            this.interfaceTypeList.SelectedIndexChanged += new System.EventHandler(this.interfaceTypeList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Type";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Port";
            // 
            // interfacePortList
            // 
            this.interfacePortList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.interfacePortList.FormattingEnabled = true;
            this.interfacePortList.Location = new System.Drawing.Point(7, 43);
            this.interfacePortList.Name = "interfacePortList";
            this.interfacePortList.Size = new System.Drawing.Size(286, 24);
            this.interfacePortList.TabIndex = 1;
            this.interfacePortList.SelectedIndexChanged += new System.EventHandler(this.interfacePortList_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.writeFullContentsButton);
            this.groupBox1.Controls.Add(this.readFullContentsButton);
            this.groupBox1.Controls.Add(this.modifyVinButton);
            this.groupBox1.Controls.Add(this.readPropertiesButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 162);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(299, 179);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Operations";
            // 
            // writeFullContentsButton
            // 
            this.writeFullContentsButton.Enabled = false;
            this.writeFullContentsButton.Location = new System.Drawing.Point(6, 133);
            this.writeFullContentsButton.Name = "writeFullContentsButton";
            this.writeFullContentsButton.Size = new System.Drawing.Size(285, 31);
            this.writeFullContentsButton.TabIndex = 3;
            this.writeFullContentsButton.Text = "&Write Full Contents";
            this.writeFullContentsButton.UseVisualStyleBackColor = true;
            // 
            // readFullContentsButton
            // 
            this.readFullContentsButton.Enabled = false;
            this.readFullContentsButton.Location = new System.Drawing.Point(7, 59);
            this.readFullContentsButton.Name = "readFullContentsButton";
            this.readFullContentsButton.Size = new System.Drawing.Size(285, 31);
            this.readFullContentsButton.TabIndex = 1;
            this.readFullContentsButton.Text = "&Read Full Contents";
            this.readFullContentsButton.UseVisualStyleBackColor = true;
            this.readFullContentsButton.Click += new System.EventHandler(this.readFullContentsButton_Click);
            // 
            // modifyVinButton
            // 
            this.modifyVinButton.Enabled = false;
            this.modifyVinButton.Location = new System.Drawing.Point(7, 96);
            this.modifyVinButton.Name = "modifyVinButton";
            this.modifyVinButton.Size = new System.Drawing.Size(285, 31);
            this.modifyVinButton.TabIndex = 2;
            this.modifyVinButton.Text = "Modify &VIN";
            this.modifyVinButton.UseVisualStyleBackColor = true;
            // 
            // readPropertiesButton
            // 
            this.readPropertiesButton.Enabled = false;
            this.readPropertiesButton.Location = new System.Drawing.Point(7, 22);
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
            this.tabs.Controls.Add(this.debugTab);
            this.tabs.Location = new System.Drawing.Point(317, 12);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(706, 443);
            this.tabs.TabIndex = 2;
            // 
            // resultsTab
            // 
            this.resultsTab.Controls.Add(this.userLog);
            this.resultsTab.Location = new System.Drawing.Point(4, 25);
            this.resultsTab.Name = "resultsTab";
            this.resultsTab.Padding = new System.Windows.Forms.Padding(3);
            this.resultsTab.Size = new System.Drawing.Size(698, 414);
            this.resultsTab.TabIndex = 0;
            this.resultsTab.Text = "Results";
            this.resultsTab.UseVisualStyleBackColor = true;
            // 
            // userLog
            // 
            this.userLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userLog.Location = new System.Drawing.Point(6, 6);
            this.userLog.Multiline = true;
            this.userLog.Name = "userLog";
            this.userLog.ReadOnly = true;
            this.userLog.Size = new System.Drawing.Size(686, 402);
            this.userLog.TabIndex = 0;
            // 
            // debugTab
            // 
            this.debugTab.Controls.Add(this.debugLog);
            this.debugTab.Location = new System.Drawing.Point(4, 25);
            this.debugTab.Name = "debugTab";
            this.debugTab.Padding = new System.Windows.Forms.Padding(3);
            this.debugTab.Size = new System.Drawing.Size(698, 414);
            this.debugTab.TabIndex = 1;
            this.debugTab.Text = "Debug Log";
            this.debugTab.UseVisualStyleBackColor = true;
            // 
            // debugLog
            // 
            this.debugLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debugLog.Location = new System.Drawing.Point(6, 6);
            this.debugLog.Multiline = true;
            this.debugLog.Name = "debugLog";
            this.debugLog.ReadOnly = true;
            this.debugLog.Size = new System.Drawing.Size(686, 402);
            this.debugLog.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1035, 467);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.interfaceBox);
            this.Name = "MainForm";
            this.Text = "Prototype";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.interfaceBox.ResumeLayout(false);
            this.interfaceBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button writeFullContentsButton;
        private System.Windows.Forms.Button readFullContentsButton;
        private System.Windows.Forms.Button modifyVinButton;
        private System.Windows.Forms.Button readPropertiesButton;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage resultsTab;
        private System.Windows.Forms.TextBox userLog;
        private System.Windows.Forms.TabPage debugTab;
        private System.Windows.Forms.TextBox debugLog;
    }
}

