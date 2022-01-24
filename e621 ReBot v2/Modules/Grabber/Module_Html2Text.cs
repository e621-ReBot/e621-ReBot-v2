using HtmlAgilityPack;
using System;
using System.Net;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Html2Text
    {
        public static string DecodeText(string InputString)
        {
            string OutputString = null;
            if (InputString != null)
            {
                OutputString = WebUtility.HtmlDecode(InputString).Trim();
                OutputString = OutputString.Replace("http://", "https://");
            }
            if (OutputString != null)
            {
                OutputString = string.IsNullOrEmpty(OutputString) ? null : OutputString;
            }

            return OutputString;
        }



        public static string Html2Text_Inkbunny(HtmlNode TextHolderNode)
        {
            string TextString = null;

            if (TextHolderNode.InnerHtml.Contains("<span"))
            {
                foreach (HtmlNode SpanNode in TextHolderNode.SelectNodes(".//span"))
                {
                    SpanNode.ParentNode.RemoveChild(SpanNode, true);
                }
            }

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_Inkbunny(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_Inkbunny(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        string TextTest = TextHolderNode.InnerText.Trim();
                        //if (TextHolderNode.InnerText.Contains("\r\n\t"))
                        //{
                        //    TextHolder += TextHolderNode.InnerText.Replace(" ", "💩").Trim().Replace("💩", " ");
                        //}
                        //else
                        if (TextTest.Length != 0)
                        {
                            TextHolder += TextHolderNode.InnerText;
                        }
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "strong":
                    {
                        if (TextHolderNode.FirstChild != null) // can be blank sometimes
                        {
                            if (TextHolderNode.ChildNodes.Count > 1)
                            {
                                TextHolder += $"[b]{Html2Text_Inkbunny(TextHolderNode)}[/b]";
                            }
                            else
                            {
                                TextHolder += $"[b]{ParseNode_Inkbunny(TextHolderNode.FirstChild)}[/b]";
                            }
                        }
                        break;
                    }

                case "em":
                    {
                        if (TextHolderNode.FirstChild != null) // can be blank sometimes
                        {
                            if (!TextHolderNode.FirstChild.Name.Equals("#text"))
                            {
                                if (TextHolderNode.ChildNodes.Count > 1)
                                {
                                    TextHolder += $"[i]{Html2Text_Inkbunny(TextHolderNode)}[/i]";
                                }
                                else
                                {
                                    TextHolder += $"[i]{ParseNode_Inkbunny(TextHolderNode.FirstChild)}[/i]";
                                }
                            }
                            else
                            {
                                TextHolder += $"[i]{TextHolderNode.InnerText}[/i]";
                            }
                        }
                        break;
                    }

                case "a":
                    {
                        // Skip image icons
                        if (TextHolderNode.FirstChild != null) // can be blank sometimes https://inkbunny.net/s/2153286
                        {
                            if (TextHolderNode.FirstChild.Name.Equals("img"))
                            {
                                if (TextHolderNode.FirstChild.Attributes["src"].Value.Contains("internet-furaffinity.png"))
                                {
                                    TextHolder += "🦊";
                                }
                            }
                            else
                            {
                                TextHolder += $"\"{TextHolderNode.InnerText}\":{TextHolderNode.Attributes["href"].Value}";
                            }
                        }
                        break;
                    }

                case "table":
                    {
                        HtmlNode SubElement = TextHolderNode.SelectSingleNode(".//a[@class='widget_userNameSmall']");
                        if (SubElement != null)
                        {
                            TextHolder += $"🐰\"{SubElement.InnerText}\":{SubElement.Attributes["href"].Value}";
                            break;
                        }

                        SubElement = TextHolderNode.SelectSingleNode(".//div[@class='widget_imageFromSubmission ']");
                        if (SubElement != null)
                        {
                            string PicUrl = SubElement.SelectSingleNode(".//img").Attributes["src"].Value;
                            string PostURL = null;
                            if (PicUrl.Contains("overlays/blocked.png")) // https://inkbunny.net/s/2163614
                            {
                                PostURL = $"https://inkbunny.net{TextHolderNode.SelectSingleNode(".//a").Attributes["href"].Value}";
                                TextHolder += $"🐰\"{PostURL}\":{PostURL}";
                            }
                            else // https://inkbunny.net/s/2165811
                            {
                                PostURL = SubElement.SelectSingleNode(".//img").ParentNode.Attributes["href"].Value;
                                if (PostURL.Substring(0, 3).Equals("/s/")) // https://inkbunny.net/s/2075088
                                {
                                    PostURL = $"https://inkbunny.net{PostURL}";
                                }
                                TextHolder += $"🐰\"{SubElement.Attributes["title"].Value}\":{PostURL}";
                            }
                            break;
                        }
                        break;
                    }

                default:
                    {
                        if (TextHolderNode.Name.Equals("div"))
                        {
                            switch (TextHolderNode.Attributes["class"].Value)
                            {
                                case "bbcode_quote":
                                    {
                                        TextHolder += $"[quote]{TextHolderNode.InnerText}[/quote]";
                                        break;
                                    }

                                case "align_center":
                                    {
                                        TextHolder += Html2Text_Inkbunny(TextHolderNode);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            TextHolder += "UNKNOWN ELEMENT";
                        }
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_Pixiv(HtmlNode TextHolderNode)
        {
            string TextString = null;

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_Pixiv(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_Pixiv(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText;
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "strong":
                    {
                        TextHolder += !TextHolderNode.FirstChild.Name.Equals("#text") ? "[b]" + ParseNode_Pixiv(TextHolderNode.FirstChild) + " [/b]" : "[b]" + TextHolderNode.InnerText + "[/b]";
                        break;
                    }

                case "a":
                    {
                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        if (aURL.StartsWith("/jump.php?", StringComparison.OrdinalIgnoreCase))
                        {
                            aURL = aURL.Substring(10);
                        }
                        TextHolder += string.Format("\"{0}\":{1} ", TextHolderNode.InnerText, aURL);
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_FurAffinity(HtmlNode TextHolderNode)
        {
            string TextString = null;

            //remove colors
            HtmlNodeCollection SelectColorNodes = TextHolderNode.SelectNodes(".//span[starts-with(@style, 'color:')]");
            if (SelectColorNodes != null)
            {
                foreach (HtmlNode SpanNode in SelectColorNodes)
                {
                    SpanNode.ParentNode.RemoveChild(SpanNode, true);
                }

            }

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_FurAffinity(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_FurAffinity(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText;
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "i":
                    {
                        TextHolder += "[i]" + TextHolderNode.InnerText + "[/i]";
                        break;
                    }

                case "u":
                    {
                        TextHolder += "[u]" + TextHolderNode.InnerText + "[/u]";
                        break;
                    }

                case "hr":
                    {
                        TextHolder += "--------------------------------";
                        break;
                    }

                case "strong":
                    {
                        if (TextHolderNode.FirstChild != null) // can be blank sometimes https://www.furaffinity.net/view/36905527/
                        {
                            if (TextHolderNode.ChildNodes.Count > 1)
                            {
                                TextHolder += "[b]" + Html2Text_FurAffinity(TextHolderNode) + " [/b]";
                            }
                            else
                            {
                                TextHolder += "[b]" + ParseNode_FurAffinity(TextHolderNode.FirstChild) + " [/b]";
                            }
                        }
                        break;
                    }

                case "a":
                    {
                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        string TempTextHolder = TextHolderNode.InnerText.Replace("&nbsp;", " ").Trim();
                        // parsed_nav_links doesn't have class attribute
                        if (TextHolderNode.Attributes["class"] != null)
                        {
                            if (TextHolderNode.Attributes["class"].Value.Equals("linkusername") || TextHolderNode.Attributes["class"].Value.Equals("iconusername"))
                            {
                                aURL = "https://www.furaffinity.net" + aURL;
                                TextHolder += "🦊";
                            }
                            TextHolder += TempTextHolder == null ? string.Format("\"{0}\":{1} ", TextHolderNode.Attributes["title"].Value, aURL) : string.Format("\"{0}\":{1} ", TempTextHolder, aURL);
                        }
                        else
                        {
                            aURL = "https://www.furaffinity.net" + aURL;
                            TextHolder += string.Format("\"{0}\":{1} ", TempTextHolder, aURL);
                        }
                        break;
                    }

                case "span":
                    {
                        switch (TextHolderNode.Attributes["class"].Value)
                        {
                            case "bbcode_quote":
                                {
                                    TextHolder += TextHolderNode.ChildNodes.Count > 1 ? "[quote]" + Html2Text_FurAffinity(TextHolderNode) + " [/quote]" : "[quote]" + ParseNode_FurAffinity(TextHolderNode.FirstChild) + " [/quote]";
                                    break;
                                }

                            default:
                                {
                                    TextHolder += TextHolderNode.ChildNodes.Count > 1 ? Html2Text_FurAffinity(TextHolderNode) : ParseNode_FurAffinity(TextHolderNode.FirstChild);
                                    break;
                                }
                        }
                        break;
                    }

                case "h2": //https://www.furaffinity.net/view/39735601/
                case "code":
                case "sub": // https://www.furaffinity.net/view/36370763/
                    {
                        TextHolder += Html2Text_FurAffinity(TextHolderNode);
                        break;
                    }

                case "div":
                    {
                        switch (TextHolderNode.Attributes["class"].Value)
                        {
                            case "submission-footer": // https://www.furaffinity.net/view/36368046/
                                {
                                    TextHolder += Environment.NewLine + Environment.NewLine + Html2Text_FurAffinity(TextHolderNode);
                                    break;
                                }

                            default:
                                {
                                    TextHolder += "UNKNOWN DIV";
                                    break;
                                }
                        }
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_Newgrounds(HtmlNode TextHolderNode)
        {
            string TextString = null;

            if (TextHolderNode == null)
            {
                return null;
            }

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_Newgrounds(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_Newgrounds(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "p":
                    {
                        TextHolder += TextHolderNode.FirstChild != null ? ParseNode_Newgrounds(TextHolderNode.FirstChild) : null;
                        break;
                    }

                case "#text":
                    {
                        string TempTextHolder = TextHolderNode.InnerText;
                        if (TempTextHolder.Length > 0)
                        {
                            TextHolder += TempTextHolder + Environment.NewLine;
                        }
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "i":
                    {
                        TextHolder += "[i]" + TextHolderNode.InnerText + "[/i]";
                        break;
                    }

                case "a":
                    {
                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        TextHolder += string.Format("\"{0}\":{1} ", TextHolderNode.InnerText, aURL) + Environment.NewLine;
                        break;
                    }

                case "img":
                    {
                        TextHolder = null; // skip images
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT" + Environment.NewLine;
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_HicceArs(HtmlNode TextHolderNode)
        {
            string TextString = null;

            HtmlNodeCollection SelectPNodes = TextHolderNode.SelectNodes("./p");
            foreach (HtmlNode Line in SelectPNodes)
            {
                TextString += ParseNode_HicceArs(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_HicceArs(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.FirstChild.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText;
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }




        public static string Html2Text_SoFurry(HtmlNode TextHolderNode)
        {
            string TextString = null;


            if (TextHolderNode == null)
            {
                return null;
            }

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_SoFurry(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_SoFurry(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText + Environment.NewLine;
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "a":
                    {
                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        TextHolder += string.Format("\"{0}\":{1} ", TextHolderNode.InnerText, aURL) + Environment.NewLine;
                        break;
                    }

                case "div":
                    {
                        TextHolder += Html2Text_SoFurry(TextHolderNode);
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_Weasyl(HtmlNode TextHolderNode)
        {
            string TextString = null;


            if (TextHolderNode == null)
            {
                return null;
            }

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_Weasyl(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_Weasyl(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText;
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "a":
                    {

                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        TextHolder += string.Format("\"{0}\":{1} ", TextHolderNode.InnerText, aURL);
                        break;
                    }

                case "em":
                    {
                        TextHolder += "[i]" + TextHolderNode.InnerText + "[/i]";
                        break;
                    }

                case "p":
                case "div":
                    {
                        TextHolder += Html2Text_Weasyl(TextHolderNode);
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }



        public static string Html2Text_HentaiFoundry(HtmlNode TextHolderNode)
        {
            string TextString = null;

            foreach (HtmlNode Line in TextHolderNode.ChildNodes)
            {
                TextString += ParseNode_HentaiFoundry(Line);
            }

            return DecodeText(TextString);
        }

        private static string ParseNode_HentaiFoundry(HtmlNode TextHolderNode)
        {
            string TextHolder = null;
            switch (TextHolderNode.Name)
            {
                case "#text":
                    {
                        TextHolder += TextHolderNode.InnerText;
                        break;
                    }

                case "br":
                    {
                        TextHolder += Environment.NewLine;
                        break;
                    }

                case "i":
                    {
                        TextHolder += "[i]" + TextHolderNode.InnerText + "[/i]";
                        break;
                    }

                case "u":
                    {
                        TextHolder += "[u]" + TextHolderNode.InnerText + "[/u]";
                        break;
                    }

                case "a":
                    {
                        string aURL = WebUtility.UrlDecode(TextHolderNode.Attributes["href"].Value);
                        string TempTextHolder = TextHolderNode.InnerText.Replace("&nbsp;", " ").Trim();
                        TextHolder += $"\"{TempTextHolder}\":{aURL}";
                        break;
                    }

                case "strong":
                    {
                        if (TextHolderNode.ChildNodes.Count > 1)
                        {
                            TextHolder += $"[b]{Html2Text_FurAffinity(TextHolderNode)}[/b]";
                        }
                        else
                        {
                            TextHolder += $"[b]{ParseNode_FurAffinity(TextHolderNode.FirstChild)}[/b]";
                        }
                        break;
                    }

                case "span":
                    {
                        switch (TextHolderNode.Attributes["class"].Value)
                        {
                            case "bbcode_quote":
                                {
                                    TextHolder += TextHolderNode.ChildNodes.Count > 1 ? "[quote]" + Html2Text_FurAffinity(TextHolderNode) + " [/quote]" : "[quote]" + ParseNode_FurAffinity(TextHolderNode.FirstChild) + " [/quote]";
                                    break;
                                }

                            default:
                                {
                                    TextHolder += TextHolderNode.ChildNodes.Count > 1 ? Html2Text_FurAffinity(TextHolderNode) : ParseNode_FurAffinity(TextHolderNode.FirstChild);
                                    break;
                                }
                        }
                        break;
                    }

                case "h2": //https://www.furaffinity.net/view/39735601/
                case "code":
                case "sub": // https://www.furaffinity.net/view/36370763/
                    {
                        TextHolder += Html2Text_FurAffinity(TextHolderNode);
                        break;
                    }

                case "div":
                    {
                        switch (TextHolderNode.Attributes["class"].Value)
                        {
                            case "submission-footer": // https://www.furaffinity.net/view/36368046/
                                {
                                    TextHolder += Environment.NewLine + Environment.NewLine + Html2Text_FurAffinity(TextHolderNode);
                                    break;
                                }

                            default:
                                {
                                    TextHolder += "UNKNOWN DIV";
                                    break;
                                }
                        }
                        break;
                    }

                default:
                    {
                        TextHolder += "UNKNOWN ELEMENT";
                        break;
                    }
            }

            return TextHolder;
        }
    }
}
