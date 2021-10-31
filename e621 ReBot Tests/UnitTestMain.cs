using e621_ReBot_v2.Modules;
using e621_ReBot_v2.Modules.Grabber;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace e621_ReBot_Tests
{
    [TestClass]
    public class UnitTestMain
    {
        [TestMethod]
        public void Test_Inkbunny()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://inkbunny.net/s/2";
            DataRowTemp["Grab_DateTime"] = new DateTime(2010, 03, 03, 14, 26, 00);
            DataRowTemp["Grab_Title"] = "⮚ \"Mapleleaf story\" ⮘ by mek on Inkbunny";
            DataRowTemp["Grab_TextBody"] = "Old art of old characters~ ~ ";
            DataRowTemp["Grab_MediaURL"] = "https://nl.ib.metapix.net/files/full/0/2_mek_spmcover001ed.png";
            DataRowTemp["Grab_ThumbnailURL"] = "https://nl.ib.metapix.net/thumbnails/large/0/2_mek_spmcover001ed_noncustom.jpg";
            DataRowTemp["Info_MediaFormat"] = "png";
            DataRowTemp["Info_MediaByteLength"] = 439296;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2010";
            DataRowTemp["Artist"] = "mek";

            Module_Grabber._GrabQueue_URLs.Add("https://inkbunny.net/s/2");
            Module_Inkbunny.Grab("https://inkbunny.net/s/2");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://inkbunny.net/s/2"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_Pixiv()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.pixiv.net/en/artworks/20";
            DataRowTemp["Grab_DateTime"] = new DateTime(2007, 09, 09, 15, 14, 07);
            DataRowTemp["Grab_Title"] = "⮚ \"2000年\" ⮘ by 馬骨 on Pixiv";
            //DataRowTemp["Grab_TextBody"] = "";
            DataRowTemp["Grab_MediaURL"] = "https://i.pximg.net/img-original/img/2007/09/09/22/14/07/20_p0.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://i.pximg.net/c/250x250_80_a2/img-master/img/2007/09/09/22/14/07/20_p0_square1200.jpg";
            DataRowTemp["Thumbnail_FullInfo"] = true;
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaWidth"] = 450;
            DataRowTemp["Info_MediaHeight"] = 582;
            DataRowTemp["Info_MediaByteLength"] = 47168;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2007";
            DataRowTemp["Artist"] = "馬骨";

            Module_Grabber._GrabQueue_URLs.Add("https://www.pixiv.net/en/artworks/20");
            Module_Pixiv.Grab("https://www.pixiv.net/en/artworks/20");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.pixiv.net/en/artworks/20"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_FurAffinity()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.furaffinity.net/view/1";
            DataRowTemp["Grab_DateTime"] = new DateTime(2005, 12, 04, 18, 31, 00);
            DataRowTemp["Grab_Title"] = "⮚ \"Congratulations!\" ⮘ by alkora on FurAffinity";
            DataRowTemp["Grab_TextBody"] = "You've made it to submission number 1!\r\n\r\nYou gain 1 fat fender :D ";
            DataRowTemp["Grab_MediaURL"] = "https://d.furaffinity.net/art/alkora/1436887262/1133739096.alkora_background.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://t.facdn.net/1@200-1436887262.jpg";
            DataRowTemp["Thumbnail_FullInfo"] = true;
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaWidth"] = 800;
            DataRowTemp["Info_MediaHeight"] = 708;
            DataRowTemp["Info_MediaByteLength"] = 157165;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2005";
            DataRowTemp["Artist"] = "alkora";

            Module_Grabber._GrabQueue_URLs.Add("https://www.furaffinity.net/view/1");
            Module_FurAffinity.Grab("https://www.furaffinity.net/view/1");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.furaffinity.net/view/1"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        //[TestMethod]
        //public void Test_Twitter()
        //{
        //    DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
        //    DataRow DataRowTemp = DataTableTemp.NewRow();
        //    //DataRowTemp["Grab_URL"] = "https://twitter.com/Twitter/status/1410384347748839428";
        //    //DataRowTemp["Grab_DateTime"] = new DateTime(2005, 12, 04, 18, 31, 00);
        //    //DataRowTemp["Grab_Title"] = "⮚ \"Congratulations!\" ⮘ by alkora on FurAffinity";
        //    //DataRowTemp["Grab_TextBody"] = "You've made it to submission number 1!\r\n\r\nYou gain 1 fat fender :D ";
        //    //DataRowTemp["Grab_MediaURL"] = "https://d.furaffinity.net/art/alkora/1436887262/1133739096.alkora_background.jpg";
        //    //DataRowTemp["Grab_ThumbnailURL"] = "https://t.facdn.net/1@200-1436887262.jpg";
        //    //DataRowTemp["Thumbnail_FullInfo"] = true;
        //    //DataRowTemp["Info_MediaFormat"] = "jpg";
        //    //DataRowTemp["Info_MediaWidth"] = 800;
        //    //DataRowTemp["Info_MediaHeight"] = 708;
        //    //DataRowTemp["Info_MediaByteLength"] = 157165;
        //    //DataRowTemp["Upload_Rating"] = "E";
        //    //DataRowTemp["Upload_Tags"] = "2005";
        //    //DataRowTemp["Artist"] = "alkora";

        //    Module_Grabber._GrabQueue_URLs.Add("https://twitter.com/Twitter/status/1410384347748839428");
        //    string HTMLSource = Module_Grabber.GrabPageSource("https://twitter.com/Twitter/status/1410384347748839428", ref Module_CookieJar.Cookies_Twitter);
        //    Module_Twitter.Grab_Status("https://twitter.com/Twitter/status/1410384347748839428", HTMLSource);
        //    DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://twitter.com/Twitter/status/1410384347748839428"]).Rows[0];

        //    bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

        //    Assert.IsTrue(RowsAreEqual);
        //}
    }
}
