namespace PcmHacking.DialogBoxes
{
    partial class VinForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.vinBox = new System.Windows.Forms.TextBox();
            this.prompt = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.prompt2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter &VIN:";
            // 
            // vinBox
            // 
            this.vinBox.Location = new System.Drawing.Point(114, 13);
            this.vinBox.Name = "vinBox";
            this.vinBox.Size = new System.Drawing.Size(343, 22);
            this.vinBox.TabIndex = 1;
            this.vinBox.TextChanged += new System.EventHandler(this.vinBox_TextChanged);
            // 
            // prompt
            // 
            this.prompt.AutoSize = true;
            this.prompt.Location = new System.Drawing.Point(111, 44);
            this.prompt.Name = "prompt";
            this.prompt.Size = new System.Drawing.Size(311, 17);
            this.prompt.TabIndex = 2;
            this.prompt.Text = "This is where we explain the length requirement.";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(354, 92);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 30);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(248, 92);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 30);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // prompt2
            // 
            this.prompt2.AutoSize = true;
            this.prompt2.Location = new System.Drawing.Point(111, 65);
            this.prompt2.Name = "prompt2";
            this.prompt2.Size = new System.Drawing.Size(315, 17);
            this.prompt2.TabIndex = 5;
            this.prompt2.Text = "This is where we prompt for letters and numbers.";
            // 
            // VinForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 134);
            this.Controls.Add(this.prompt2);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.prompt);
            this.Controls.Add(this.vinBox);
            this.Controls.Add(this.label1);
            this.Name = "VinForm";
            this.Text = "Modify VIN";
            this.Load += new System.EventHandler(this.VinForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox vinBox;
        private System.Windows.Forms.Label prompt;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label prompt2;
    }
}