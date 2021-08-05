using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class PuzzleGamePanel : Panel
    {
        private Bitmap PreloadImage;
        public Bitmap CompleteImage;
        public List<PuzzlePiece> PuzzlePieces;
        public int _RowCount = 4;
        public int _CollumnCount = 5;
        public int _BlankPiece;
        public int _PieceWidth;
        public int _PieceHeight;

        public PuzzleGamePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // GamePanel
            // 
            Font = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel);
            MouseClick += GamePanel_MouseClick;
            ResumeLayout(false);

        }

        private void Custom_Panel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        public void LoadPuzzle()
        {
            if (PreloadImage == null)
            {
                Paint -= Custom_Panel_Paint;
                ImageLoader();
            }
            else
            {
                _RowCount = int.Parse(Form_Loader._FormReference.comboBox_PuzzleRows.Text);
                _CollumnCount = int.Parse(Form_Loader._FormReference.comboBox_PuzzleCollumns.Text);

                if (CompleteImage != null)
                {
                    CompleteImage.Dispose();
                }

                Bitmap TempBitmap = new Bitmap(PreloadImage);

                PreloadImage.Dispose();
                PreloadImage = null;

                double ScaleFactor = 0.99;
                if (TempBitmap.Width > MaximumSize.Width)
                {
                    ScaleFactor *= (double)MaximumSize.Width / TempBitmap.Width;
                }
                if ((TempBitmap.Height * ScaleFactor) > MaximumSize.Height)
                {
                    ScaleFactor *= (double)MaximumSize.Height / (TempBitmap.Height * ScaleFactor);
                }
                int scaledWidth = (int)(TempBitmap.Width * ScaleFactor);
                int scaledHeight = (int)(TempBitmap.Height * ScaleFactor);

                CompleteImage = new Bitmap(scaledWidth, scaledHeight, PixelFormat.Format32bppArgb);
                using (Graphics TempGraphics = Graphics.FromImage(CompleteImage))
                {
                    TempGraphics.InterpolationMode = InterpolationMode.High;
                    TempGraphics.CompositingQuality = CompositingQuality.HighQuality;
                    TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    TempGraphics.DrawImage(TempBitmap, 0, 0, scaledWidth, scaledHeight);
                }
                Size = CompleteImage.Size;

                Form_Loader._FormReference.pB_GameThumb.BackgroundImage = CompleteImage;
                CreatePuzzlePieces();
                ShufflePuzzle();
                DrawPuzzlePieces();
                LastHintState = Form_Loader._FormReference.CC_GameIndexHints.Checked;

                Location = new Point(20 + (MaximumSize.Width - Width) / 2, 8 + (MaximumSize.Height - Height) / 2);
                Form_Loader._FormReference.GB_RestartGame.Enabled = true;
            }
        }

        private void ImageLoader()
        {
            if (Form_Loader._FormReference.rb_GameStart_1.Checked)
            {
                Form_Loader._FormReference.labelPuzzle_SelectedPost.Visible = false;
                using (OpenFileDialog ImageDialog = new OpenFileDialog())
                {
                    ImageDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                    ImageDialog.Title = "Select an image to load for the puzzle.";

                    if (ImageDialog.ShowDialog() == DialogResult.OK)
                    {
                        PreloadImage = new Bitmap(ImageDialog.FileName);
                        LoadPuzzle();
                    }
                }
            }
            if (Form_Loader._FormReference.rb_GameStart_2.Checked)
            {
            Repeat_Random:
                JToken RandomJSON = JObject.Parse(Module_e621Info.e621InfoDownload("https://e621.net/posts/random.json", false))["post"];

                int ImageWidth = RandomJSON["file"]["width"].Value<int>();
                int ImageHeight = RandomJSON["file"]["height"].Value<int>();
                double ImageRatio = (double)Math.Max(ImageWidth, ImageHeight) / Math.Min(ImageWidth, ImageHeight);
                if (ImageRatio > 3)
                {
                    goto Repeat_Random;
                }

                string FileURL = string.Format("https://static1.e621.net/data/{0}/{1}/{2}.{3}", RandomJSON["file"]["md5"].Value<string>().Substring(0, 2), RandomJSON["file"]["md5"].Value<string>().Substring(2, 2), RandomJSON["file"]["md5"].Value<string>(), RandomJSON["file"]["ext"].Value<string>());
                if (RandomJSON["file"]["ext"].Value<string>().Equals("jpg") || RandomJSON["file"]["ext"].Value<string>().Equals("png"))
                {
                    Form_Loader._FormReference.labelPuzzle_SelectedPost.Text = "Random post #" + RandomJSON["id"].Value<string>();
                    Form_Loader._FormReference.labelPuzzle_SelectedPost.Tag = RandomJSON["id"].Value<string>();
                    using (WebClient WebClientTemp = new WebClient())
                    {
                        WebClientTemp.DownloadDataCompleted += ImageDownloadComplete;
                        WebClientTemp.DownloadDataAsync(new Uri(FileURL));
                    }
                }
                else
                {
                    goto Repeat_Random;
                }
            }
            if (Form_Loader._FormReference.rb_GameStart_3.Checked)
            {
                string PuzzlePostID;
                using (Form_e6Post Form_e6PostTemp = new Form_e6Post(Form_Loader._FormReference.GB_StartGame.PointToScreen(Point.Empty), Form_Loader._FormReference))
                {
                    Form_e6PostTemp.Tag = "Puzzle";
                    Form_e6PostTemp.ShowDialog();

                    PuzzlePostID = Form_e6PostTemp.PuzzlePostID;
                }

                string PostTest = Module_e621Info.e621InfoDownload(string.Format("https://e621.net/posts/{0}.json", PuzzlePostID), false);
                if (PostTest == null || PostTest.Length < 10)
                {
                    MessageBox.Show(string.Format("Post with ID#{0} does not exist.", PuzzlePostID), "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                JToken RandomJSON = JObject.Parse(PostTest)["post"];

                if (RandomJSON["flags"]["deleted"].Value<bool>())
                {
                    MessageBox.Show(string.Format("Post with ID#{0} does not exist anymore.", PuzzlePostID), "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string FileURL = string.Format("https://static1.e621.net/data/{0}/{1}/{2}.{3}", RandomJSON["file"]["md5"].Value<string>().Substring(0, 2), RandomJSON["file"]["md5"].Value<string>().Substring(2, 2), RandomJSON["file"]["md5"].Value<string>(), RandomJSON["file"]["ext"].Value<string>());
                if (RandomJSON["file"]["ext"].Value<string>().Equals("jpg") || RandomJSON["file"]["ext"].Value<string>().Equals("png"))
                {
                    Form_Loader._FormReference.labelPuzzle_SelectedPost.Text = "Selected post #" + PuzzlePostID;
                    Form_Loader._FormReference.labelPuzzle_SelectedPost.Tag = PuzzlePostID;
                    using (WebClient WebClientTemp = new WebClient())
                    {
                        WebClientTemp.DownloadDataCompleted += ImageDownloadComplete;
                        WebClientTemp.DownloadDataAsync(new Uri(FileURL));
                    }
                }
                else
                {
                    MessageBox.Show("That post does not contain an image of compatible format.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ImageDownloadComplete(object sender, DownloadDataCompletedEventArgs e)
        {
            Form_Loader._FormReference.labelPuzzle_SelectedPost.Visible = true;
            using (MemoryStream MemoryStreamTemp = new MemoryStream(e.Result))
            {
                PreloadImage = (Bitmap)Image.FromStream(MemoryStreamTemp);
                LoadPuzzle();
            }
        }

        private void CreatePuzzlePieces()
        {
            if (PuzzlePieces != null)
            {
                foreach (PuzzlePiece PuzzlePieceTemp in PuzzlePieces)
                {
                    PuzzlePieceTemp.PieceImage.Dispose();
                }
                PuzzlePieces.Clear();
            }

            PuzzlePieces = new List<PuzzlePiece>();
            _PieceWidth = CompleteImage.Width / _CollumnCount;
            _PieceHeight = CompleteImage.Height / _RowCount;
            for (int RowCount = 0; RowCount < _RowCount; RowCount++)
            {
                for (int CollumnCount = 0; CollumnCount < _CollumnCount; CollumnCount++)
                {
                    PuzzlePiece TempPuzzlePiece = new PuzzlePiece
                    {
                        PieceIndex = RowCount * _CollumnCount + CollumnCount,
                        PieceImage = ImageCut(CollumnCount * _PieceWidth, RowCount * _PieceHeight, RowCount * _CollumnCount + CollumnCount)
                    };
                    PuzzlePieces.Add(TempPuzzlePiece);
                }
            }
        }

        private Bitmap ImageCut(int xPoint, int yPoint, int PieceIndex)
        {
            Bitmap TempBitmap = new Bitmap(_PieceWidth + 1, _PieceHeight + 1, PixelFormat.Format32bppArgb);
            using (Graphics TempGraphics = Graphics.FromImage(TempBitmap))
            {
                TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                TempGraphics.InterpolationMode = InterpolationMode.High;
                TempGraphics.CompositingQuality = CompositingQuality.HighQuality;

                TempGraphics.DrawImage(CompleteImage, new Rectangle(new Point(0, 0), new Size(_PieceWidth, _PieceHeight)), new Rectangle(new Point(xPoint, yPoint), new Size(_PieceWidth, _PieceHeight)), GraphicsUnit.Pixel);
                using (Pen TempPen = new Pen(Color.Black, 1))
                {
                    TempGraphics.DrawRectangle(TempPen, 0, 0, _PieceWidth + 1, _PieceHeight + 1);
                }

                TempGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                if (Form_Loader._FormReference.CC_GameIndexHints.Checked)
                {
                    using (GraphicsPath gPath = new GraphicsPath())
                    {
                        using (StringFormat StringFormatTemp = new StringFormat())
                        {
                            int FontSize = 20;
                            if (_PieceWidth < 40 && _PieceHeight > _PieceWidth)
                            {
                                StringFormatTemp.Alignment = StringAlignment.Near;
                                StringFormatTemp.LineAlignment = StringAlignment.Near;
                                TempGraphics.RotateTransform(-90, MatrixOrder.Append);
                                TempGraphics.TranslateTransform(0, _PieceHeight, MatrixOrder.Append);
                                gPath.AddString(PieceIndex.ToString(), Font.FontFamily, (int)Font.Style, FontSize, new Rectangle(new Point(0, 0), new Size(48, 32)), StringFormatTemp);
                            }
                            else
                            {
                                StringFormatTemp.Alignment = StringAlignment.Near;
                                StringFormatTemp.LineAlignment = StringAlignment.Far;
                                gPath.AddString(PieceIndex.ToString(), Font.FontFamily, (int)Font.Style, FontSize, new Rectangle(new Point(0, _PieceHeight - 32), new Size(48, 32)), StringFormatTemp);
                            }
                        }
                        TempGraphics.DrawPath(new Pen(Color.Black, 4), gPath);
                        TempGraphics.FillPath(new SolidBrush(Color.DarkOrange), gPath);
                    }
                }
            }
            return TempBitmap;
        }

        private void ShufflePuzzle()
        {
            int[,] PuzzleGrid = new int[_RowCount, _CollumnCount];
            int TotalCounter = 0;
            for (int RowCount = 0; RowCount < _RowCount; RowCount++)
            {
                for (int CollumnCount = 0; CollumnCount < _CollumnCount; CollumnCount++)
                {
                    PuzzleGrid[RowCount, CollumnCount] = TotalCounter;
                    TotalCounter += 1;
                }
            }
            PuzzleGrid[_RowCount - 1, _CollumnCount - 1] = -1;

            int BlankPieceRow = _RowCount - 1;
            int BlankPieceCollumn = _CollumnCount - 1;

            List<int[]> MovablePieces = new List<int[]>();
            Random RandomGenerator = new Random();
            for (int ShuffleCount = 0; ShuffleCount < PuzzlePieces.Count * 256; ShuffleCount++)
            {
                MovablePieces.Clear();
                if (BlankPieceRow > 0)
                {
                    MovablePieces.Add(new int[] { BlankPieceRow - 1, BlankPieceCollumn });
                }
                if (BlankPieceRow < (_RowCount - 1))
                {
                    MovablePieces.Add(new int[] { BlankPieceRow + 1, BlankPieceCollumn });
                }
                if (BlankPieceCollumn > 0)
                {
                    MovablePieces.Add(new int[] { BlankPieceRow, BlankPieceCollumn - 1 });
                }
                if (BlankPieceCollumn < (_CollumnCount - 1))
                {
                    MovablePieces.Add(new int[] { BlankPieceRow, BlankPieceCollumn + 1 });
                } 

                int RndSelected = RandomGenerator.Next(0, MovablePieces.Count); //max is actually max-1

                PuzzleGrid[BlankPieceRow, BlankPieceCollumn] = PuzzleGrid[MovablePieces[RndSelected][0], MovablePieces[RndSelected][1]];
                BlankPieceRow = MovablePieces[RndSelected][0];
                BlankPieceCollumn = MovablePieces[RndSelected][1];
                PuzzleGrid[BlankPieceRow, BlankPieceCollumn] = -1;
            }
            MovablePieces.Clear();

            //move blank back to bottom right
            for (int currRow = BlankPieceRow; currRow < _RowCount; currRow++)
            {
                for (int currCollumn = BlankPieceCollumn; currCollumn < _CollumnCount; currCollumn++)
                {
                    PuzzleGrid[BlankPieceRow, BlankPieceCollumn] = PuzzleGrid[currRow, currCollumn];
                    PuzzleGrid[currRow, currCollumn] = -1;
                    BlankPieceRow = currRow;
                    BlankPieceCollumn = currCollumn;
                }
            }

            //set new positions
            TotalCounter = 0;
            for (int RowCount = 0; RowCount < _RowCount; RowCount++)
            {
                for (int CollumnCount = 0; CollumnCount < _CollumnCount; CollumnCount++)
                {
                    PuzzlePieces[TotalCounter].BoardIndex = PuzzleGrid[RowCount, CollumnCount];
                    TotalCounter += 1;
                }
            }
        }

        private void DrawPuzzlePieces()
        {
            if (BackgroundImage != null)
            {
                BackgroundImage.Dispose();
            } 

            Bitmap TempBitmap = new Bitmap(CompleteImage);

            List<Point> PointPool = new List<Point>();
            int CancelCounter = 0;
            for (int RowCount = 0; RowCount < _RowCount; RowCount++)
            {
                for (int CollumnCount = 0; CollumnCount < _CollumnCount; CollumnCount++)
                {
                    if (CancelCounter == _RowCount * _CollumnCount - 1)
                    {
                        _BlankPiece = CancelCounter;
                        continue; //skip last piece
                    }
                    PointPool.Add(new Point(CollumnCount * _PieceWidth, RowCount * _PieceHeight));
                    CancelCounter += 1;
                }
            }

            using (Graphics TempGraphics = Graphics.FromImage(TempBitmap))
            {
                TempGraphics.InterpolationMode = InterpolationMode.High;
                TempGraphics.CompositingQuality = CompositingQuality.HighQuality;
                TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                TempGraphics.Clear(Color.FromArgb(0, 45, 90));

                for (int i = 0; i < PuzzlePieces.Count - 1; i++)
                {
                    TempGraphics.DrawImage(PuzzlePieces[i].PieceImage, new Point(PointPool[PuzzlePieces[i].BoardIndex].X, PointPool[PuzzlePieces[i].BoardIndex].Y));
                }
                BackgroundImage = TempBitmap;
            }
        }

        bool LastHintState;
        public void ResetPuzzle()
        {
            if (Form_Loader._FormReference.CC_GameIndexHints.Checked == LastHintState)
            {
                for (int i = 0; i < PuzzlePieces.Count; i++)
                {
                    PuzzlePieces[i].BoardIndex = i;
                }
            }
            else
            {
                CreatePuzzlePieces();
            }
            ShufflePuzzle();
            DrawPuzzlePieces();
            LastHintState = Form_Loader._FormReference.CC_GameIndexHints.Checked;
        }

        private void GamePanel_MouseClick(object sender, MouseEventArgs e)
        {
            int ClickRow = e.Y / _PieceHeight;
            int ClickCollumn = e.X / _PieceWidth;
            int ClickIndex = ClickRow * _CollumnCount + ClickCollumn;

            if (ClickIndex == _BlankPiece)
            {
                return;
            } 

            int BlankPieceRow = _BlankPiece / _CollumnCount;
            int BlankPieceCollumn = _BlankPiece - (BlankPieceRow * _CollumnCount);

            List<int> MovablePieces = new List<int>();
            if (BlankPieceRow > 0)
            {
                MovablePieces.Add(_BlankPiece - _CollumnCount);
            }
            if (BlankPieceRow < (_RowCount - 1))
            {
                MovablePieces.Add(_BlankPiece + _CollumnCount);
            }
            if (BlankPieceCollumn > 0)
            {
                MovablePieces.Add(_BlankPiece - 1);
            }
            if (BlankPieceCollumn < (_CollumnCount - 1))
            {
                MovablePieces.Add(_BlankPiece + 1);
            }

            if (MovablePieces.Count == 0)
            {
                return;
            }

            if (MovablePieces.Contains(ClickIndex))
            {
                for (int PieceIndex = 0; PieceIndex < PuzzlePieces.Count; PieceIndex++)
                {
                    if (PuzzlePieces[PieceIndex].BoardIndex == ClickIndex)
                    {
                        MovePuzzlePiece(PieceIndex, ClickRow, ClickCollumn, BlankPieceRow, BlankPieceCollumn);
                        PuzzlePieces[PieceIndex].BoardIndex = _BlankPiece;
                        _BlankPiece = ClickIndex;
                        break;
                    }
                }
            }
            MovablePieces.Clear();
        }

        private void MovePuzzlePiece(int PieceIndex, int ClickRow, int ClickCollumn, int BlankPieceRow, int BlankPieceCollumn)
        {
            Bitmap TempBitmap = new Bitmap(BackgroundImage);
            using (Graphics TempGraphics = Graphics.FromImage(TempBitmap))
            {
                TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                TempGraphics.InterpolationMode = InterpolationMode.High;
                TempGraphics.CompositingQuality = CompositingQuality.HighQuality;
                if (Form_Loader._FormReference.CC_GameAnimations.Checked == false)
                {
                    TempGraphics.DrawImage(PuzzlePieces[PieceIndex].PieceImage, new Point(BlankPieceCollumn * _PieceWidth, BlankPieceRow * _PieceHeight));
                } 
                using (Brush TempBrush = new SolidBrush(Color.FromArgb(0, 45, 90)))
                {
                    TempGraphics.FillRectangle(TempBrush, new Rectangle(new Point(ClickCollumn * _PieceWidth, ClickRow * _PieceHeight), new Size(_PieceWidth, _PieceHeight)));
                }
            }

            if (Form_Loader._FormReference.CC_GameAnimations.Checked)
            {
                BackgroundImage.Dispose();
                if (CleanBackground != null)
                {
                    CleanBackground.Dispose();
                } 
                CleanBackground = TempBitmap;
                PizzleImage = PuzzlePieces[PieceIndex].PieceImage;
                StartPoint = new Point(ClickCollumn * _PieceWidth , ClickRow * _PieceHeight);
                DestinationPoint = new Point(BlankPieceCollumn * _PieceWidth , BlankPieceRow * _PieceHeight);
                CurrentPoint = new Point(ClickCollumn * _PieceWidth, ClickRow * _PieceHeight );
                AnimationStep = new Point((DestinationPoint.X - StartPoint.X) / AnimationSteps, (DestinationPoint.Y - StartPoint.Y) / AnimationSteps);
                AnimationTickCounter = 0;
                MouseClick -= GamePanel_MouseClick;
                Thread TrheadTemp = new Thread(AniMove);
                TrheadTemp.Start();             
            }
            else
            {
                BackgroundImage = TempBitmap;
                BeginInvoke(new Action(() => { CheckVictory(); }));
            }
        }

        private Point StartPoint;
        private Point DestinationPoint;
        private Point CurrentPoint;
        private Point AnimationStep;
        private Bitmap CleanBackground;
        private Bitmap PizzleImage;
        private int AnimationTickCounter;
        private readonly int AnimationSteps = 8;
        private void AniMove()
        {
            for (int i = 0; i <= AnimationSteps; i++)
            {
                if (AnimationTickCounter == AnimationSteps)
                {
                    CurrentPoint.X = DestinationPoint.X;
                    CurrentPoint.Y = DestinationPoint.Y;
                }
                Bitmap TempBitmap = new Bitmap(CleanBackground);
                using (Graphics TempGraphics = Graphics.FromImage(TempBitmap))
                {
                    TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    TempGraphics.InterpolationMode = InterpolationMode.High;
                    TempGraphics.CompositingQuality = CompositingQuality.HighQuality;
                    TempGraphics.DrawImage(PizzleImage, new Point(CurrentPoint.X, CurrentPoint.Y));
                }
                BackgroundImage.Dispose();
                Invoke(new Action(() => { BackgroundImage = TempBitmap; }));

                AnimationTickCounter += 1;
                CurrentPoint.X += AnimationStep.X;
                CurrentPoint.Y += AnimationStep.Y;

                Thread.Sleep(1);
            }
            MouseClick += GamePanel_MouseClick;
            BeginInvoke(new Action(() => { CheckVictory(); }));
        }

        private void CheckVictory()
        {
            for (int PieceIndex = 0; PieceIndex < PuzzlePieces.Count - 1; PieceIndex++)
            {
                if (PuzzlePieces[PieceIndex].PieceIndex != PuzzlePieces[PieceIndex].BoardIndex)
                {
                    return;
                } 
            }

            Form_Loader._FormReference.GB_RestartGame.Enabled = false;
            BackgroundImage.Dispose();
            BackgroundImage = CompleteImage;
            for (int PieceIndex = 0; PieceIndex < PuzzlePieces.Count; PieceIndex++)
            {
                PuzzlePieces[PieceIndex].PieceImage.Dispose();
            }
            PuzzlePieces.Clear();
            Paint += Custom_Panel_Paint;
        }
    }

    public class PuzzlePiece
    {
        public int PieceIndex { get; set; }
        public int BoardIndex { get; set; }
        public Bitmap PieceImage { get; set; }
    }
}
