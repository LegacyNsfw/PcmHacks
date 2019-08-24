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
            this.stopLogging = new System.Windows.Forms.Button();
            this.startLogging = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.logValues = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // stopLogging
            // 
            this.stopLogging.Location = new System.Drawing.Point(12, 86);
            this.stopLogging.Name = "stopLogging";
            this.stopLogging.Size = new System.Drawing.Size(215, 23);
            this.stopLogging.TabIndex = 5;
            this.stopLogging.Text = "Stop Logging";
            this.stopLogging.UseVisualStyleBackColor = true;
            this.stopLogging.Click += new System.EventHandler(this.stopLogging_Click);
            // 
            // startLogging
            // 
            this.startLogging.Location = new System.Drawing.Point(12, 57);
            this.startLogging.Name = "startLogging";
            this.startLogging.Size = new System.Drawing.Size(215, 23);
            this.startLogging.TabIndex = 4;
            this.startLogging.Text = "Start Logging";
            this.startLogging.UseVisualStyleBackColor = true;
            this.startLogging.Click += new System.EventHandler(this.startLogging_Click);
            // 
            // deviceDescription
            // 
            this.deviceDescription.AutoSize = true;
            this.deviceDescription.Location = new System.Drawing.Point(234, 18);
            this.deviceDescription.Name = "deviceDescription";
            this.deviceDescription.Size = new System.Drawing.Size(88, 13);
            this.deviceDescription.TabIndex = 3;
            this.deviceDescription.Text = "[selected device]";
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(12, 12);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(216, 25);
            this.selectButton.TabIndex = 2;
            this.selectButton.Text = "&Select Device";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // logValues
            // 
            this.logValues.Location = new System.Drawing.Point(237, 59);
            this.logValues.Multiline = true;
            this.logValues.Name = "logValues";
            this.logValues.ReadOnly = true;
            this.logValues.Size = new System.Drawing.Size(318, 206);
            this.logValues.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 277);
            this.Controls.Add(this.logValues);
            this.Controls.Add(this.stopLogging);
            this.Controls.Add(this.startLogging);
            this.Controls.Add(this.deviceDescription);
            this.Controls.Add(this.selectButton);
            this.Name = "MainForm";
            this.Text = "PCM Logger";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startLogging;
        private System.Windows.Forms.Button stopLogging;
        private System.Windows.Forms.TextBox logValues;
    }
}

