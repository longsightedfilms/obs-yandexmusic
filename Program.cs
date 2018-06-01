using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace OBS_YaMusic
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr windowHandle, StringBuilder stringBuilder, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hwnd);

        private static List<IntPtr> windowList;
        private static string _className;
        private static StringBuilder apiResult = new StringBuilder(256); //256 Is max class name length.
        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        private static string oldsongname = "";
        static string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yandex-music.txt");
        private static Dictionary<string, string> browsers = new Dictionary<string, string>
        {
            {"chrome", "Chrome_WidgetWin_1"},
            {"opera", "Chrome_WidgetWin_1"},
            //{"firefox", "MozillaWindowClass"},//Currenly glitching
            {"iexplore", "IEFrame"},
            {"MicrosoftEdgeCP", "TabWindowClass"}
        };
        static string currentbrowser;
        static Boolean reverse;
        static string newsongname;
        // Blacklisting tab names
        static string[] blacklist = {
            "WordPress",
            "Яндекс.Музыка",
            ". Слушать онлайн на Яндекс.Музыке",
            "слушать онлайн на Яндекс.Музыке",
            "радио на любой вкус",
            "радио по жанрам"
        };
        static string[] browserargs = {
            "chrome", "opera", "ie", "edge"
        };
        static Dictionary<string, string> list = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            Console.WriteLine("OBS Yandex Music v.0.1.2 by Maxim Makarov");
            CSVRead();
            if(args.Length == 0) {
                currentbrowser = "chrome";
                reverse = false;
            } else {
                foreach (string arg in args) {
                    if (args.Contains("reverse")){
                        reverse = true;
                    } else {
                        reverse = false;
                    }
                    if (browserargs.Contains(arg)){
                        switch (arg)
                        {
                            case "edge":
                                currentbrowser = "MicrosoftEdgeCP";
                                break;
                            case "ie":
                                currentbrowser = "iexplore";
                                break;
                            default:
                                currentbrowser = arg;
                                break;
                        }
                    } else {
                        currentbrowser = "chrome";
                    }
                }
            }
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Console writes all played songs below:");
            while(true){
                List<IntPtr> BrowserTab = WindowsFinder(browsers[currentbrowser], currentbrowser);
                foreach (IntPtr windowHandle in BrowserTab)
                {
                    int length = GetWindowTextLength(windowHandle);
                    StringBuilder sb = new StringBuilder(length + 1);
                    GetWindowText(windowHandle, sb, sb.Capacity);
                    if (!String.IsNullOrEmpty(sb.ToString()))
                    {
                        // Reverse string before splitting. It helps to split fine
                        string[] appname = Reverse(sb.ToString()).Split(" - ", 2);
                        List<string> windowname = new List<string>();
                        for (int i = 0; i < appname.Length; i++) {
                            windowname.Add(Reverse(appname[i]));
                        }
                        string[] tokens = windowname.ToArray();
                        if (currentbrowser == "MicrosoftEdgeCP"){ // Firefox currently unused
                            tokens = tokens[0].ToString().Split(" — ");
                        } else {
                            tokens = tokens[1].ToString().Split(" — ");
                        }
                        if (tokens.Length < 2){
                            break;
                        } else {
                            bool isValid = blacklist.Any(s => tokens[1].Contains(s));
                            if (!isValid){
                                if (reverse){
                                    newsongname = Shorten(tokens[1]) + " - " + tokens[0];
                                } else {
                                    newsongname = tokens[0] + " - " + Shorten(tokens[1]);
                                }
                                if (!newsongname.Equals(oldsongname)) {
                                    // Reducing I/O write to disk
                                    oldsongname = newsongname;
                                    Console.WriteLine(oldsongname);
                                    System.IO.File.WriteAllText(destPath, oldsongname);
                                }
                            }
                        }
                        break;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        // Shorten some very long artist names
        public static string Shorten(string artist)
        {
            if(list.ContainsKey(artist)){
                return list[artist];
            } else{
                return artist;
            }
        }
        public static void CSVRead() {
            using (var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shorten.csv")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    list.Add(values[0], values[1]);
                }
            }
        }
        private static List<IntPtr> WindowsFinder(string className, string process)
        {
            _className = className;
            windowList = new List<IntPtr>();

            Process[] chromeList = Process.GetProcessesByName(process);

            if (chromeList.Length > 0)
            {
                foreach (Process chrome in chromeList)
                {
                    if (chrome.MainWindowHandle != IntPtr.Zero)
                    {
                        foreach (ProcessThread thread in chrome.Threads)
                        {
                            EnumThreadWindows((uint)thread.Id, new EnumThreadDelegate(EnumThreadCallback), IntPtr.Zero);
                        }
                    }
                }
            }

            return windowList;
        }

        static bool EnumThreadCallback(IntPtr hWnd, IntPtr lParam)
        {
            if (GetClassName(hWnd, apiResult, apiResult.Capacity) != 0)
            {
                if (string.CompareOrdinal(apiResult.ToString(), _className) == 0)
                {
                    windowList.Add(hWnd);
                }
            }
            return true;
        }
    }
}