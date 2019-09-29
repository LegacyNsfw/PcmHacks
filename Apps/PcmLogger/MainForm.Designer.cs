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
            this.profilePath = new System.Windows.Forms.Label();
            this.selectProfileButton = new System.Windows.Forms.Button();
            this.logValues = new System.Windows.Forms.TextBox();
            this.startStopLogging = new System.Windows.Forms.Button();
            this.deviceDescription = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // profilePath
            // 
            this.profilePath.AutoSize = true;
            this.profilePath.Location = new System.Drawing.Point(233, 48);
            this.profilePath.Name = "profilePath";
            this.profilePath.Size = new System.Drawing.Size(84, 13);
            this.profilePath.TabIndex = 10;
            this.profilePath.Text = "[selected profile]";
            // 
            // selectProfileButton
            // 
            this.selectProfileButton.Location = new System.Drawing.Point(12, 43);
            this.selectProfileButton.Name = "selectProfileButton";
            this.selectProfileButton.Size = new System.Drawing.Size(215, 23);
            this.selectProfileButton.TabIndex = 9;
            this.selectProfileButton.Text = "Select Log &Profile";
            this.selectProfileButton.UseVisualStyleBackColor = true;
            this.selectProfileButton.Click += new System.EventHandler(this.selectProfile_Click);
            // 
            // logValues
            // 
            this.logValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logValues.Location = new System.Drawing.Point(12, 101);
            this.logValues.Multiline = true;
            this.logValues.Name = "logValues";
            this.logValues.ReadOnly = true;
            this.logValues.Size = new System.Drawing.Size(543, 218);
            this.logValues.TabIndex = 7;
            // 
            // startStopLogging
            // 
            this.startStopLogging.Location = new System.Drawing.Point(12, 72);
            this.startStopLogging.Name = "startStopLogging";
            this.startStopLogging.Size = new System.Drawing.Size(215, 23);
            this.startStopLogging.TabIndex = 4;
            this.startStopLogging.Text = "Start &Logging";
            this.startStopLogging.UseVisualStyleBackColor = true;
            this.startStopLogging.Click += new System.EventHandler(this.startLogging_Click);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 331);
            this.Controls.Add(this.profilePath);
            this.Controls.Add(this.selectProfileButton);
            this.Controls.Add(this.logValues);
            this.Controls.Add(this.startStopLogging);
            this.Controls.Add(this.deviceDescription);
            this.Controls.Add(this.selectButton);
            this.Name = "MainForm";
            this.Text = "PCM Logger";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startStopLogging;
        private System.Windows.Forms.TextBox logValues;
        private System.Windows.Forms.Button selectProfileButton;
        private System.Windows.Forms.Label profilePath;
    }
}

