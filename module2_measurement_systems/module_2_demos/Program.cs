using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;

namespace PubDashSE
{
    class Program
    {
        private static int[,] statsArray = new int[3, 6];   // publications statistics - this is where we keep the results
        private static int iStartYear = 2011;               // year where we start counting
        private static int iJournalRow = 1;                 // row of the statistics for the journal
        private static int iConferenceRow = 0;              // row of the statistics for the conferences
        private static int iChapterRow = 2;                 // row of the statistics for the book chapters
        private static Hashtable pubIds = new Hashtable();  // publications ids, to avoid double counting from DBs
        private static int iCounter = 0;                    // total counter of the publicatins

        /**
         * Main program function
         * takes no arguments
         **/ 
        static void Main(string[] args)
        {
            // writing out the initial welcome information
            Console.WriteLine("===============================================");
            Console.WriteLine("========= (c) Miroslaw Staron =================");
            Console.WriteLine("======== Publication Dashboard for SE =========");
            Console.WriteLine("===============================================");


            Console.WriteLine("==> Getting the publication list from the server - 1718, GU");
            System.IO.StreamWriter rawDataFile = new System.IO.StreamWriter(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\publications.csv");
            System.IO.StreamWriter statsFile = new System.IO.StreamWriter(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\statistics.csv");
            rawDataFile.WriteLine("Title;Year;Source;Type;Authors");
            string seNames = "Staron;Torkar;Tichy;Bosch;Chaudron;Berger;Berntsson Svensson;Feldt;Hansson;Pareto;Olsson Holmström;Burden;Crnkovic;Ericsson;Hammouda;Heldal;Knauss;Nilsson;Pelliccione;Scandriato;Steghofer;Mamun;Andrade;Alahyari;Alegroth;Antinyan;Gren;Kashfi;Lenberg;Liebel;Martini;Ho Quang;Rana;Hebig";

            
            //string Url = "http://gup.ub.gu.se/lists/publications/departments/xml/index.xsql?ids=1718&lyear=2011&hyear=2020&sort=year&format=default&rows=100000";
            //string Url = "http://gup.ub.gu.se/lists/publications/departments/xml/index.xsql?ids=1294,366,1718&lyear=2011&hyear=2020&sort=year&format=default&rows=100000";
            //string Url = "http://gup.ub.gu.se/lists/publications/departments/xml/index.xsql?ids=1294,366,1718&lyear=2015&hyear=2020&sort=year&format=default&rows=100000";
            string Url = "http://publications.lib.chalmers.se/lists/publications/departments/xml/index.xsql?ids=1294,366,1718&lyear=2011&hyear=2020&sort=year&format=default&rows=100000";

            HttpWebRequest myRequest;
            WebResponse myResponse;
            StreamReader sr;
            string result;
            XmlDocument doc;
            bool isSE;
            
            makeCall(rawDataFile, 
                     seNames, 
                     Url, 
                     out myRequest, 
                     out myResponse, 
                     out sr, 
                     out result, 
                     out doc, 
                     out isSE);

            Console.WriteLine("==> Getting the publication list from the server - 1294, CTH");
            Url = "http://publications.lib.chalmers.se/lists/publications/departments/xml/index.xsql?ids=1294&lyear=2011&hyear=2020&sort=year&format=default&rows=100000";

            makeCall(rawDataFile,
                     seNames,
                     Url,
                     out myRequest,
                     out myResponse,
                     out sr,
                     out result,
                     out doc,
                     out isSE);


            Console.WriteLine("==> Getting the publication list from the server - 366, CTH");
            Url = "http://publications.lib.chalmers.se/lists/publications/departments/xml/index.xsql?ids=366&lyear=2011&hyear=2020";

            makeCall(rawDataFile,
                     seNames,
                     Url,
                     out myRequest,
                     out myResponse,
                     out sr,
                     out result,
                     out doc,
                     out isSE);

                     

            

            Console.WriteLine("==> Getting the publication list from the server - 1718, CTH");
            Url = "http://publications.lib.chalmers.se/lists/publications/departments/xml/index.xsql?ids=1718&lyear=2011&hyear=2020&sort=year&format=default&rows=100000";

            makeCall(rawDataFile,
                     seNames,
                     Url,
                     out myRequest,
                     out myResponse,
                     out sr,
                     out result,
                     out doc,
                     out isSE);

            rawDataFile.Close();
            writeHTML();

            Console.WriteLine("============ Thank you! =======================");
        }

        private static void makeCall(System.IO.StreamWriter rawDataFile, string seNames, string Url, out HttpWebRequest myRequest, out WebResponse myResponse, out StreamReader sr, out string result, out XmlDocument doc, out bool isSE)
        {
            myRequest = (HttpWebRequest)WebRequest.Create(Url);
            myRequest.Method = "GET";
            myRequest.Timeout = 1000000;
            myRequest.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            myRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
            myResponse = myRequest.GetResponse();
            sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();

            Console.WriteLine("==> Making the statistics");
            doc = new XmlDocument();
            doc.LoadXml(result);
            isSE = false;
            makeStats(seNames, doc, rawDataFile, isSE);
            Console.WriteLine("Total publications in SE: " + iCounter.ToString()); 
        }

        private static int makeStats(string seNames, XmlDocument doc, System.IO.StreamWriter rawDataFile, bool isSE)
        {
            foreach (XmlNode row in doc.SelectNodes("//upl-record"))
            {
                isSE = false;

                string authors = "";

                foreach (XmlNode oneTitle in row.SelectNodes("persons/persons_item"))
                {
                    if (authors.Length != 0)
                        authors += ":";
                    string oneAuthor = oneTitle.SelectSingleNode("last").InnerText;

                    if (seNames.Contains(oneAuthor))
                        isSE = true;
                    authors += oneAuthor;

                }

                if (pubIds.ContainsKey(row.SelectSingleNode("pubid").InnerText))
                {
                    isSE = false;
                    System.Diagnostics.Debug.WriteLine("Exists: " + row.SelectSingleNode("pubid").InnerText); 
                    continue;
                }

                pubIds.Add(row.SelectSingleNode("pubid").InnerText, 1);

                string strSource = "";

                try
                {
                    strSource = row.SelectSingleNode("sourcetitle").InnerText;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Problem: " + row.SelectSingleNode("title").InnerText + ": " + row.SelectSingleNode("pubtype/pubtype_item/dcname").InnerText);
                    //System.Diagnostics.Debug.WriteLine("Problem: " + row.SelectSingleNode("sourcetitle"));
                }

                try
                {
                    if (isSE)
                    {
                        rawDataFile.WriteLine(row.SelectSingleNode("title").InnerText + ";" +
                                       row.SelectSingleNode("pubyear").InnerText + ";" +
                                       strSource + ";" +
                                       row.SelectSingleNode("pubtype/pubtype_item/dcname").InnerText + ";" +
                                       authors);
                        iCounter++;

                        Int32 iYear = Int32.Parse(row.SelectSingleNode("pubyear").InnerText.ToString());
                        int iIndexYear = iYear - iStartYear;

                        // here we make the statistics about the publications
                        // to be done in the next version of the code
                        if (row.SelectSingleNode("pubtype/pubtype_item/dcname").InnerText.Contains("Conference"))
                        {
                            statsArray[iConferenceRow, iIndexYear]++;
                        }

                        if (row.SelectSingleNode("pubtype/pubtype_item/dcname").InnerText.Contains("Journal"))
                        {
                            statsArray[iJournalRow, iIndexYear]++;
                        }

                        if (row.SelectSingleNode("pubtype/pubtype_item/dcname").InnerText.Contains("Chapter"))
                        {
                            statsArray[iChapterRow, iIndexYear]++;
                        }
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Problem (P): " + row.SelectSingleNode("title").InnerText);
                }


            }

            
            return iCounter;
        }

        // this method writes the HTML statistics file
        // based on the predefined templates
        public static void writeHTML()
        {
            System.IO.StreamWriter htmlFile = new System.IO.StreamWriter(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\sepub_new.html");
            System.IO.StreamReader footerFile = new System.IO.StreamReader(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\sepub_auto_footer.html");
            System.IO.StreamReader endFile = new System.IO.StreamReader(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\sepub_auto_footer_end.html");
            System.IO.StreamReader headerFile = new System.IO.StreamReader(@"\\sol.ita.chalmers.se\miroslaw\www\www.cse.chalmers.se\sepub\sepub_auto_header.html");
            string line;

            System.Diagnostics.Debug.WriteLine(iCounter); 

            while ((line = headerFile.ReadLine()) != null)
            {
                htmlFile.WriteLine(line);
            }

            htmlFile.WriteLine("['2011', " + statsArray[iJournalRow, 0] + "," + statsArray[iConferenceRow, 0] + ","  + statsArray[iChapterRow, 0] + "],");
            htmlFile.WriteLine("['2012', " + statsArray[iJournalRow, 1] + "," + statsArray[iConferenceRow, 1] + "," + statsArray[iChapterRow, 1] + "],");
            htmlFile.WriteLine("['2013', " + statsArray[iJournalRow, 2] + "," + statsArray[iConferenceRow, 2] + "," + statsArray[iChapterRow, 2] + "],");
            htmlFile.WriteLine("['2014', " + statsArray[iJournalRow, 3] + "," + statsArray[iConferenceRow, 3] + "," + statsArray[iChapterRow, 3] + "],");
            htmlFile.WriteLine("['2015', " + statsArray[iJournalRow, 4] + "," + statsArray[iConferenceRow, 4] + "," + statsArray[iChapterRow, 4] + "],");
            htmlFile.WriteLine("['2016', " + statsArray[iJournalRow, 5] + "," + statsArray[iConferenceRow, 5] + "," + statsArray[iChapterRow, 5] + "],");

            /*  ['2011', 3, 11, 0],
                ['2012', 4, 26, 3],
                ['2013', 11, 42, 0],
                ['2014', 14, 59, 10],
                ['2015', 8, 24, 3]  
             */
            
            while ((line = footerFile.ReadLine()) != null)
            {
                htmlFile.WriteLine(line);
            }

            htmlFile.WriteLine(DateTime.Now.ToShortDateString());

            while ((line = endFile.ReadLine()) != null)
            {
                htmlFile.WriteLine(line);
            }

            htmlFile.Close();
            footerFile.Close();
            endFile.Close();
        }
    }
}
