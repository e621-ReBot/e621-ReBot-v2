
namespace e621_ReBot_v2.Forms
{
    partial class Form_ParentOffset
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
            this.FlowLayoutPanel_Holder = new System.Windows.Forms.FlowLayoutPanel();
            this.PicLoad_Timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // FlowLayoutPanel_Holder
            // 
            this.FlowLayoutPanel_Holder.AutoScroll = true;
            this.FlowLayoutPanel_Holder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel_Holder.Location = new System.Drawing.Point(0, 0);
            this.FlowLayoutPanel_Holder.MaximumSize = new System.Drawing.Size(0, 221);
            this.FlowLayoutPanel_Holder.Name = "FlowLayoutPanel_Holder";
            this.FlowLayoutPanel_Holder.Size = new System.Drawing.Size(207, 205);
            this.FlowLayoutPanel_Holder.TabIndex = 1;
            this.FlowLayoutPanel_Holder.WrapContents = false;
            // 
            // PicLoad_Timer
            // 
            this.PicLoad_Timer.Interval = 1000;
            this.PicLoad_Timer.Tick += new System.EventHandler(this.PicLoad_Timer_Tick);
            // 
            // Form_ParentOffset
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(207, 205);
            this.Controls.Add(this.FlowLayoutPanel_Holder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_ParentOffset";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Parent Offset";
            this.Load += new System.EventHandler(this.Form_ParentOffset_Load);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel_Holder;
        internal System.Windows.Forms.Timer PicLoad_Timer;
    }
}