using System;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public partial class Custom_InputBox : Form
    {
        public Custom_InputBox(in Form FormPass)
        {
            InitializeComponent();
            Owner = FormPass;
        }

        private void TextBox_Input_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        if (Text.Equals("e621 ReBot") || Text.Contains("Category"))
                        {
                            e.SuppressKeyPress = false;
                        }
                        else
                        {
                            e.SuppressKeyPress = true;
                        }
                        break;
                    }

                case Keys.Enter:
                    {
                        _txtInput = textBox_Input.Text;
                        NotCanceled = true;
                        Close();
                        break;
                    }

                case Keys.Escape:
                    {
                        Close();
                        break;
                    }
            }

        }

        private string _txtInput;
        private void Button_OK_Click(object sender, EventArgs e)
        {
            _txtInput = textBox_Input.Text;
            NotCanceled = true;
            Close();
        }

        public static string Show(Form FormPass, string Title, string Description, Point StartingLocation, string StartingSting = "")
        {
            Custom_InputBox cInputBox = new Custom_InputBox(FormPass)
            {
                Location = new Point(StartingLocation.X - 7, StartingLocation.Y - 2),
                Text = Title
            };
            cInputBox.label_Description.Text = Description;
            cInputBox.textBox_Input.Text = StartingSting;
            cInputBox.ShowDialog();

            return cInputBox._txtInput;
        }

        private bool NotCanceled;
        private void Custom_InputBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (NotCanceled)
            {
                _txtInput = _txtInput ?? "✄";
            }
            else
            {
                _txtInput = "✄";
            }
            Owner.Activate();
            Owner.Focus();
            Dispose();
        }
    }
}