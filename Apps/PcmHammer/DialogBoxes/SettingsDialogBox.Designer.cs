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
            this.mainWindowPersistenceCheckBox = new System.Windows.Forms.CheckBox();
            this.saveUserLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.saveDebugLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.logDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.logDirectoryButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(371, 212);
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
            this.cancelButton.Location = new System.Drawing.Point(290, 212);
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
            this.okButton.Location = new System.Drawing.Point(209, 212);
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
            this.tabControl.Size = new System.Drawing.Size(434, 194);
            this.tabControl.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.mainWindowPersistenceCheckBox);
            this.generalTabPage.Controls.Add(this.saveUserLogOnExitCheckBox);
            this.generalTabPage.Controls.Add(this.saveDebugLogOnExitCheckBox);
            this.generalTabPage.Controls.Add(this.logDirectoryTextBox);
            this.generalTabPage.Controls.Add(this.logDirectoryButton);
            this.generalTabPage.Location = new System.Drawing.Point(4, 22);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(426, 168);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "General";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // mainWindowPersistenceCheckBox
            // 
            this.mainWindowPersistenceCheckBox.AutoSize = true;
            this.mainWindowPersistenceCheckBox.Location = new System.Drawing.Point(7, 7);
            this.mainWindowPersistenceCheckBox.Name = "mainWindowPersistenceCheckBox";
            this.mainWindowPersistenceCheckBox.Size = new System.Drawing.Size(149, 17);
            this.mainWindowPersistenceCheckBox.TabIndex = 0;
            this.mainWindowPersistenceCheckBox.Text = "Main Window Persistence";
            this.mainWindowPersistenceCheckBox.UseVisualStyleBackColor = true;
            this.mainWindowPersistenceCheckBox.CheckedChanged += new System.EventHandler(this.mainWindowPersistenceCheckBox_CheckedChanged);
            // 
            // saveUserLogOnExitCheckBox
            // 
            this.saveUserLogOnExitCheckBox.AutoSize = true;
            this.saveUserLogOnExitCheckBox.Location = new System.Drawing.Point(7, 93);
            this.saveUserLogOnExitCheckBox.Name = "saveUserLogOnExitCheckBox";
            this.saveUserLogOnExitCheckBox.Size = new System.Drawing.Size(189, 17);
            this.saveUserLogOnExitCheckBox.TabIndex = 1;
            this.saveUserLogOnExitCheckBox.Text = "Save results log on application exit";
            this.saveUserLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveUserLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveUserLogOnExitCheckBox_CheckedChanged);
            // 
            // saveDebugLogOnExitCheckBox
            // 
            this.saveDebugLogOnExitCheckBox.AutoSize = true;
            this.saveDebugLogOnExitCheckBox.Location = new System.Drawing.Point(7, 116);
            this.saveDebugLogOnExitCheckBox.Name = "saveDebugLogOnExitCheckBox";
            this.saveDebugLogOnExitCheckBox.Size = new System.Drawing.Size(189, 17);
            this.saveDebugLogOnExitCheckBox.TabIndex = 2;
            this.saveDebugLogOnExitCheckBox.Text = "Save debug log on application exit";
            this.saveDebugLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveDebugLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveDebugLogOnExitCheckBox_CheckedChanged);
            // 
            // logDirectoryTextBox
            // 
            this.logDirectoryTextBox.Location = new System.Drawing.Point(128, 142);
            this.logDirectoryTextBox.Name = "logDirectoryTextBox";
            this.logDirectoryTextBox.Size = new System.Drawing.Size(292, 20);
            this.logDirectoryTextBox.TabIndex = 4;
            this.logDirectoryTextBox.TextChanged += new System.EventHandler(this.logDirectoryTextBox_TextChanged);
            // 
            // logDirectoryButton
            // 
            this.logDirectoryButton.Location = new System.Drawing.Point(7, 139);
            this.logDirectoryButton.Name = "logDirectoryButton";
            this.logDirectoryButton.Size = new System.Drawing.Size(115, 23);
            this.logDirectoryButton.TabIndex = 3;
            this.logDirectoryButton.Text = "Log Directory";
            this.logDirectoryButton.UseVisualStyleBackColor = true;
            this.logDirectoryButton.Click += new System.EventHandler(this.logDirectoryButton_Click);
            // 
            // SettingsDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 247);
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
            this.generalTabPage.PerformLayout();
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
    }
}