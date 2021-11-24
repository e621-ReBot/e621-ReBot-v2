using e621_ReBot_v2;
using e621_ReBot_v2.Modules;
using e621_ReBot_v2.Modules.Grabber;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

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
            DataRowTemp["Grab_DateTime"] = new DateTime(2005, 12, 04, 19, 31, 00);
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

        [TestMethod]
        public void Test_Newgrounds()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.newgrounds.com/art/view/banzchan/bomber-man";
            DataRowTemp["Grab_DateTime"] = new DateTime(2009, 08, 26, 9, 52, 00);
            DataRowTemp["Grab_Title"] = "⮚ \"Bomber Man\" ⮘ by Banzchan on Newgrounds";
            DataRowTemp["Grab_TextBody"] = "Created in March 2006. Posted on my Deviantart. Four hour drawing using pencils, a Copic brush pen, 02 Micron, and Photoshop 5. I was bored and wanted to do something that was quick and high octane.\r\n\n\n\r\nSweet! This got featured! Thanks Newgrounds. :D ";
            DataRowTemp["Grab_MediaURL"] = "https://art.ngfiles.com/images/39000/39423_banzchan_bomber-man.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://art.ngfiles.com/thumbnails/39000/39423_full.jpg";
            DataRowTemp["Thumbnail_FullInfo"] = true;
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaWidth"] = 508;
            DataRowTemp["Info_MediaHeight"] = 700;
            DataRowTemp["Info_MediaByteLength"] = 326784;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2009";
            DataRowTemp["Artist"] = "Banzchan";

            Module_Grabber._GrabQueue_URLs.Add("https://www.newgrounds.com/art/view/banzchan/bomber-man");
            Module_Newgrounds.Grab("https://www.newgrounds.com/art/view/banzchan/bomber-man", true);
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.newgrounds.com/art/view/banzchan/bomber-man"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_HicceArs()
        {
            //Set cookies first, doesn't work without it.
            HttpWebRequest HTMLWebRequest = (HttpWebRequest)WebRequest.Create("https://www.hiccears.com/");
            HTMLWebRequest.UserAgent = Form_Loader.GlobalUserAgent;
            HTMLWebRequest.CookieContainer = new CookieContainer();
            HTMLWebRequest.GetResponse().Close();
            Module_CookieJar.Cookies_HicceArs = HTMLWebRequest.CookieContainer;

            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.hiccears.com/picture.php?pid=4";
            DataRowTemp["Grab_DateTime"] = new DateTime(2016, 02, 05, 5, 32, 07);
            DataRowTemp["Grab_Title"] = "⮚ \"妈妈生日快乐\" ⮘ by Lee on HicceArs";
            DataRowTemp["Grab_TextBody"] = "Description: \n\t\t\t\t\t\t\t果然还是暑假时间比较多 ";
            DataRowTemp["Grab_MediaURL"] = "https://www.hiccears.com/upl0ads/imgs/b39b7ba7e07d710447e89c40277171aa4c568e3d-08e4970d7579288af5bf3f4393a35bc12ba5b2794391eccf63a32706d7a2d0402f2289c9512d95d00d61bdec03d2b99d6ecc455ee5644ae52d10e7c-a61c93062dc97a3.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://www.hiccears.com/upl0ads/thumbnails/b39b7ba7e07d710447e89c40277171aa4c568e3d-08e4970d7579288af5bf3f4393a35bc12ba5b2794391eccf63a32706d7a2d0402f2289c9512d95d00d61bdec03d2b99d6ecc455ee5644ae52d10e7c-a61c93062dc97a3.png";
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaByteLength"] = 342863;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2016";
            DataRowTemp["Artist"] = "Lee";

            Module_Grabber._GrabQueue_URLs.Add("https://www.hiccears.com/picture.php?pid=4");
            Module_HicceArs.Grab("https://www.hiccears.com/picture.php?pid=4");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.hiccears.com/picture.php?pid=4"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_SoFurry()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.sofurry.com/view/13262";
            DataRowTemp["Grab_Title"] = "\"puppy1\" by kogie on SoFurry";
            DataRowTemp["Grab_MediaURL"] = "https://www.sofurryfiles.com/std/content?page=13262&kogie-puppy1.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://www.sofurryfiles.com/std/thumb?page=13262";
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "";
            DataRowTemp["Artist"] = "kogie";

            Module_Grabber._GrabQueue_URLs.Add("https://www.sofurry.com/view/13262");
            Module_SoFurry.Grab("https://www.sofurry.com/view/13262");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.sofurry.com/view/13262"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_Mastodon()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://mastodon.social/@Gargron/6349";
            DataRowTemp["Grab_DateTime"] = new DateTime(2016, 09, 07, 18, 32, 37);
            DataRowTemp["Grab_Title"] = "Created by Eugen (@Gargron@mastodon.social)";
            DataRowTemp["Grab_TextBody"] = "Guess what";
            DataRowTemp["Grab_MediaURL"] = "https://files.mastodon.social/media_attachments/files/000/000/018/original/27-harambe-gorilla-heaven.w710.h473.2x.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://files.mastodon.social/media_attachments/files/000/000/018/small/27-harambe-gorilla-heaven.w710.h473.2x.jpg";
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaByteLength"] = 490189;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2016";
            DataRowTemp["Artist"] = "Eugen (@Gargron@mastodon.social)";

            Module_Grabber._GrabQueue_URLs.Add("https://mastodon.social/@Gargron/6349");
            using (WebClient WebClientTemp = new WebClient())
            {
                Module_Mastodon.Grab("https://mastodon.social/@Gargron/6349", WebClientTemp.DownloadString("https://mastodon.social/@Gargron/6349"));
            }
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://mastodon.social/@Gargron/6349"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_Plurk()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.plurk.com/p/olweca";
            DataRowTemp["Grab_DateTime"] = new DateTime(2021, 10, 22, 5, 31, 07);
            DataRowTemp["Grab_Title"] = "Plurk by ಠ_ಠ (@anonymous)";
            DataRowTemp["Grab_TextBody"] = "where is Lilith?? ";
            DataRowTemp["Grab_MediaURL"] = "https://images.plurk.com/4aHuWFbJNjZHyEsy0YVP2d.png";
            DataRowTemp["Grab_ThumbnailURL"] = "https://images.plurk.com/mx_4aHuWFbJNjZHyEsy0YVP2d.jpg";
            DataRowTemp["Info_MediaFormat"] = "png";
            DataRowTemp["Info_MediaByteLength"] = 178564;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2021";
            DataRowTemp["Artist"] = "ಠ_ಠ (@anonymous)";

            Module_Grabber._GrabQueue_URLs.Add("https://www.plurk.com/p/olweca");
            Module_Plurk.Grab("https://www.plurk.com/p/olweca");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.plurk.com/p/olweca"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_Pawoo()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://pawoo.net/@pawoo_russel/106581866880817198";
            DataRowTemp["Grab_DateTime"] = new DateTime(2021, 07, 15, 2, 50, 24);
            DataRowTemp["Grab_Title"] = "Created by pawoo-russell (@pawoo_russel@pawoo.net)";
            DataRowTemp["Grab_TextBody"] = "【タイムライン広告表示のお知らせ】Pawooをご利用いただきありがとうございます。この度Pawooは、「ホームタイムライン」「ローカルタイムライン」への広告表示を開始する事になりました。広告表示のイメージは、添付画像をご覧ください。ユーザーの皆さまには、ご理解をいただけましたら幸いです。今後ともPawooをよろしくお願いします。";
            DataRowTemp["Grab_MediaURL"] = "https://img.pawoo.net/media_attachments/files/037/456/511/original/028af6b9981d5288.jpg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://img.pawoo.net/media_attachments/files/037/456/511/small/028af6b9981d5288.jpg";
            DataRowTemp["Thumbnail_FullInfo"] = true;
            DataRowTemp["Info_MediaFormat"] = "jpg";
            DataRowTemp["Info_MediaWidth"] = 1280;
            DataRowTemp["Info_MediaHeight"] = 1024;
            DataRowTemp["Info_MediaByteLength"] = 162573;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2021";
            DataRowTemp["Artist"] = "pawoo-russell (@pawoo_russel@pawoo.net)";

            Module_Grabber._GrabQueue_URLs.Add("https://pawoo.net/@pawoo_russel/106581866880817198");
            using (WebClient WebClientTemp = new WebClient())
            {
                byte[] htmlData = WebClientTemp.DownloadData("https://pawoo.net/@pawoo_russel/106581866880817198");
                string htmlCode = Encoding.UTF8.GetString(htmlData);
                Module_Mastodon.Grab("https://pawoo.net/@pawoo_russel/106581866880817198", htmlCode);
            }
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://pawoo.net/@pawoo_russel/106581866880817198"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            Assert.IsTrue(RowsAreEqual);
        }

        [TestMethod]
        public void Test_Weasly()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://www.weasyl.com/~haunt/submissions/24/winter-sigh-test-submission";
            DataRowTemp["Grab_DateTime"] = new DateTime(2011, 10, 29, 00, 24, 42);
            DataRowTemp["Grab_Title"] = "⮚ \"Winter Sigh. test submission\" ⮘ by haunt on Weasly";
            DataRowTemp["Grab_MediaURL"] = "https://cdn.weasyl.com/static/media/f6/8d/15/f68d15e5398f39aec37fe0d9d991e444e6ac63e89c26fab76ffa010e64101dfa.png";
            DataRowTemp["Info_MediaFormat"] = "png";
            DataRowTemp["Info_MediaByteLength"] = 251659;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2011";
            DataRowTemp["Artist"] = "haunt";

            Module_Grabber._GrabQueue_URLs.Add("https://www.weasyl.com/~haunt/submissions/24/winter-sigh-test-submission");
            Module_Weasyl.Grab("https://www.weasyl.com/~haunt/submissions/24/winter-sigh-test-submission");
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://www.weasyl.com/~haunt/submissions/24/winter-sigh-test-submission"]).Rows[0];

            int EqualtyCounter = DataTableTemp.Columns.Count;
            for (int i = 0; i <= DataTableTemp.Columns.Count - 1; i++)
            {
                if (DataRowTemp[i].Equals(DataRowResult[i]))
                {
                    EqualtyCounter -= 1;
                }
            }
            //can't test bitmap so check that the other 29 collumns are equal.
            Assert.IsTrue(EqualtyCounter == 1);
        }

        [TestMethod]
        public void Test_Baraag()
        {
            DataTable DataTableTemp = Module_TableHolder.Database_Table.Clone();
            DataRow DataRowTemp = DataTableTemp.NewRow();
            DataRowTemp["Grab_URL"] = "https://baraag.net/@satori/3";
            DataRowTemp["Grab_DateTime"] = new DateTime(2017, 04, 25, 23, 27, 00);
            DataRowTemp["Grab_Title"] = "Created by koi (@satori@baraag.net)";
            DataRowTemp["Grab_TextBody"] = "What does the elephant say? The English onomatopoeia for it is apparently \"baraag\" https://baraag.net/media/ffA6pGLVWD3ghlbCzdE";
            DataRowTemp["Grab_MediaURL"] = "https://baraag.net/system/media_attachments/files/000/000/003/original/8b6599107e3a06e0.jpeg";
            DataRowTemp["Grab_ThumbnailURL"] = "https://baraag.net/system/media_attachments/files/000/000/003/small/8b6599107e3a06e0.jpeg";
            DataRowTemp["Info_MediaFormat"] = "jpeg";
            DataRowTemp["Info_MediaByteLength"] = 134543;
            DataRowTemp["Upload_Rating"] = "E";
            DataRowTemp["Upload_Tags"] = "2017";
            DataRowTemp["Artist"] = "koi (@satori@baraag.net)";

            Module_Grabber._GrabQueue_URLs.Add("https://baraag.net/@satori/3");
            using (WebClient WebClientTemp = new WebClient())
            {
                Module_Mastodon.Grab("https://baraag.net/@satori/3", WebClientTemp.DownloadString("https://baraag.net/@satori/3"));
            }
            DataRow DataRowResult = ((DataTable)Module_Grabber._GrabQueue_WorkingOn["https://baraag.net/@satori/3"]).Rows[0];

            bool RowsAreEqual = DataRowResult.ItemArray.SequenceEqual(DataRowTemp.ItemArray);

            //for (int i = 0; i <= DataTableTemp.Columns.Count - 1; i++)
            //{
            //    Debug.Print(DataRowTemp[i].Equals(DataRowResult[i]).ToString());
            //}

            Assert.IsTrue(RowsAreEqual);
        }
    }
}