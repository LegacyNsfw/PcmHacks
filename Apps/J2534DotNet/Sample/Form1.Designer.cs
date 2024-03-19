namespace Sample
{
    partial class Form1
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
            this.cmdDetectDevices = new System.Windows.Forms.Button();
            this.txtDevices = new System.Windows.Forms.TextBox();
            this.cmdReadVoltage = new System.Windows.Forms.Button();
            this.txtVoltage = new System.Windows.Forms.TextBox();
            this.cmdReadVin = new System.Windows.Forms.Button();
            this.txtReadVin = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cmdDetectDevices
            // 
            this.cmdDetectDevices.Location = new System.Drawing.Point(13, 13);
            this.cmdDetectDevices.Name = "cmdDetectDevices";
            this.cmdDetectDevices.Size = new System.Drawing.Size(154, 23);
            this.cmdDetectDevices.TabIndex = 0;
            this.cmdDetectDevices.Text = "Detect J2534 Devices";
            this.cmdDetectDevices.UseVisualStyleBackColor = true;
            this.cmdDetectDevices.Click += new System.EventHandler(this.CmdDetectDevicesClick);
            // 
            // txtDevices
            // 
            this.txtDevices.Location = new System.Drawing.Point(173, 15);
            this.txtDevices.Multiline = true;
            this.txtDevices.Name = "txtDevices";
            this.txtDevices.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDevices.Size = new System.Drawing.Size(429, 160);
            this.txtDevices.TabIndex = 1;
            // 
            // cmdReadVoltage
            // 
            this.cmdReadVoltage.Location = new System.Drawing.Point(12, 188);
            this.cmdReadVoltage.Name = "cmdReadVoltage";
            this.cmdReadVoltage.Size = new System.Drawing.Size(155, 23);
            this.cmdReadVoltage.TabIndex = 2;
            this.cmdReadVoltage.Text = "Read Voltage";
            this.cmdReadVoltage.UseVisualStyleBackColor = true;
            this.cmdReadVoltage.Click += new System.EventHandler(this.CmdReadVoltageClick);
            // 
            // txtVoltage
            // 
            this.txtVoltage.Location = new System.Drawing.Point(173, 190);
            this.txtVoltage.Name = "txtVoltage";
            this.txtVoltage.Size = new System.Drawing.Size(429, 20);
            this.txtVoltage.TabIndex = 3;
            // 
            // cmdReadVin
            // 
            this.cmdReadVin.Location = new System.Drawing.Point(12, 217);
            this.cmdReadVin.Name = "cmdReadVin";
            this.cmdReadVin.Size = new System.Drawing.Size(155, 23);
            this.cmdReadVin.TabIndex = 4;
            this.cmdReadVin.Text = "Read VIN";
            this.cmdReadVin.UseVisualStyleBackColor = true;
            this.cmdReadVin.Click += new System.EventHandler(this.CmdReadVinClick);
            // 
            // txtReadVin
            // 
            this.txtReadVin.Location = new System.Drawing.Point(174, 219);
            this.txtReadVin.Name = "txtReadVin";
            this.txtReadVin.Size = new System.Drawing.Size(428, 20);
            this.txtReadVin.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 430);
            this.Controls.Add(this.txtReadVin);
            this.Controls.Add(this.cmdReadVin);
            this.Controls.Add(this.txtVoltage);
            this.Controls.Add(this.cmdReadVoltage);
            this.Controls.Add(this.txtDevices);
            this.Controls.Add(this.cmdDetectDevices);
            this.Name = "Form1";
            this.Text = "J2534DotNet Sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdDetectDevices;
        private System.Windows.Forms.TextBox txtDevices;
        private System.Windows.Forms.Button cmdReadVoltage;
        private System.Windows.Forms.TextBox txtVoltage;
        private System.Windows.Forms.Button cmdReadVin;
        private System.Windows.Forms.TextBox txtReadVin;
    }
}

