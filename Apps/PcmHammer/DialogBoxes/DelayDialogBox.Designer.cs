namespace PcmHacking
{
    partial class DelayDialogBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DelayDialogBox));
            this.explanation = new System.Windows.Forms.Label();
            this.continueButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.countdown = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // explanation
            // 
            this.explanation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.explanation.Location = new System.Drawing.Point(12, 9);
            this.explanation.Name = "explanation";
            this.explanation.Size = new System.Drawing.Size(407, 117);
            this.explanation.TabIndex = 0;
            this.explanation.Text = resources.GetString("explanation.Text");
            // 
            // continueButton
            // 
            this.continueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.continueButton.Location = new System.Drawing.Point(163, 167);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(125, 25);
            this.continueButton.TabIndex = 1;
            this.continueButton.Text = "&Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(294, 167);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(125, 25);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Abort";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // countdown
            // 
            this.countdown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.countdown.Location = new System.Drawing.Point(12, 126);
            this.countdown.Name = "countdown";
            this.countdown.Size = new System.Drawing.Size(407, 31);
            this.countdown.TabIndex = 3;
            this.countdown.Text = "X seconds remaining...";
            this.countdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DelayDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 204);
            this.Controls.Add(this.countdown);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.explanation);
            this.Name = "DelayDialogBox";
            this.Text = "Sorry To Interrupt...";
            this.Load += new System.EventHandler(this.DelayDialogBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label countdown;
    }
}