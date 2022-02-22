namespace PcmHacking.DialogBoxes
{
    partial class SettingsDialogBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialogBox));
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.generalTabPage = new System.Windows.Forms.TabPage();
            this.windowGroupBox = new System.Windows.Forms.GroupBox();
            this.mainWindowPersistenceCheckBox = new System.Windows.Forms.CheckBox();
            this.binGroupBox = new System.Windows.Forms.GroupBox();
            this.binDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.binDirectoryButton = new System.Windows.Forms.Button();
            this.logGroupBox = new System.Windows.Forms.GroupBox();
            this.saveUserLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.useLogSaveAsDialogCheckBox = new System.Windows.Forms.CheckBox();
            this.logDirectoryButton = new System.Windows.Forms.Button();
            this.saveDebugLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.logDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            this.windowGroupBox.SuspendLayout();
            this.binGroupBox.SuspendLayout();
            this.logGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(371, 270);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(290, 270);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(209, 270);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.generalTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(434, 252);
            this.tabControl.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.windowGroupBox);
            this.generalTabPage.Controls.Add(this.binGroupBox);
            this.generalTabPage.Controls.Add(this.logGroupBox);
            this.generalTabPage.Location = new System.Drawing.Point(4, 22);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(426, 226);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "General";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // windowGroupBox
            // 
            this.windowGroupBox.Controls.Add(this.mainWindowPersistenceCheckBox);
            this.windowGroupBox.Location = new System.Drawing.Point(6, 6);
            this.windowGroupBox.Name = "windowGroupBox";
            this.windowGroupBox.Size = new System.Drawing.Size(414, 41);
            this.windowGroupBox.TabIndex = 0;
            this.windowGroupBox.TabStop = false;
            this.windowGroupBox.Text = "Window";
            // 
            // mainWindowPersistenceCheckBox
            // 
            this.mainWindowPersistenceCheckBox.AutoSize = true;
            this.mainWindowPersistenceCheckBox.Location = new System.Drawing.Point(6, 19);
            this.mainWindowPersistenceCheckBox.Name = "mainWindowPersistenceCheckBox";
            this.mainWindowPersistenceCheckBox.Size = new System.Drawing.Size(149, 17);
            this.mainWindowPersistenceCheckBox.TabIndex = 0;
            this.mainWindowPersistenceCheckBox.Text = "Main Window Persistence";
            this.mainWindowPersistenceCheckBox.UseVisualStyleBackColor = true;
            this.mainWindowPersistenceCheckBox.CheckedChanged += new System.EventHandler(this.mainWindowPersistenceCheckBox_CheckedChanged);
            // 
            // binGroupBox
            // 
            this.binGroupBox.Controls.Add(this.binDirectoryTextBox);
            this.binGroupBox.Controls.Add(this.binDirectoryButton);
            this.binGroupBox.Location = new System.Drawing.Point(6, 53);
            this.binGroupBox.Name = "binGroupBox";
            this.binGroupBox.Size = new System.Drawing.Size(414, 45);
            this.binGroupBox.TabIndex = 2;
            this.binGroupBox.TabStop = false;
            this.binGroupBox.Text = "Bin";
            // 
            // binDirectoryTextBox
            // 
            this.binDirectoryTextBox.Location = new System.Drawing.Point(127, 19);
            this.binDirectoryTextBox.Name = "binDirectoryTextBox";
            this.binDirectoryTextBox.Size = new System.Drawing.Size(280, 20);
            this.binDirectoryTextBox.TabIndex = 1;
            this.binDirectoryTextBox.TextChanged += new System.EventHandler(this.binDirectoryTextBox_TextChanged);
            // 
            // binDirectoryButton
            // 
            this.binDirectoryButton.Location = new System.Drawing.Point(6, 16);
            this.binDirectoryButton.Name = "binDirectoryButton";
            this.binDirectoryButton.Size = new System.Drawing.Size(115, 23);
            this.binDirectoryButton.TabIndex = 0;
            this.binDirectoryButton.Text = "Bin Directory";
            this.binDirectoryButton.UseVisualStyleBackColor = true;
            this.binDirectoryButton.Click += new System.EventHandler(this.binDirectoryButton_Click);
            // 
            // logGroupBox
            // 
            this.logGroupBox.Controls.Add(this.saveUserLogOnExitCheckBox);
            this.logGroupBox.Controls.Add(this.useLogSaveAsDialogCheckBox);
            this.logGroupBox.Controls.Add(this.logDirectoryButton);
            this.logGroupBox.Controls.Add(this.saveDebugLogOnExitCheckBox);
            this.logGroupBox.Controls.Add(this.logDirectoryTextBox);
            this.logGroupBox.Location = new System.Drawing.Point(6, 104);
            this.logGroupBox.Name = "logGroupBox";
            this.logGroupBox.Size = new System.Drawing.Size(414, 116);
            this.logGroupBox.TabIndex = 1;
            this.logGroupBox.TabStop = false;
            this.logGroupBox.Text = "Log";
            // 
            // saveUserLogOnExitCheckBox
            // 
            this.saveUserLogOnExitCheckBox.AutoSize = true;
            this.saveUserLogOnExitCheckBox.Location = new System.Drawing.Point(6, 42);
            this.saveUserLogOnExitCheckBox.Name = "saveUserLogOnExitCheckBox";
            this.saveUserLogOnExitCheckBox.Size = new System.Drawing.Size(189, 17);
            this.saveUserLogOnExitCheckBox.TabIndex = 1;
            this.saveUserLogOnExitCheckBox.Text = "Save results log on application exit (always silent)";
            this.saveUserLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveUserLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveUserLogOnExitCheckBox_CheckedChanged);
            // 
            // useLogSaveAsDialogCheckBox
            // 
            this.useLogSaveAsDialogCheckBox.AutoSize = true;
            this.useLogSaveAsDialogCheckBox.Location = new System.Drawing.Point(6, 19);
            this.useLogSaveAsDialogCheckBox.Name = "useLogSaveAsDialogCheckBox";
            this.useLogSaveAsDialogCheckBox.Size = new System.Drawing.Size(137, 17);
            this.useLogSaveAsDialogCheckBox.TabIndex = 0;
            this.useLogSaveAsDialogCheckBox.Text = "Use Log SaveAs Dialog";
            this.useLogSaveAsDialogCheckBox.UseVisualStyleBackColor = true;
            this.useLogSaveAsDialogCheckBox.CheckedChanged += new System.EventHandler(this.useLogSaveAsDialogCheckBox_CheckedChanged);
            // 
            // logDirectoryButton
            // 
            this.logDirectoryButton.Location = new System.Drawing.Point(6, 87);
            this.logDirectoryButton.Name = "logDirectoryButton";
            this.logDirectoryButton.Size = new System.Drawing.Size(115, 23);
            this.logDirectoryButton.TabIndex = 3;
            this.logDirectoryButton.Text = "Log Directory";
            this.logDirectoryButton.UseVisualStyleBackColor = true;
            this.logDirectoryButton.Click += new System.EventHandler(this.logDirectoryButton_Click);
            // 
            // saveDebugLogOnExitCheckBox
            // 
            this.saveDebugLogOnExitCheckBox.AutoSize = true;
            this.saveDebugLogOnExitCheckBox.Location = new System.Drawing.Point(6, 65);
            this.saveDebugLogOnExitCheckBox.Name = "saveDebugLogOnExitCheckBox";
            this.saveDebugLogOnExitCheckBox.Size = new System.Drawing.Size(189, 17);
            this.saveDebugLogOnExitCheckBox.TabIndex = 2;
            this.saveDebugLogOnExitCheckBox.Text = "Save debug log on application exit (always silent)";
            this.saveDebugLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveDebugLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveDebugLogOnExitCheckBox_CheckedChanged);
            // 
            // logDirectoryTextBox
            // 
            this.logDirectoryTextBox.Location = new System.Drawing.Point(127, 90);
            this.logDirectoryTextBox.Name = "logDirectoryTextBox";
            this.logDirectoryTextBox.Size = new System.Drawing.Size(280, 20);
            this.logDirectoryTextBox.TabIndex = 4;
            this.logDirectoryTextBox.TextChanged += new System.EventHandler(this.logDirectoryTextBox_TextChanged);
            // 
            // SettingsDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 305);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.applyButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsDialogBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsDialogBox";
            this.Load += new System.EventHandler(this.SettingsDialogBox_Load);
            this.tabControl.ResumeLayout(false);
            this.generalTabPage.ResumeLayout(false);
            this.windowGroupBox.ResumeLayout(false);
            this.windowGroupBox.PerformLayout();
            this.binGroupBox.ResumeLayout(false);
            this.binGroupBox.PerformLayout();
            this.logGroupBox.ResumeLayout(false);
            this.logGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage generalTabPage;
        private System.Windows.Forms.Button logDirectoryButton;
        private System.Windows.Forms.TextBox logDirectoryTextBox;
        private System.Windows.Forms.CheckBox saveUserLogOnExitCheckBox;
        private System.Windows.Forms.CheckBox saveDebugLogOnExitCheckBox;
        private System.Windows.Forms.CheckBox mainWindowPersistenceCheckBox;
        private System.Windows.Forms.TextBox binDirectoryTextBox;
        private System.Windows.Forms.Button binDirectoryButton;
        private System.Windows.Forms.CheckBox useLogSaveAsDialogCheckBox;
        private System.Windows.Forms.GroupBox windowGroupBox;
        private System.Windows.Forms.GroupBox binGroupBox;
        private System.Windows.Forms.GroupBox logGroupBox;
    }
}