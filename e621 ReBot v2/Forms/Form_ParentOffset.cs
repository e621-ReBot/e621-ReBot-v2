using e621_ReBot_v2.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_ParentOffset : Form
    {
        public Form_ParentOffset()
        {
            InitializeComponent();
        }

        private void Form_ParentOffset_Load(object sender, EventArgs e)
        {
            int RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(Form_Tagger._FormReference.Tagger_RowHolder);
            bool LaunchTimer = false;
            List<PictureBox> TempList = new List<PictureBox>();
            for (int x = 0; x <= RowIndex - 1; x++)
            {
                var TempPicBox = new PictureBox()
                {
                    Size = new Size(200, 200),
                    Margin = new Padding(2, 2, 2, 2),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Form_Loader.Cursor_Default,
                    Tag = Module_TableHolder.Database_Table.Rows[x],
                    BackgroundImage = Properties.Resources.E6Image_Loading
                };
                if (Module_TableHolder.Database_Table.Rows[x]["Thumbnail_Image"] == DBNull.Value)
                {
                    DataRow DataRowTemp = Module_TableHolder.Database_Table.Rows[x];
                    Module_Grabber.GrabDownloadThumb(DataRowTemp);
                    LaunchTimer = true;
                }
                else
                {
                    TempPicBox.Image = (Image)Module_TableHolder.Database_Table.Rows[x]["Thumbnail_Image"];
                    TempPicBox.BackgroundImage = null;
                }
                TempPicBox.MouseClick += PictureClick;
                TempList.Add(TempPicBox);
            }

            for (int y = RowIndex + 1; y <= Module_TableHolder.Database_Table.Rows.Count - 1; y++)
            {
                if (Module_TableHolder.Database_Table.Rows[y]["Uploaded_As"] != DBNull.Value)
                {
                    PictureBox TempPicBox = new PictureBox()
                    {
                        Size = new Size(200, 200),
                        Margin = new Padding(2, 2, 2, 2),
                        SizeMode = PictureBoxSizeMode.CenterImage,
                        BorderStyle = BorderStyle.FixedSingle,
                        Cursor = Form_Loader.Cursor_Default,
                        Tag = Module_TableHolder.Database_Table.Rows[y],
                        BackgroundImage = Properties.Resources.E6Image_Loading
                    };
                    if (Module_TableHolder.Database_Table.Rows[y]["Thumbnail_Image"] == DBNull.Value)
                    {
                        DataRow DataRowTemp = Module_TableHolder.Database_Table.Rows[y];
                        Module_Grabber.GrabDownloadThumb(DataRowTemp);
                        LaunchTimer = true;
                    }
                    else
                    {
                        TempPicBox.Image = (Image)Module_TableHolder.Database_Table.Rows[y]["Thumbnail_Image"];
                        TempPicBox.BackgroundImage = null;
                    }

                    TempPicBox.MouseClick += PictureClick;
                    TempList.Add(TempPicBox);
                }
            }

            if (TempList.Count > 160)
            {
                Width += 2 * 204 + 16;
                FlowLayoutPanel_Holder.WrapContents = true;
            }
            else
            {
                if (TempList.Count < 4)
                {
                    if (TempList.Count > 1)
                        Width += (TempList.Count - 1) * 204;
                }
                else
                {
                    Width += 2 * 204;
                    Height += 20;
                }
            }

            FlowLayoutPanel_Holder.SuspendLayout();
            FlowLayoutPanel_Holder.Controls.AddRange(TempList.ToArray());
            FlowLayoutPanel_Holder.ResumeLayout();

            // FlowLayoutPanel_Holder.ScrollControlIntoView(FlowLayoutPanel_Holder.Controls.Item(RowIndex + 1))
            if (TempList.Count > 160)
            {
                FlowLayoutPanel_Holder.AutoScrollPosition = new Point(0, (int)((RowIndex - 2) / 3d) * 204);
            }
            else
            {
                FlowLayoutPanel_Holder.AutoScrollPosition = new Point((RowIndex - 1) * 204 - 102, 0);
            }

            if (LaunchTimer)
            {
                PicLoad_Timer.Start();
            }

        }

        private void PictureClick(object sender, MouseEventArgs e)
        {
            PictureBox SenderPictureBox = (PictureBox)sender;
            if (e.Button == MouseButtons.Left)
            {
                Form_Tagger._FormReference.Tagger_RowHolder["Upload_ParentOffset"] = (DataRow)SenderPictureBox.Tag;
                Form_Tagger._FormReference.TB_ParentOffset.Tag = "Set";
                Close();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    {
                        Close();
                        break;
                    }
            }
            return false;
        }

        private void PicLoad_Timer_Tick(object sender, EventArgs e)
        {
            PicLoad_Timer.Stop();
            bool LaunchTimer = false;

            foreach (PictureBox PicImage in FlowLayoutPanel_Holder.Controls)
            {
                if (PicImage.BackgroundImage != null)
                {
                    if (((DataRow)PicImage.Tag)["Thumbnail_Image"] == DBNull.Value)
                    {
                        LaunchTimer = true;
                    }
                    else
                    {
                        Module_Grabber.WriteImageInfo((DataRow)PicImage.Tag);
                        PicImage.Image = (Image)((DataRow)PicImage.Tag)["Thumbnail_Image"];
                        PicImage.BackgroundImage = null;
                    }
                }
            }
            if (LaunchTimer)
            {
                PicLoad_Timer.Start();
            }
        }

    }
}