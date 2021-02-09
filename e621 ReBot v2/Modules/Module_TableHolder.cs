using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace e621_ReBot_v2.Modules
{
    public static class Module_TableHolder
    {
        static Module_TableHolder()
        {
            Create_DBTable(ref Database_Table);
            Create_UploadTable(ref Upload_Table);
            Create_DownloadTable(ref Download_Table);
        }



        public static DataTable Database_Table = new DataTable();
        public static void Create_DBTable(ref DataTable ReferenceTable)
        {
            // \\\ = = = = = Always present
            ReferenceTable.Columns.Add("Grab_URL", typeof(string));
            ReferenceTable.Columns.Add("Grab_DateTime", typeof(DateTime));
            ReferenceTable.Columns.Add("Grab_Title", typeof(string));
            ReferenceTable.Columns.Add("Grab_TextBody", typeof(string));
            ReferenceTable.Columns.Add("Grab_MediaURL", typeof(string));
            ReferenceTable.Columns.Add("Grab_ThumbnailURL", typeof(string));

            // \\\ = = = = = 
            ReferenceTable.Columns.Add("Thumbnail_Image", typeof(Image));
            ReferenceTable.Columns.Add("Thumbnail_DLStart", typeof(bool));
            ReferenceTable.Columns.Add("Thumbnail_FullInfo", typeof(bool));

            // \\\ = = = = = 
            ReferenceTable.Columns.Add("Info_MediaFormat", typeof(string));
            ReferenceTable.Columns.Add("Info_MediaWidth", typeof(int));
            ReferenceTable.Columns.Add("Info_MediaHeight", typeof(int));
            ReferenceTable.Columns.Add("Info_MediaByteLength", typeof(int));
            ReferenceTable.Columns.Add("Info_MediaMD5", typeof(string));
            ReferenceTable.Columns.Add("Info_TooBig", typeof(bool));

            // \\\ = = = = = 
            //ReferenceTable.Columns.Add("Upload_Rating", typeof(string));
            DataColumn TempDataColumn0 = new DataColumn
            {
                ColumnName = "Upload_Rating",
                DataType = typeof(string),
                DefaultValue = "E"
            };
            ReferenceTable.Columns.Add(TempDataColumn0);
            ReferenceTable.Columns.Add("Upload_Tags", typeof(string));
            ReferenceTable.Columns.Add("Upload_ParentOffset", typeof(DataRow));
            ReferenceTable.Columns.Add("Uploaded_As", typeof(string));
            ReferenceTable.Columns.Add("Inferior_ID", typeof(string));
            ReferenceTable.Columns.Add("Inferior_Description", typeof(string));
            ReferenceTable.Columns.Add("Inferior_Sources", typeof(List<string>));
            ReferenceTable.Columns.Add("Inferior_ParentID", typeof(string));
            ReferenceTable.Columns.Add("Inferior_Children", typeof(List<string>));
            ReferenceTable.Columns.Add("Inferior_HasNotes", typeof(bool));
            ReferenceTable.Columns.Add("Inferior_NotesSizeRatio", typeof(double));

            // \\\ = = = = = 
            //ReferenceTable.Columns.Add("UPDL_Queued", typeof(bool));
            DataColumn TempDataColumn1 = new DataColumn
            {
                ColumnName = "UPDL_Queued",
                DataType = typeof(bool),
                DefaultValue = false
            };
            ReferenceTable.Columns.Add(TempDataColumn1);

            // \\\ = = = = = Download Stuff
            ReferenceTable.Columns.Add("Artist", typeof(string));
            ReferenceTable.Columns.Add("DL_FilePath", typeof(string));
            ReferenceTable.Columns.Add("DL_ImageID", typeof(string));
        }



        public static DataTable Upload_Table = new DataTable();
        public static void Create_UploadTable(ref DataTable ReferenceTable)
        {
            ReferenceTable.Columns.Add("Grab_MediaURL", typeof(string));
            ReferenceTable.Columns.Add("DataRowRef", typeof(DataRow));
            DataColumn TempDataColumn0 = new DataColumn
            {
                ColumnName = "Upload",
                Caption = "Upload",
                DataType = typeof(bool),
                DefaultValue = true
            };
            ReferenceTable.Columns.Add(TempDataColumn0);
            DataColumn TempDataColumn1 = new DataColumn
            {
                ColumnName = "CopyNotes",
                Caption = "Copy Notes",
                DataType = typeof(bool),
                DefaultValue = false
            };
            ReferenceTable.Columns.Add(TempDataColumn1);
            DataColumn TempDataColumn2 = new DataColumn
            {
                ColumnName = "MoveChildren",
                Caption = "Move Children",
                DataType = typeof(bool),
                DefaultValue = false
            };
            ReferenceTable.Columns.Add(TempDataColumn2);
            DataColumn TempDataColumn3 = new DataColumn
            {
                ColumnName = "FlagInferior",
                Caption = "Flag Inferior",
                DataType = typeof(bool),
                DefaultValue = false
            };
            ReferenceTable.Columns.Add(TempDataColumn3);
            DataColumn TempDataColumn4 = new DataColumn
            {
                ColumnName = "ChangeParent",
                Caption = "Change Parent",
                DataType = typeof(bool),
                DefaultValue = false
            };
            ReferenceTable.Columns.Add(TempDataColumn4);
        }

        public static bool UploadQueueContainsURL(string MediaURL)
        {
            return Upload_Table.AsEnumerable().Any(RowData => MediaURL == (string)RowData[0]);
        }


        public static DataTable Download_Table = new DataTable();
        public static void Create_DownloadTable(ref DataTable ReferenceTable)
        {
            ReferenceTable.Columns.Add("DataRowRef", typeof(DataRow));
            ReferenceTable.Columns.Add("Grab_URL", typeof(string));
            ReferenceTable.Columns.Add("Grab_MediaURL", typeof(string));
            ReferenceTable.Columns.Add("Grab_ThumbnailURL", typeof(string));
            ReferenceTable.Columns.Add("Artist", typeof(string));
            ReferenceTable.Columns.Add("Grab_Title", typeof(string));
            ReferenceTable.Columns.Add("e6_PostID", typeof(string));
            ReferenceTable.Columns.Add("e6_PoolName", typeof(string));
            ReferenceTable.Columns.Add("e6_PoolPostIndex", typeof(string));
        }

        public static bool DownloadQueueContainsURL(string MediaURL)
        {
            return Download_Table.AsEnumerable().Any(RowData => MediaURL == (string)RowData[2]);
        }

        public static void DownloadQueueRemoveURL(string MediaURL)
        {
            lock (Download_Table)
            {
                for (int i = Download_Table.Rows.Count - 1; i >= 0; i--)
                {
                    if (Download_Table.Rows[i][2].ToString().Equals(MediaURL))
                    {
                        Download_Table.Rows.RemoveAt(i);
                    }
                }
            }
        }

        public static DataTable ReverseDataTable(DataTable originalDT)
        {
            DataTable reversedDT = originalDT.Clone();
            for (int i = originalDT.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = originalDT.Rows[i];
                reversedDT.ImportRow(row);
            }
            reversedDT.AcceptChanges();
            return reversedDT;
        }
    }
}
    
