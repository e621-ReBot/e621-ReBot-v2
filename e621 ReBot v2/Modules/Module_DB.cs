﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace e621_ReBot_v2.Modules
{
    public static class Module_DB
    {
        public readonly static SQLiteConnection SQL_Connection = new SQLiteConnection("Data Source=d.b; Version=3;");

        public static void CreateDBs()
        {
            SQL_Connection.Open();
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "CREATE TABLE IF NOT EXISTS [Database] ([Url] TEXT UNIQUE, [Rating] TEXT, [PostID] MEDIUMINT UNSIGNED)";
                SQL_Command.ExecuteNonQuery();

                SQL_Command.CommandText = "CREATE TABLE IF NOT EXISTS [ArtistAlias] ([Name] TEXT, [Website] TEXT, [Alias] TEXT)";
                SQL_Command.ExecuteNonQuery();

                SQL_Command.CommandText = "CREATE TABLE IF NOT EXISTS [CustomTags] ([CustomTag] TEXT)";
                SQL_Command.ExecuteNonQuery();
            }
        }

        // = = = = =

        public static void DB_Media_CreateRecord(in DataRow DataRowRef)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "INSERT OR IGNORE INTO [Database] ([Url], [Rating], [PostID]) VALUES (@url, @rating, @postid)";
                SQL_Command.Parameters.AddWithValue("@url", DataRowRef["Grab_MediaURL"]);
                SQL_Command.Parameters.AddWithValue("@rating", DataRowRef["Upload_Rating"]);
                SQL_Command.Parameters.AddWithValue("@postid", DataRowRef["Uploaded_As"]);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static void DB_Media_RemoveRecord(in DataRow DataRowRef)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "DELETE FROM [Database] WHERE [PostID] = @postid";
                SQL_Command.Parameters.AddWithValue("@postid", DataRowRef["Uploaded_As"]);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static void DB_Media_CheckRecord(ref DataRow DataRowRef)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "SELECT * FROM [Database] WHERE ([Url]=@picurl)";
                SQL_Command.Parameters.AddWithValue("@picurl", DataRowRef["Grab_MediaURL"]);
                using (SQLiteDataReader SQL_Reader = SQL_Command.ExecuteReader())
                {
                    if (SQL_Reader.HasRows)
                    {
                        SQL_Reader.Read(); // read first row, REQUIRED!
                        DataRowRef["Upload_Rating"] = SQL_Reader.GetString(1);
                        DataRowRef["Uploaded_As"] = SQL_Reader.GetValue(2).ToString();
                    }
                }
            }
        }

        public static void DB_Media_DeleteRecord(string PostID)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "DELETE FROM [Database] WHERE ([PostID]=@postID)";
                SQL_Command.Parameters.AddWithValue("@postID", PostID);
                SQL_Command.ExecuteNonQuery();
            }
        }

        // = = = = =

        public static void DB_AA_CreateRecord(string ArtistName, string Website, string NewAlias)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "INSERT OR IGNORE INTO [ArtistAlias] ([Name], [Website], [Alias]) VALUES (@name, @website, @alias)";
                SQL_Command.Parameters.AddWithValue("@name", ArtistName);
                SQL_Command.Parameters.AddWithValue("@website", WebsiteURLFix(Website));
                SQL_Command.Parameters.AddWithValue("@alias", NewAlias);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static string DB_AA_CheckRecord(string ArtistName, string Website)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "SELECT * FROM [ArtistAlias] WHERE ([Name]=@name AND [Website]=@website)";
                SQL_Command.Parameters.AddWithValue("@name", ArtistName);
                SQL_Command.Parameters.AddWithValue("@website", WebsiteURLFix(Website));
                using (SQLiteDataReader SQL_Reader = SQL_Command.ExecuteReader())
                {
                    if (SQL_Reader.HasRows)
                    {
                        SQL_Reader.Read(); // read first row
                        return SQL_Reader.GetString(2);
                    }
                }
            }
            return null;
        }

        public static void DB_AA_UpdateRecord(string ArtistName, string Website, string NewAlias)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "UPDATE [ArtistAlias] SET [Alias]=@alias WHERE ([Name]=@name AND [Website]=@website)";
                SQL_Command.Parameters.AddWithValue("@name", ArtistName);
                SQL_Command.Parameters.AddWithValue("@website", WebsiteURLFix(Website));
                SQL_Command.Parameters.AddWithValue("@alias", NewAlias);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static void DB_AA_DeleteRecord(string ArtistName, string Website)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "DELETE FROM [ArtistAlias] WHERE ([Name]=@name AND [Website]=@website)";
                SQL_Command.Parameters.AddWithValue("@name", ArtistName);
                SQL_Command.Parameters.AddWithValue("@website", WebsiteURLFix(Website));
                SQL_Command.ExecuteNonQuery();
            }
        }

        private static string WebsiteURLFix(string Website)
        {
            Uri URLURI = new Uri(Website);
            return URLURI.Host;
        }

        // = = = = =

        public static void DB_CT_CreateRecord(string CustomTag)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "INSERT OR IGNORE INTO [CustomTags] ([CustomTag]) VALUES (@tag)";
                SQL_Command.Parameters.AddWithValue("@tag", CustomTag);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static void DB_CT_DeleteRecord(string CustomTag)
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "DELETE FROM [CustomTags] WHERE ([CustomTag]=@tag)";
                SQL_Command.Parameters.AddWithValue("@tag", CustomTag);
                SQL_Command.ExecuteNonQuery();
            }
        }

        public static List<string> DB_CT_ReadTable ()
        {
            using (SQLiteCommand SQL_Command = SQL_Connection.CreateCommand())
            {
                SQL_Command.CommandText = "SELECT * FROM [CustomTags]";
                using (SQLiteDataReader SQL_Reader = SQL_Command.ExecuteReader())
                {
                    List<string> TempList = new List<string>();
                    if (SQL_Reader.HasRows)
                    {       
                        while (SQL_Reader.Read())
                        {
                            TempList.Add(SQL_Reader.GetString(0));
                        }
                    }
                    return TempList;
                }
            }
        }

    }
}