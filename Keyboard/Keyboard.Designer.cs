namespace Keyboard
{
    partial class Keyboard
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Keyboard));
            this.label9 = new System.Windows.Forms.Label();
            this.lbCaption = new System.Windows.Forms.Label();
            this.lblCoords = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label9.Location = new System.Drawing.Point(20, 49);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Message:";
            // 
            // lbCaption
            // 
            this.lbCaption.Location = new System.Drawing.Point(18, 19);
            this.lbCaption.Name = "lbCaption";
            this.lbCaption.Size = new System.Drawing.Size(652, 30);
            this.lbCaption.TabIndex = 23;
            this.lbCaption.Text = "Press any key on an attached keyboard to see details of the input device and the " +
    "key(s) you pressed.";
            // 
            // lblCoords
            // 
            this.lblCoords.AutoSize = true;
            this.lblCoords.Location = new System.Drawing.Point(63, 74);
            this.lblCoords.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCoords.Name = "lblCoords";
            this.lblCoords.Size = new System.Drawing.Size(35, 13);
            this.lblCoords.TabIndex = 25;
            this.lblCoords.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "label1";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            // 
            // Keyboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 323);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCoords);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lbCaption);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Keyboard";
            this.Text = "Raw Keyboard Input";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Keyboard_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lbCaption;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCoords;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
    }
}

