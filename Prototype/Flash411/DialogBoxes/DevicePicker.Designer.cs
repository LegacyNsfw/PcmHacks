namespace Flash411
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
            this.j2534DeviceButton = new System.Windows.Forms.RadioButton();
            this.serialRadioButton = new System.Windows.Forms.RadioButton();
            this.serialOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.serialDeviceList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.serialPortList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.j2534DeviceList = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.autoDetectButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.categories.SuspendLayout();
            this.serialOptionsGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categories
            // 
            this.categories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categories.Controls.Add(this.j2534DeviceButton);
            this.categories.Controls.Add(this.serialRadioButton);
            this.categories.Location = new System.Drawing.Point(12, 12);
            this.categories.Name = "categories";
            this.categories.Size = new System.Drawing.Size(288, 49);
            this.categories.TabIndex = 0;
            this.categories.TabStop = false;
            this.categories.Text = "Device &Category";
            // 
            // j2534DeviceButton
            // 
            this.j2534DeviceButton.AutoSize = true;
            this.j2534DeviceButton.Location = new System.Drawing.Point(142, 20);
            this.j2534DeviceButton.Name = "j2534DeviceButton";
            this.j2534DeviceButton.Size = new System.Drawing.Size(91, 17);
            this.j2534DeviceButton.TabIndex = 1;
            this.j2534DeviceButton.TabStop = true;
            this.j2534DeviceButton.Text = "&J2534 Device";
            this.j2534DeviceButton.UseVisualStyleBackColor = true;
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
            // 
            // serialOptionsGroupBox
            // 
            this.serialOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serialOptionsGroupBox.Controls.Add(this.serialDeviceList);
            this.serialOptionsGroupBox.Controls.Add(this.label2);
            this.serialOptionsGroupBox.Controls.Add(this.serialPortList);
            this.serialOptionsGroupBox.Controls.Add(this.label1);
            this.serialOptionsGroupBox.Location = new System.Drawing.Point(13, 68);
            this.serialOptionsGroupBox.Name = "serialOptionsGroupBox";
            this.serialOptionsGroupBox.Size = new System.Drawing.Size(287, 122);
            this.serialOptionsGroupBox.TabIndex = 1;
            this.serialOptionsGroupBox.TabStop = false;
            this.serialOptionsGroupBox.Text = "Serial Device Options";
            // 
            // serialDeviceList
            // 
            this.serialDeviceList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serialDeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serialDeviceList.FormattingEnabled = true;
            this.serialDeviceList.Location = new System.Drawing.Point(10, 82);
            this.serialDeviceList.Name = "serialDeviceList";
            this.serialDeviceList.Size = new System.Drawing.Size(271, 21);
            this.serialDeviceList.TabIndex = 3;
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
            this.serialPortList.Location = new System.Drawing.Point(10, 36);
            this.serialPortList.Name = "serialPortList";
            this.serialPortList.Size = new System.Drawing.Size(130, 21);
            this.serialPortList.TabIndex = 1;
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
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.j2534DeviceList);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 197);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(288, 72);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "J2534 Device Options";
            // 
            // j2534DeviceList
            // 
            this.j2534DeviceList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.j2534DeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.j2534DeviceList.FormattingEnabled = true;
            this.j2534DeviceList.Location = new System.Drawing.Point(7, 37);
            this.j2534DeviceList.Name = "j2534DeviceList";
            this.j2534DeviceList.Size = new System.Drawing.Size(275, 21);
            this.j2534DeviceList.TabIndex = 1;
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
            this.autoDetectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autoDetectButton.Location = new System.Drawing.Point(12, 295);
            this.autoDetectButton.Name = "autoDetectButton";
            this.autoDetectButton.Size = new System.Drawing.Size(75, 23);
            this.autoDetectButton.TabIndex = 3;
            this.autoDetectButton.Text = "&Auto Detect";
            this.autoDetectButton.UseVisualStyleBackColor = true;
            this.autoDetectButton.Click += new System.EventHandler(this.autoDetectButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(136, 294);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(217, 294);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(83, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // DevicePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 330);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.autoDetectButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.serialOptionsGroupBox);
            this.Controls.Add(this.categories);
            this.Name = "DevicePicker";
            this.Text = "Select Device";
            this.Load += new System.EventHandler(this.DevicePicker_Load);
            this.categories.ResumeLayout(false);
            this.categories.PerformLayout();
            this.serialOptionsGroupBox.ResumeLayout(false);
            this.serialOptionsGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox categories;
        private System.Windows.Forms.RadioButton j2534DeviceButton;
        private System.Windows.Forms.RadioButton serialRadioButton;
        private System.Windows.Forms.GroupBox serialOptionsGroupBox;
        private System.Windows.Forms.ComboBox serialDeviceList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox serialPortList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox j2534DeviceList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button autoDetectButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}