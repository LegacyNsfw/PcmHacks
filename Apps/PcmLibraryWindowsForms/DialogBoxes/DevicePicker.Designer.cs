namespace PcmHacking
{
    partial class DevicePicker
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
            this.categories = new System.Windows.Forms.GroupBox();
            this.j2534RadioButton = new System.Windows.Forms.RadioButton();
            this.serialRadioButton = new System.Windows.Forms.RadioButton();
            this.serialOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.serialDeviceList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.serialPortList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.j2534OptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.j2534DeviceList = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.autoDetectButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.enable4xReadWriteCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.categories.SuspendLayout();
            this.serialOptionsGroupBox.SuspendLayout();
            this.j2534OptionsGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // categories
            // 
            this.categories.Controls.Add(this.j2534RadioButton);
            this.categories.Controls.Add(this.serialRadioButton);
            this.categories.Location = new System.Drawing.Point(12, 12);
            this.categories.Name = "categories";
            this.categories.Size = new System.Drawing.Size(283, 49);
            this.categories.TabIndex = 0;
            this.categories.TabStop = false;
            this.categories.Text = "Device &Category";
            // 
            // j2534RadioButton
            // 
            this.j2534RadioButton.AutoSize = true;
            this.j2534RadioButton.Location = new System.Drawing.Point(142, 20);
            this.j2534RadioButton.Name = "j2534RadioButton";
            this.j2534RadioButton.Size = new System.Drawing.Size(91, 17);
            this.j2534RadioButton.TabIndex = 1;
            this.j2534RadioButton.TabStop = true;
            this.j2534RadioButton.Text = "&J2534 Device";
            this.j2534RadioButton.UseVisualStyleBackColor = true;
            this.j2534RadioButton.CheckedChanged += new System.EventHandler(this.j2534RadioButton_CheckedChanged);
            // 
            // serialRadioButton
            // 
            this.serialRadioButton.AutoSize = true;
            this.serialRadioButton.Location = new System.Drawing.Point(7, 20);
            this.serialRadioButton.Name = "serialRadioButton";
            this.serialRadioButton.Size = new System.Drawing.Size(110, 17);
            this.serialRadioButton.TabIndex = 0;
            this.serialRadioButton.TabStop = true;
            this.serialRadioButton.Text = "&Serial Port Device";
            this.serialRadioButton.UseVisualStyleBackColor = true;
            this.serialRadioButton.CheckedChanged += new System.EventHandler(this.serialRadioButton_CheckedChanged);
            // 
            // serialOptionsGroupBox
            // 
            this.serialOptionsGroupBox.Controls.Add(this.serialDeviceList);
            this.serialOptionsGroupBox.Controls.Add(this.label2);
            this.serialOptionsGroupBox.Controls.Add(this.serialPortList);
            this.serialOptionsGroupBox.Controls.Add(this.label1);
            this.serialOptionsGroupBox.Location = new System.Drawing.Point(12, 68);
            this.serialOptionsGroupBox.Name = "serialOptionsGroupBox";
            this.serialOptionsGroupBox.Size = new System.Drawing.Size(283, 122);
            this.serialOptionsGroupBox.TabIndex = 1;
            this.serialOptionsGroupBox.TabStop = false;
            this.serialOptionsGroupBox.Text = "Serial Device Options";
            // 
            // serialDeviceList
            // 
            this.serialDeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serialDeviceList.FormattingEnabled = true;
            this.serialDeviceList.Location = new System.Drawing.Point(7, 82);
            this.serialDeviceList.Name = "serialDeviceList";
            this.serialDeviceList.Size = new System.Drawing.Size(266, 21);
            this.serialDeviceList.TabIndex = 3;
            this.serialDeviceList.SelectedIndexChanged += new System.EventHandler(this.serialDeviceList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Device Type";
            // 
            // serialPortList
            // 
            this.serialPortList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serialPortList.FormattingEnabled = true;
            this.serialPortList.Location = new System.Drawing.Point(7, 36);
            this.serialPortList.Name = "serialPortList";
            this.serialPortList.Size = new System.Drawing.Size(266, 21);
            this.serialPortList.TabIndex = 1;
            this.serialPortList.SelectedIndexChanged += new System.EventHandler(this.serialPortList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Serial &Port";
            // 
            // j2534OptionsGroupBox
            // 
            this.j2534OptionsGroupBox.Controls.Add(this.j2534DeviceList);
            this.j2534OptionsGroupBox.Controls.Add(this.label3);
            this.j2534OptionsGroupBox.Location = new System.Drawing.Point(12, 197);
            this.j2534OptionsGroupBox.Name = "j2534OptionsGroupBox";
            this.j2534OptionsGroupBox.Size = new System.Drawing.Size(283, 72);
            this.j2534OptionsGroupBox.TabIndex = 2;
            this.j2534OptionsGroupBox.TabStop = false;
            this.j2534OptionsGroupBox.Text = "J2534 Device Options";
            // 
            // j2534DeviceList
            // 
            this.j2534DeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.j2534DeviceList.FormattingEnabled = true;
            this.j2534DeviceList.Location = new System.Drawing.Point(7, 37);
            this.j2534DeviceList.Name = "j2534DeviceList";
            this.j2534DeviceList.Size = new System.Drawing.Size(270, 21);
            this.j2534DeviceList.TabIndex = 1;
            this.j2534DeviceList.SelectedIndexChanged += new System.EventHandler(this.j2534DeviceList_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "&Device Type";
            // 
            // autoDetectButton
            // 
            this.autoDetectButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.autoDetectButton.Location = new System.Drawing.Point(13, 330);
            this.autoDetectButton.Name = "autoDetectButton";
            this.autoDetectButton.Size = new System.Drawing.Size(94, 23);
            this.autoDetectButton.TabIndex = 3;
            this.autoDetectButton.Text = "&Auto Detect";
            this.autoDetectButton.UseVisualStyleBackColor = true;
            this.autoDetectButton.Click += new System.EventHandler(this.autoDetectButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(301, 12);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(94, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(301, 41);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(94, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // testButton
            // 
            this.testButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.testButton.Location = new System.Drawing.Point(201, 330);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(94, 23);
            this.testButton.TabIndex = 4;
            this.testButton.Text = "&Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(12, 276);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 13);
            this.status.TabIndex = 7;
            // 
            // enable4xReadWriteCheckBox
            // 
            this.enable4xReadWriteCheckBox.AutoSize = true;
            this.enable4xReadWriteCheckBox.Location = new System.Drawing.Point(7, 19);
            this.enable4xReadWriteCheckBox.Name = "enable4xReadWriteCheckBox";
            this.enable4xReadWriteCheckBox.Size = new System.Drawing.Size(148, 17);
            this.enable4xReadWriteCheckBox.TabIndex = 8;
            this.enable4xReadWriteCheckBox.Text = "Enable &4x Communication";
            this.enable4xReadWriteCheckBox.UseVisualStyleBackColor = true;
            this.enable4xReadWriteCheckBox.CheckedChanged += new System.EventHandler(this.enable4xReadWriteCheckBox_CheckedChanged);
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.enable4xReadWriteCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(12, 275);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(283, 39);
            this.optionsGroupBox.TabIndex = 9;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // DevicePicker
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 365);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.status);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.autoDetectButton);
            this.Controls.Add(this.j2534OptionsGroupBox);
            this.Controls.Add(this.serialOptionsGroupBox);
            this.Controls.Add(this.categories);
            this.Name = "DevicePicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Device";
            this.Load += new System.EventHandler(this.DevicePicker_Load);
            this.categories.ResumeLayout(false);
            this.categories.PerformLayout();
            this.serialOptionsGroupBox.ResumeLayout(false);
            this.serialOptionsGroupBox.PerformLayout();
            this.j2534OptionsGroupBox.ResumeLayout(false);
            this.j2534OptionsGroupBox.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox categories;
        private System.Windows.Forms.RadioButton j2534RadioButton;
        private System.Windows.Forms.RadioButton serialRadioButton;
        private System.Windows.Forms.GroupBox serialOptionsGroupBox;
        private System.Windows.Forms.ComboBox serialDeviceList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox serialPortList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox j2534OptionsGroupBox;
        private System.Windows.Forms.ComboBox j2534DeviceList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button autoDetectButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.CheckBox enable4xReadWriteCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
    }
}