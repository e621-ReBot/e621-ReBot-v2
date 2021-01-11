
namespace e621_ReBot_v2.Forms
{
    partial class Form_SimilarSearch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_SimilarSearch));
            this.FlowLayoutPanel_Holder = new System.Windows.Forms.FlowLayoutPanel();
            this.Label_SearchCheck = new System.Windows.Forms.Label();
            this.timer_Delay = new System.Windows.Forms.Timer(this.components);
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.FlowLayoutPanel_Holder.SuspendLayout();
            this.SuspendLayout();
            // 
            // FlowLayoutPanel_Holder
            // 
            this.FlowLayoutPanel_Holder.Controls.Add(this.Label_SearchCheck);
            this.FlowLayoutPanel_Holder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel_Holder.Location = new System.Drawing.Point(0, 0);
            this.FlowLayoutPanel_Holder.Name = "FlowLayoutPanel_Holder";
            this.FlowLayoutPanel_Holder.Size = new System.Drawing.Size(284, 41);
            this.FlowLayoutPanel_Holder.TabIndex = 3;
            // 
            // Label_SearchCheck
            // 
            this.Label_SearchCheck.Font = new System.Drawing.Font("Arial Black", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Label_SearchCheck.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.Label_SearchCheck.Location = new System.Drawing.Point(0, 8);
            this.Label_SearchCheck.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.Label_SearchCheck.Name = "Label_SearchCheck";
            this.Label_SearchCheck.Size = new System.Drawing.Size(284, 24);
            this.Label_SearchCheck.TabIndex = 1;
            this.Label_SearchCheck.Text = "Checking Site status...";
            this.Label_SearchCheck.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer_Delay
            // 
            this.timer_Delay.Interval = 1000;
            this.timer_Delay.Tick += new System.EventHandler(this.Timer_Delay_Tick);
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 10000;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 20;
            // 
            // Form_SimilarSearch
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(284, 41);
            this.Controls.Add(this.FlowLayoutPanel_Holder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_SimilarSearch";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Site";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Similar_FormClosed);
            this.Load += new System.EventHandler(this.Form_Similar_Load);
            this.Shown += new System.EventHandler(this.Form_Similar_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_Similar_KeyDown);
            this.FlowLayoutPanel_Holder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel_Holder;
        internal System.Windows.Forms.Label Label_SearchCheck;
        internal System.Windows.Forms.Timer timer_Delay;
        private System.Windows.Forms.ToolTip toolTip_Display;
    }
}