
namespace e621_ReBot_v2.Forms
{
    partial class Form_e6Pool
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
            this.ID_TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ID_TextBox
            // 
            this.ID_TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ID_TextBox.Location = new System.Drawing.Point(8, 8);
            this.ID_TextBox.MaxLength = 7;
            this.ID_TextBox.Name = "ID_TextBox";
            this.ID_TextBox.Size = new System.Drawing.Size(88, 26);
            this.ID_TextBox.TabIndex = 2;
            this.ID_TextBox.Text = "Enter ID";
            this.ID_TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ID_TextBox.Enter += new System.EventHandler(this.ID_TextBox_Enter);
            this.ID_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ID_TextBox_KeyDown);
            this.ID_TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ID_TextBox_KeyPress);
            this.ID_TextBox.Leave += new System.EventHandler(this.ID_TextBox_Leave);
            // 
            // Form_e6Pool
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(105, 42);
            this.Controls.Add(this.ID_TextBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_e6Pool";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Enter Pool ID";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_e6Pool_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox ID_TextBox;
    }
}