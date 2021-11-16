using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lotto
{
    class Log
    {
        static public string m_sLogDir = System.Windows.Forms.Application.StartupPath;
        static public string m_sLogFileName = "Lotto.csv";
        static string m_sFileName = "\\Lotto_Rst.csv";
        static object lockObj = new object();
        static StreamWriter m_swLog = null;

        public static bool LoadText(ref List<string[]> szHistoryList, string szFIlePath)
        {
            if (File.Exists(szFIlePath) == false)
                return false;
            char sep = ',';
            int nColCount = 0;
            List<string> szBufferList = new List<string>();

            StreamReader vStreamReader = null;
            try
            {
                vStreamReader = File.OpenText(szFIlePath);
            }
            catch (NotSupportedException e) // 문자의 형태가 잘못되었을 때 이 안으로 들어온다. 
            {
                string s = e.ToString();
                Debug.Assert(false);
            }
            bool bIsEOF = vStreamReader.EndOfStream;
            string szStr = "";
            while (!bIsEOF)
            {
                szStr = vStreamReader.ReadLine();
                szBufferList.Add(szStr);
                bIsEOF = vStreamReader.EndOfStream;
            }
            vStreamReader.Close();
            vStreamReader = null;

            int nRows = szBufferList.Count();
            int nCols = 0;

            foreach (string szBuffer in szBufferList)
            {
                int cnt = GetColCount(szBuffer, sep);
                nCols = Math.Max(nCols, cnt);
            }
            nColCount = nCols;
            if (nRows == 0 || nCols == 0)
            {
                return false;
            }

            szHistoryList.Clear();
            foreach (string szBuffer in szBufferList)
            {
                string[] szParts = szBuffer.Split(sep);
                string[] szToken = new string[nCols];
                int nWordCount = szParts.Count();
                nWordCount = Math.Min(nWordCount, nCols);

                char charCad = '"';
                int nIndex = 0;
                for (int i = 0; i < nWordCount; i++)
                {
                    szToken[nIndex] = szParts[i];
                    szToken[nIndex] = szToken[nIndex].Trim(charCad);

                    nIndex++;
                }
                if (nIndex == 0)
                    continue;
                int nAddCount = nCols - nIndex;
                for (int i = 0; i < nAddCount; i++)
                {
                    szToken[nIndex + i] = "";
                }
                szHistoryList.Add(szToken);
            }
            szBufferList = null;

            GC.Collect();
            return true;
        }
        private static int GetColCount(string szBuffer, char sep)
        {
            string[] words = szBuffer.Split(sep);
            int nWordCount = words.Count();

            int i = nWordCount - 1;
            for (; i >= 0; --i)
            {
                if (!string.IsNullOrEmpty(words[i]))
                    break;
            }
            return (i + 1);
        }
        public static void OpenLog(string fileName)
        {
            try
            {
                if (m_swLog == null)
                {
                    m_swLog = new StreamWriter(fileName, true, System.Text.Encoding.Unicode);
                    m_swLog.AutoFlush = true;
                }
            }
            catch (Exception ex)
            {
                AddLog(ex);
            }
        }
        public static void CloseLog()
        {
            try
            {
                if (m_swLog != null)
                {
                    m_swLog.Close();
                    m_swLog = null;
                }
            }
            catch (Exception ex)
            {
                AddLog(ex);
            }
        }
        private static void WriteLog(string fileName, string[] val)
        {
            try
            {
                string str = "";
                for (int i = 0; i < val.Length; i++)
                {
                    if (i < val.Length - 1)
                        str += val[i] + ",";
                    else
                        str += val[i];
                }
                str += "\r\n";

                m_swLog.Write(str);
            }
            catch (Exception ex)
            {
                AddLog(ex);
            }
        }
        public static void AddLog(string sMemo, int nThreadID = -100)
        {
            try
            {
                if (string.IsNullOrEmpty(sMemo))
                    return;
                if (Directory.Exists(m_sLogDir) == false)
                {
                    if (Directory.Exists(m_sLogDir))
                        Directory.CreateDirectory(m_sLogDir);
                }
                string sLogFolder = m_sLogDir;
                string sLogPath = sLogFolder + m_sFileName;
                string sTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
                string sThID = "";
                if (nThreadID != -100)
                    sThID = "[" + nThreadID.ToString("D6") + "]";
                else
                    sThID = "[" + Thread.CurrentThread.ManagedThreadId.ToString("D6") + "]";

                lock (lockObj)
                {
                    if (Directory.Exists(sLogFolder) == false)
                        Directory.CreateDirectory(sLogFolder);

                    if (m_swLog == null && File.Exists(sLogPath))
                        OpenLog(sLogPath);
                    if (File.Exists(sLogPath) == false)
                    {
                        CloseLog();
                        OpenLog(sLogPath);
                    }
                    WriteLog(sLogPath, new string[] { sTime, sThID, sMemo });
                }
            }
            catch (Exception ex)
            {
                AddLog(ex);
            }
        }
        public static void AddLog(Exception ex)
        {
            StringBuilder s = new StringBuilder(ex.Message);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                s.AppendFormat(":\r\n{0}", ex.Message);
            }
            if (ex.StackTrace != null)
            {
                string[] sa = ex.StackTrace.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s1 in sa)
                {
                    if (!s1.Contains(":line") && !s1.Contains(":줄"))
                        continue;

                    s.AppendFormat("\r\n{0}", s1.Trim());
                    break;
                }
            }
            AddLog(s.ToString());
        }
    }
}
