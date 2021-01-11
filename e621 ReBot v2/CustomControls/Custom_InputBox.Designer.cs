namespace e621_ReBot_v2.CustomControls
{
    partial class Custom_InputBox
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
            this.textBox_Input = new System.Windows.Forms.TextBox();
            this.button_OK = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.label_Description = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_Input
            // 
            this.textBox_Input.BackColor = System.Drawing.Color.Silver;
            this.textBox_Input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Input.Location = new System.Drawing.Point(1, 32);
            this.textBox_Input.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_Input.Name = "textBox_Input";
            this.textBox_Input.Size = new System.Drawing.Size(256, 20);
            this.textBox_Input.TabIndex = 0;
            this.textBox_Input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_Input_KeyDown);
            // 
            // button_OK
            // 
            this.button_OK.BackColor = System.Drawing.SystemColors.Control;
            this.button_OK.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.CheckMark_Icon;
            this.button_OK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_OK.Location = new System.Drawing.Point(256, 32);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(32, 20);
            this.button_OK.TabIndex = 1;
            this.button_OK.TabStop = false;
            this.button_OK.UseVisualStyleBackColor = false;
            this.button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // label_Description
            // 
            this.label_Description.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Description.Location = new System.Drawing.Point(1, 1);
            this.label_Description.Margin = new System.Windows.Forms.Padding(0);
            this.label_Description.Name = "label_Description";
            this.label_Description.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Description.Size = new System.Drawing.Size(287, 32);
            this.label_Description.TabIndex = 2;
            this.label_Description.Text = "label_Description";
            this.label_Description.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Custom_InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(289, 53);
            this.Controls.Add(this.label_Description);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_Input);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Custom_InputBox";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "InputBox";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Custom_InputBox_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Input;
        private Button_Unfocusable button_OK;
        private System.Windows.Forms.Label label_Description;
    }
}