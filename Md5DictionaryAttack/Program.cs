using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Md5DictionaryAttack
{
    class Program
    {
        static List<Md5Pair> wordlist;
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            if (!File.Exists("wordlist.txt"))
            {
                File.Create("wordlist.txt");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("Wordlist is being created! Please re-run your command."));
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
            DateTime start = DateTime.Now;
            wordlist = loadWordList("wordlist.txt").OrderBy(md5 => md5.Md5).Distinct(new DistinctPairComparer()).ToList();
            DateTime end = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Wordlist loaded after: " + (end - start).TotalSeconds + " secs");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (args[0] == "-f")
            {
                try
                {
                    string md5 = args[1];
                    printSearchResult(md5);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else if (args[0] == "-fl")
            {
                string md5line = args[1];
                string[] md5s = md5line.Split(',');

                foreach (string md5 in md5s)
                {
                    printSearchResult(md5);
                }
            }else if (args[0] == "-e")
            {
                string word = args[1];
                Console.WriteLine(md5hash(word));

            }else if (args[0] == "-s")
            {
                Console.WriteLine("Word Count: " + wordlist.Count);
            }else if(args[0] == "-aw")
            {
                string path = args[1];
                if (!File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("File Not Found: " + path);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }
                string[] words = System.IO.File.ReadAllLines(string.Format(@"{0}", path));
                addToWordList(words);

            }
            if (args[0] == "-a")
            {
                try
                {
                    string []words = { args[1] };
                    addToWordList(words);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            else if (args[0] == "-al")
            {
                string wordline = args[1];
                string[] words = wordline.Split(',');

                addToWordList(words);
            }
            else if(args[0] == "-au")
            {
                string url = args[1];

                string contents;
                using (var wc = new System.Net.WebClient())
                {
                    wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                    contents = wc.DownloadString(url);
                }
                string[] words = contents.Split('\n');
                addToWordList(words);



            }
            else if(args[0] == "-h")
            {
                Console.WriteLine("md5 -f  <md5>                                 \t|Find Pass from wordlist          \n" +
                                 "md5 -fl <md5 list |,|>                          |Find list of pass from wordlist  \n" +
                                 "md5 -e  <md5>                                   |Returns Hashed Word              \n" +
                                 "md5 -a  <word>                                  |Add word in wordlist             \n" +
                                 "md5 -al <word list |,|>                         |Add words to list from input                                 \n" +
                                 "md5 -aw <filepath>                              |Add words to list from file        \n" +
                                 "md5 -au <url>                                   |Add words to list from url        \n" +
                                 "md5 -s                                          |Status of md5 Dictionary         \n" +
                                 "md5 -h                                          |Help|\n");
            }



        }

        static void addToWordList(string []words)
        {
            DateTime start = DateTime.Now;
            int wordlength = words.Length;
            int counter = 0;

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string word in words)
            {
                string hash = md5hash(word.Trim());

                // Using Streamwriter is faster than AppendAllText
                //File.AppendAllText("wordlist.txt", String.Format("{0},{1}", hash, word.Trim()) + Environment.NewLine);
                appendToWordList(String.Format("{0},{1}", hash, word.Trim()) + Environment.NewLine);
                counter++;
                Console.WriteLine(String.Format("({0}%) {1}/{2} Added: {3} - {4}", (((double)counter / wordlength)* 100).ToString("#.##"), counter, wordlength, hash, word));
            }
            DateTime end = DateTime.Now;
            Console.WriteLine("Time Elapsed: " + (end - start).TotalSeconds  + " secs");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public class Md5Pair
        {
            public string Md5 { get; set; }
            public string word { get; set; }
        }


        static string md5hash(string word)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(word);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
        /*
        // Function down below is way much faster than this.
        static List<Md5Pair> loadWordList(string path)
        {
            List<Md5Pair> list = new List<Md5Pair>();
            string[] lines = System.IO.File.ReadAllLines(string.Format(@"{0}", path));
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    Md5Pair pair = new Md5Pair();
                    pair.Md5 = parts[0];
                    pair.word = parts[1];
                    list.Add(pair);
                }
            return list;
        }
        */

        static List<Md5Pair> loadWordList(string path)
        {
            List<Md5Pair> list = new List<Md5Pair>();
            using (FileStream fs = File.OpenRead(path))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] parts = s.Split(',');
                    Md5Pair pair = new Md5Pair();
                    pair.Md5 = parts[0];
                    pair.word = parts[1];
                    list.Add(pair);
                }
            }

            return list;
        }

        static void appendToWordList(string text)
        {
            using (StreamWriter stream = new StreamWriter("wordlist.txt", true))
            {
                stream.WriteLine(text);
            }
        }




        static Md5Pair find(string md5)
        {
            return wordlist.Find(r => String.Equals(r.Md5, md5, StringComparison.OrdinalIgnoreCase));
        }

        static void printSearchResult(string md5)
        {
            DateTime start = DateTime.Now;
            Md5Pair pair = find(md5);
            if (pair == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not Found: " + md5);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("Found: {0} = ({1})", md5, pair.word));

            }

            DateTime end = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Time Elapsed: " + (end - start).TotalSeconds + " secs");
            Console.ForegroundColor = ConsoleColor.Gray;


        }

        class DistinctPairComparer : IEqualityComparer<Md5Pair>
        {
            public bool Equals(Md5Pair x, Md5Pair y)
            {
                return x.Md5 == y.Md5 &&
                    x.word == y.word;
            }

            public int GetHashCode(Md5Pair obj)
            {
                return obj.Md5.GetHashCode() ^
                    obj.word.GetHashCode();
            }
        }
    }
}

