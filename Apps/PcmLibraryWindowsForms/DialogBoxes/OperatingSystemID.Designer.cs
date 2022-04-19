namespace PcmHacking
{
    partial class OperatingSystemIDDialogBox
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
            this.operatingSystemIdLabel = new System.Windows.Forms.Label();
            this.operatingSystemIdTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // operatingSystemIdLabel
            // 
            this.operatingSystemIdLabel.AutoSize = true;
            this.operatingSystemIdLabel.Location = new System.Drawing.Point(15, 37);
            this.operatingSystemIdLabel.Name = "operatingSystemIdLabel";
            this.operatingSystemIdLabel.Size = new System.Drawing.Size(104, 13);
            this.operatingSystemIdLabel.TabIndex = 0;
            this.operatingSystemIdLabel.Text = "Operating System ID";
            // 
            // operatingSystemIdTextBox
            // 
            this.operatingSystemIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.operatingSystemIdTextBox.Location = new System.Drawing.Point(125, 34);
            this.operatingSystemIdTextBox.Name = "operatingSystemIdTextBox";
            this.operatingSystemIdTextBox.Size = new System.Drawing.Size(68, 20);
            this.operatingSystemIdTextBox.TabIndex = 1;
            this.operatingSystemIdTextBox.TextChanged += new System.EventHandler(this.operatingSystemIdTextBox_TextChanged);
            this.operatingSystemIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.operatingSystemIdTextBox_KeyPress);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(118, 75);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(37, 75);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(-1, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(230, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Must match the Operating System on the PCM!";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(95, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Example: 12587603";
            // 
            // OperatingSystemIDDialogBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(226, 113);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.operatingSystemIdTextBox);
            this.Controls.Add(this.operatingSystemIdLabel);
            this.Name = "OperatingSystemIDDialogBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Operating System ID";
            this.Load += new System.EventHandler(this.operatingSystemIdDialogBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label operatingSystemIdLabel;
        private System.Windows.Forms.TextBox operatingSystemIdTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}