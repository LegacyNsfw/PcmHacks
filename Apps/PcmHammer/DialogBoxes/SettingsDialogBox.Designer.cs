
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.generalTabPage = new System.Windows.Forms.TabPage();
            this.logGroupBox = new System.Windows.Forms.GroupBox();
            this.logDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.logDirectoryButton = new System.Windows.Forms.Button();
            this.saveDebugLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.saveUserLogOnExitCheckBox = new System.Windows.Forms.CheckBox();
            this.useLogSaveAsDialogCheckBox = new System.Windows.Forms.CheckBox();
            this.binGroupBox = new System.Windows.Forms.GroupBox();
            this.binDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.binDirectoryButton = new System.Windows.Forms.Button();
            this.windowGroupBox = new System.Windows.Forms.GroupBox();
            this.mainWindowPersistenceCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            this.logGroupBox.SuspendLayout();
            this.binGroupBox.SuspendLayout();
            this.windowGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.generalTabPage);
            this.tabControl.ItemSize = new System.Drawing.Size(79, 29);
            this.tabControl.Location = new System.Drawing.Point(22, 22);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(796, 465);
            this.tabControl.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.logGroupBox);
            this.generalTabPage.Controls.Add(this.binGroupBox);
            this.generalTabPage.Controls.Add(this.windowGroupBox);
            this.generalTabPage.Location = new System.Drawing.Point(4, 33);
            this.generalTabPage.Margin = new System.Windows.Forms.Padding(6);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(788, 428);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "General";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // logGroupBox
            // 
            this.logGroupBox.Controls.Add(this.logDirectoryTextBox);
            this.logGroupBox.Controls.Add(this.logDirectoryButton);
            this.logGroupBox.Controls.Add(this.saveDebugLogOnExitCheckBox);
            this.logGroupBox.Controls.Add(this.saveUserLogOnExitCheckBox);
            this.logGroupBox.Controls.Add(this.useLogSaveAsDialogCheckBox);
            this.logGroupBox.Location = new System.Drawing.Point(11, 192);
            this.logGroupBox.Margin = new System.Windows.Forms.Padding(6);
            this.logGroupBox.Name = "logGroupBox";
            this.logGroupBox.Padding = new System.Windows.Forms.Padding(6);
            this.logGroupBox.Size = new System.Drawing.Size(759, 214);
            this.logGroupBox.TabIndex = 2;
            this.logGroupBox.TabStop = false;
            this.logGroupBox.Text = "Log";
            // 
            // logDirectoryTextBox
            // 
            this.logDirectoryTextBox.Location = new System.Drawing.Point(233, 166);
            this.logDirectoryTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.logDirectoryTextBox.Name = "logDirectoryTextBox";
            this.logDirectoryTextBox.Size = new System.Drawing.Size(510, 35);
            this.logDirectoryTextBox.TabIndex = 4;
            this.logDirectoryTextBox.TextChanged += new System.EventHandler(this.logDirectoryTextBox_TextChanged);
            // 
            // logDirectoryButton
            // 
            this.logDirectoryButton.Location = new System.Drawing.Point(11, 161);
            this.logDirectoryButton.Margin = new System.Windows.Forms.Padding(6);
            this.logDirectoryButton.Name = "logDirectoryButton";
            this.logDirectoryButton.Size = new System.Drawing.Size(211, 42);
            this.logDirectoryButton.TabIndex = 3;
            this.logDirectoryButton.Text = "Log Directory";
            this.logDirectoryButton.UseVisualStyleBackColor = true;
            this.logDirectoryButton.Click += new System.EventHandler(this.logDirectoryButton_Click);
            // 
            // saveDebugLogOnExitCheckBox
            // 
            this.saveDebugLogOnExitCheckBox.AutoSize = true;
            this.saveDebugLogOnExitCheckBox.Location = new System.Drawing.Point(11, 120);
            this.saveDebugLogOnExitCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.saveDebugLogOnExitCheckBox.Name = "saveDebugLogOnExitCheckBox";
            this.saveDebugLogOnExitCheckBox.Size = new System.Drawing.Size(493, 34);
            this.saveDebugLogOnExitCheckBox.TabIndex = 2;
            this.saveDebugLogOnExitCheckBox.Text = "Save debug log on application exit (always silent)";
            this.saveDebugLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveDebugLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveDebugLogOnExitCheckBox_CheckedChanged);
            // 
            // saveUserLogOnExitCheckBox
            // 
            this.saveUserLogOnExitCheckBox.AutoSize = true;
            this.saveUserLogOnExitCheckBox.Location = new System.Drawing.Point(11, 78);
            this.saveUserLogOnExitCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.saveUserLogOnExitCheckBox.Name = "saveUserLogOnExitCheckBox";
            this.saveUserLogOnExitCheckBox.Size = new System.Drawing.Size(494, 34);
            this.saveUserLogOnExitCheckBox.TabIndex = 1;
            this.saveUserLogOnExitCheckBox.Text = "Save results log on application exit (always silent)";
            this.saveUserLogOnExitCheckBox.UseVisualStyleBackColor = true;
            this.saveUserLogOnExitCheckBox.CheckedChanged += new System.EventHandler(this.saveUserLogOnExitCheckBox_CheckedChanged);
            // 
            // useLogSaveAsDialogCheckBox
            // 
            this.useLogSaveAsDialogCheckBox.AutoSize = true;
            this.useLogSaveAsDialogCheckBox.Location = new System.Drawing.Point(11, 35);
            this.useLogSaveAsDialogCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.useLogSaveAsDialogCheckBox.Name = "useLogSaveAsDialogCheckBox";
            this.useLogSaveAsDialogCheckBox.Size = new System.Drawing.Size(240, 34);
            this.useLogSaveAsDialogCheckBox.TabIndex = 0;
            this.useLogSaveAsDialogCheckBox.Text = "Us Log SaveAs Dialog";
            this.useLogSaveAsDialogCheckBox.UseVisualStyleBackColor = true;
            this.useLogSaveAsDialogCheckBox.CheckedChanged += new System.EventHandler(this.useLogSaveAsDialogCheckBox_CheckedChanged);
            // 
            // binGroupBox
            // 
            this.binGroupBox.Controls.Add(this.binDirectoryTextBox);
            this.binGroupBox.Controls.Add(this.binDirectoryButton);
            this.binGroupBox.Location = new System.Drawing.Point(11, 98);
            this.binGroupBox.Margin = new System.Windows.Forms.Padding(6);
            this.binGroupBox.Name = "binGroupBox";
            this.binGroupBox.Padding = new System.Windows.Forms.Padding(6);
            this.binGroupBox.Size = new System.Drawing.Size(759, 83);
            this.binGroupBox.TabIndex = 1;
            this.binGroupBox.TabStop = false;
            this.binGroupBox.Text = "bin";
            // 
            // binDirectoryTextBox
            // 
            this.binDirectoryTextBox.Location = new System.Drawing.Point(233, 35);
            this.binDirectoryTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.binDirectoryTextBox.Name = "binDirectoryTextBox";
            this.binDirectoryTextBox.Size = new System.Drawing.Size(510, 35);
            this.binDirectoryTextBox.TabIndex = 1;
            // 
            // binDirectoryButton
            // 
            this.binDirectoryButton.Location = new System.Drawing.Point(11, 30);
            this.binDirectoryButton.Margin = new System.Windows.Forms.Padding(6);
            this.binDirectoryButton.Name = "binDirectoryButton";
            this.binDirectoryButton.Size = new System.Drawing.Size(211, 42);
            this.binDirectoryButton.TabIndex = 0;
            this.binDirectoryButton.Text = "Bin Directory";
            this.binDirectoryButton.UseVisualStyleBackColor = true;
            this.binDirectoryButton.Click += new System.EventHandler(this.binDirectoryButton_Click);
            // 
            // windowGroupBox
            // 
            this.windowGroupBox.Controls.Add(this.mainWindowPersistenceCheckBox);
            this.windowGroupBox.Location = new System.Drawing.Point(11, 11);
            this.windowGroupBox.Margin = new System.Windows.Forms.Padding(6);
            this.windowGroupBox.Name = "windowGroupBox";
            this.windowGroupBox.Padding = new System.Windows.Forms.Padding(6);
            this.windowGroupBox.Size = new System.Drawing.Size(759, 76);
            this.windowGroupBox.TabIndex = 0;
            this.windowGroupBox.TabStop = false;
            this.windowGroupBox.Text = "Window";
            // 
            // mainWindowPersistenceCheckBox
            // 
            this.mainWindowPersistenceCheckBox.AutoSize = true;
            this.mainWindowPersistenceCheckBox.Location = new System.Drawing.Point(11, 35);
            this.mainWindowPersistenceCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.mainWindowPersistenceCheckBox.Name = "mainWindowPersistenceCheckBox";
            this.mainWindowPersistenceCheckBox.Size = new System.Drawing.Size(277, 34);
            this.mainWindowPersistenceCheckBox.TabIndex = 0;
            this.mainWindowPersistenceCheckBox.Text = "Main Window Persistence";
            this.mainWindowPersistenceCheckBox.UseVisualStyleBackColor = true;
            this.mainWindowPersistenceCheckBox.CheckedChanged += new System.EventHandler(this.mainWindowPersistenceCheckBox_CheckedChanged);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(383, 498);
            this.okButton.Margin = new System.Windows.Forms.Padding(6);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(138, 42);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(532, 498);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(138, 42);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(680, 498);
            this.applyButton.Margin = new System.Windows.Forms.Padding(6);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(138, 42);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            // 
            // SettingsDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 563);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tabControl);
            this.Name = "SettingsDialogBox";
            this.Text = "SettingsDialogBox";
            this.tabControl.ResumeLayout(false);
            this.generalTabPage.ResumeLayout(false);
            this.logGroupBox.ResumeLayout(false);
            this.logGroupBox.PerformLayout();
            this.binGroupBox.ResumeLayout(false);
            this.binGroupBox.PerformLayout();
            this.windowGroupBox.ResumeLayout(false);
            this.windowGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage generalTabPage;
        private System.Windows.Forms.GroupBox logGroupBox;
        private System.Windows.Forms.TextBox logDirectoryTextBox;
        private System.Windows.Forms.Button logDirectoryButton;
        private System.Windows.Forms.CheckBox saveDebugLogOnExitCheckBox;
        private System.Windows.Forms.CheckBox saveUserLogOnExitCheckBox;
        private System.Windows.Forms.CheckBox useLogSaveAsDialogCheckBox;
        private System.Windows.Forms.GroupBox binGroupBox;
        private System.Windows.Forms.TextBox binDirectoryTextBox;
        private System.Windows.Forms.Button binDirectoryButton;
        private System.Windows.Forms.GroupBox windowGroupBox;
        private System.Windows.Forms.CheckBox mainWindowPersistenceCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
    }
}