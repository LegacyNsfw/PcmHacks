
namespace PcmHacking
{
    partial class HWSelector
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
            this.WarningLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.HWType = new System.Windows.Forms.ComboBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // WarningLabel
            // 
            this.WarningLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WarningLabel.Location = new System.Drawing.Point(12, 11);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(322, 77);
            this.WarningLabel.TabIndex = 0;
            this.WarningLabel.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "HW Type:";
            // 
            // HWType
            // 
            this.HWType.FormattingEnabled = true;
            this.HWType.Location = new System.Drawing.Point(12, 110);
            this.HWType.Name = "HWType";
            this.HWType.Size = new System.Drawing.Size(322, 21);
            this.HWType.TabIndex = 2;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(257, 137);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(68, 26);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(189, 137);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(62, 26);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // HWSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 175);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.HWType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.WarningLabel);
            this.Name = "HWSelector";
            this.Text = "HWSelector";
            this.Load += new System.EventHandler(this.HWSelector_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.ComboBox HWType;
    }
}