using e621_ReBot_v2.CustomControls;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_Menu : Form
    {
        public static Form_Menu _FormReference;
        public Form_Menu()
        {
            InitializeComponent();
            _FormReference = this;
        }

        private void Form_Menu_Load(object sender, EventArgs e)
        {
            Enabled = false;
            timer_FadeIn.Start();
            MB_Grid.Visible = Form_Loader._GridFLPHolder.Controls.Count > 0;

            Button_Menu HighlightButton = Menu_FlowLayoutPanel.Controls.OfType<Button_Menu>().ToArray()[Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex];
            HighlightButton.Active = true;
            HighlightButton.MB_Highlight();
        }

        private int ActionTimer;
        private void Timer_FadeIn_Tick(object sender, EventArgs e)
        {
            switch (ActionTimer)
            {
                case int n when n <= 8:
                    {
                        Location = new Point(Location.X + 8, Location.Y);
                        Opacity += 0.02;
                        break;
                    }

                case int n when n <= 18:
                    {
                        Location = new Point(Location.X + 4, Location.Y);
                        Opacity += 0.03;
                        break;
                    }

                case int n when n <= 30:
                    {
                        Opacity += 0.45;
                        break;
                    }

                default:
                    {
                        timer_FadeIn.Stop();
                        if (Form_Notes._FormReference == null)
                        {
                            Enabled = true;
                        }
                        break;
                    }
            }
            ActionTimer += 1;
        }

        private void Timer_FadeOut_Tick(object sender, EventArgs e)
        {
            Location = new Point(Location.X - 1, Location.Y);
            Opacity -= 0.05;
            ActionTimer += 1;
            if (ActionTimer > 20)
            {
                Close();
            }
        }

        private void MB_MenuClose_Click(object sender, EventArgs e)
        {
            Enabled = false;
            ActionTimer = 0;
            timer_FadeOut.Start();
            if (Form_APIKey._FormReference != null)
            {
                Form_APIKey._FormReference.Close();
            }

            if (Form_Tagger._FormReference != null && Form_Tagger._FormReference.Owner == Form_Loader._FormReference)
            {
                Form_Tagger._FormReference.Close();
            }
        }

        private void Form_Menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Loader._FormReference.Menu_Btn.Visible = true;
            Dispose();
            _FormReference = null;
        }
    }
}
