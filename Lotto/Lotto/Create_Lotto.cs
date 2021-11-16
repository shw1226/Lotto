using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace Lotto
{
    class Create_Lotto
    {
        public static int m_nLotto = 6;
        public static int[,] m_ArrLottoHistory;
        public static int m_nLottoHistoryCnt = 0;
        static int m_nLottoMax = 45;
        static int m_nLottoOK = 2;
        static int m_nMin = 5;
        static int m_nSleep = 1000;
        static int m_nSleep2 = 100;
        static float m_fAddV = 0.000001f;
        static string m_sAdd = "0.000000";
        public static void CreateLotto()
        {
            try
            {
                Log.AddLog("Start");
                string sPath = string.Format("{0}\\5.txt", Log.m_sLogDir);
                if (File.Exists(sPath))
                {
                    Type5();
                    File.Delete(sPath);
                }
                else
                {
                    Type1();
                    Type2();
                    Type3();
                    Type4();

                    string sLottoPath = Log.m_sLogDir + "\\" + Log.m_sLogFileName;
                    if (File.Exists(sLottoPath))
                        File.Copy(sLottoPath, sPath);
                    Type5();
                }
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static bool CheckItem(int nTotal, int[] nArr, int nValue)
        {
            try
            {
                if (nValue <= 0 || nValue > m_nLottoMax)
                    return false;
                for (int a = 0; a < nTotal; a++)
                {
                    if (nValue == nArr[a])
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
            return false;
        }
        private static int GetTypeValue()
        {
            try
            {
                Thread.Sleep(m_nSleep);
                Random radData = new Random();
                if (radData.Next(2) == 1)
                    return GetHisValue();
                else
                    return GetValue();
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
            return 0;
        }
        private static int GetValue()
        {
            try
            {
                Thread.Sleep(m_nSleep);
                Random radData = new Random();
                int nValue = radData.Next(m_nLottoMax + 1);
                if (nValue < 1) nValue = 1;
                if (nValue > m_nLottoMax) nValue = m_nLottoMax;
                return nValue;
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
            return 0;
        }
        private static int GetHisValue()
        {
            try
            {
                Thread.Sleep(m_nSleep);
                Random radData = new Random();
                int nValue1 = radData.Next(m_nLottoHistoryCnt);
                if (nValue1 < 0) nValue1 = 0;
                if (nValue1 >= m_nLottoHistoryCnt) nValue1 = m_nLottoHistoryCnt - 1;

                Thread.Sleep(m_nSleep);
                Random radData2 = new Random();
                int nValue2 = radData2.Next(m_nLotto);
                if (nValue2 < 0) nValue2 = 0;
                if (nValue2 >= m_nLotto) nValue2 = m_nLotto - 1;

                return m_ArrLottoHistory[nValue1, nValue2];
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
            return 0;
        }
        private static bool AliveFunc(string sName, ref int[] nArrRst, ref DateTime dt, ref float cnt)
        {
            try
            {
                Thread.Sleep(m_nSleep2);
                string sData = "";
                for (int a = 0; a < m_nLotto; a++)
                {
                    sData += nArrRst[a].ToString() + ",";
                    nArrRst[a] = 0;
                }
                TimeSpan ti = DateTime.Now - dt;
                if (ti.Minutes >= m_nMin)
                {
                    dt = DateTime.Now;
                    string sMsg = string.Format("[Alive] AliveFunc_1 {0} {1} {2}", sName, cnt.ToString(), sData);
                    Log.AddLog(sMsg);
                }
                cnt += m_fAddV;
                if (cnt == float.MaxValue - 1)
                {
                    string sMsg = string.Format("[Alive] AliveFunc_2 {0} {1} {2}", sName, cnt.ToString(), sData);
                    cnt = 0;
                    Log.AddLog(sMsg);
                }
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
            return true;
        }
        private static void EndWrite(string sName, int[] nArrRst)
        {
            try
            {
                string sMsg = "";
                for (int b = 0; b < m_nLotto; b++)
                    sMsg += nArrRst[b] + ",";
                Log.AddLog("End " + sName + " : " + sMsg);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static float GetGradient(List<float> listTotal)
        {
            float fRst = 0;
            try
            {
                if (listTotal.Count == 0)
                    return fRst;

                // 1: -[Sig]xy + a[Sig]x^2 + b[Sig]x = 0
                // 2: -[Sig]y + a[Sig]x + bn = 0
                // n = nTotalCnt
                // a, b를 계산
                float fSigmaXY = 0;
                float fSigmaXSquare = 0;
                float fSigmaX = 0;
                float fSigmaY = 0;
                for(int nX = 0; nX < listTotal.Count; nX++)
                {
                    float fX = (float)nX;
                    float fY = (float)listTotal[nX] * (1.0f / m_fAddV);
                    if (fX < 0 || fY < 0)
                        return fRst;
                    fSigmaXY = fSigmaXY + (fX * fY);
                    fSigmaXSquare = fSigmaXSquare + (fX * fX);
                    fSigmaX = fSigmaX + fX;
                    fSigmaY = fSigmaY + fY;
                }
                PointF poAB = new PointF();
                float fValue = (fSigmaX * fSigmaX) - (fSigmaXSquare * listTotal.Count);
                float fValue1 = (fSigmaX * fSigmaXY) - (fSigmaXSquare * fSigmaY);
                if (fValue != 0 && fValue1 != 0)
                    poAB.Y = fValue1 / fValue;
                else
                    poAB.Y = 0;
                fValue = fSigmaXSquare;
                fValue1 = fSigmaXY - (fSigmaX * poAB.Y);
                if (fValue != 0 && fValue1 != 0)
                    poAB.X = fValue1 / fValue;
                else
                    poAB.X = 0;

                fRst = (poAB.X * (listTotal.Count + 1) + poAB.Y) * m_fAddV;
            }
            catch (System.Exception ex)
            {
                Log.AddLog(ex);
            }
            return fRst;
        }
        private static void Type1()
        {
            try
            {
                string sName = "[Type1]";
                Log.AddLog("Start " + sName);
                int[] nArrRst = new int[m_nLotto];
                DateTime dt = DateTime.Now;
                float cnt = 0;
                while (true)
                {
                    bool bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                    for (int a = 0; a < m_nLotto; a++)
                    {
                        nArrRst[a] = GetHisValue();
                        bOK = CheckItem(a, nArrRst, nArrRst[a]);
                        if (bOK == false)
                            break;
                    }
                    if (bOK) break;
                }

                EndWrite(sName, nArrRst);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static void Type2()
        {
            try
            {
                string sName = "[Type2]";
                Log.AddLog("Start " + sName);
                int[] nArrRst = new int[m_nLotto];
                DateTime dt = DateTime.Now;
                float cnt = 0;
                while (true)
                {
                    bool bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                    for (int a = 0; a < m_nLotto; a++)
                    {
                        nArrRst[a] = GetValue();
                        bOK = CheckItem(a, nArrRst, nArrRst[a]);
                        if (bOK == false)
                            break;
                    }
                    if (bOK) break;
                }

                EndWrite(sName, nArrRst);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static void Type3()
        {
            try
            {
                string sName = "[Type3]";
                Log.AddLog("Start " + sName);
                int[] nArrRst = new int[m_nLotto];
                DateTime dt = DateTime.Now;
                float cnt = 0;
                while (true)
                {
                    bool bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                    for (int a = 0; a < m_nLotto; a++)
                    {
                        nArrRst[a] = GetTypeValue();
                        bOK = CheckItem(a, nArrRst, nArrRst[a]);
                        if (bOK == false)
                            break;
                    }
                    if (bOK) break;
                }

                EndWrite(sName, nArrRst);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static void Type4()
        {
            try
            {
                string sName = "[Type4]";
                Log.AddLog("Start " + sName);
                int[] nArrRst = new int[m_nLotto];
                DateTime dt = DateTime.Now;
                DateTime dt2 = DateTime.Now;
                float cnt = 0;
                string sMsg = "";
                while (true)
                {
                    bool bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                    for (int a = 0; a < m_nLotto; a++)
                    {
                        List<float> listTotal = new List<float>();
                        for (int b = 0; b < m_nLottoHistoryCnt; b++)
                        {
                            float total = 0;
                            while (true)
                            {
                                int nResult = GetValue();
                                total += m_fAddV;
                                if (nResult == m_ArrLottoHistory[b, a])
                                    break;
                                TimeSpan ti = DateTime.Now - dt2;
                                if (total == float.MaxValue - 1 || ti.Minutes >= m_nMin)
                                {
                                    sMsg = string.Format("[Alive] {0}_1 {1} {2} {3} {4} {5}", sName, a.ToString(), b.ToString(), total.ToString(m_sAdd), listTotal.Count, m_ArrLottoHistory[b, a]);
                                    Log.AddLog(sMsg);
                                    if (total == float.MaxValue - 1)
                                        total = 0;
                                    if (ti.Minutes >= m_nMin)
                                        dt2 = DateTime.Now;
                                }
                            }
                            listTotal.Add(total);
                        }
                        if (listTotal.Count != m_nLottoHistoryCnt)
                        {
                            bOK = false;
                            break;
                        }
                        while (true)
                        {
                            float fTotal = GetGradient(listTotal);
                            if (fTotal <= 0) fTotal = m_fAddV;
                            for (float b = 0; b < fTotal; b += m_fAddV)
                                nArrRst[a] = GetValue();
                            bOK = CheckItem(a, nArrRst, nArrRst[a]);
                            if (bOK) break;

                            TimeSpan ti = DateTime.Now - dt2;
                            if (ti.Minutes >= m_nMin)
                            {
                                sMsg = string.Format("[Alive] {0}_2 {1} {2} {3}", sName, a.ToString(), fTotal.ToString(m_sAdd), listTotal.Count);
                                Log.AddLog(sMsg);
                                if (ti.Minutes >= m_nMin)
                                    dt2 = DateTime.Now;
                            }
                        }
                    }
                    if (bOK) break;
                }

                EndWrite(sName, nArrRst);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
        private static void Type5()
        {
            try
            {
                string sName = "[Type5]";
                Log.AddLog("Start " + sName);
                int[] nArrRst = new int[m_nLotto];
                DateTime dt = DateTime.Now;
                DateTime dt2 = DateTime.Now;
                string sMsg = "";
                while (true)
                {
                    bool bOK = false;
                    List<float> listTotal = new List<float>();
                    for (int a = 0; a < m_nLottoHistoryCnt; a++)
                    {
                        float cnt = 0;
                        while (true)
                        {
                            bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                            for (int b = 0; b < m_nLotto; b++)
                            {
                                nArrRst[b] = GetTypeValue();
                                bOK = CheckItem(a, nArrRst, nArrRst[b]);
                                if (bOK == false)
                                    break;
                            }
                            int nOKCnt = 0;
                            if (bOK)
                            {
                                for (int b = 0; b < m_nLotto; b++)
                                {
                                    for (int c = 0; c < m_nLotto; c++)
                                    {
                                        if (nArrRst[b] == m_ArrLottoHistory[a, c])
                                        {
                                            nOKCnt++;
                                            break;
                                        }
                                    }
                                }
                                if (nOKCnt >= m_nLottoOK)
                                {
                                    listTotal.Add(cnt);
                                    break;
                                }
                            }
                            TimeSpan ti = DateTime.Now - dt2;
                            if (ti.Minutes >= m_nMin)
                            {
                                sMsg = string.Format("[Alive] {0}_1 {1} {2} {3}", sName, a.ToString(), cnt.ToString(m_sAdd), listTotal.Count);
                                Log.AddLog(sMsg);
                                if (ti.Minutes >= m_nMin)
                                    dt2 = DateTime.Now;
                            }
                        }
                    }
                    float fTotal = GetGradient(listTotal);
                    for (float b = 0; b < fTotal; b += m_fAddV)
                    {
                        float cnt = 0;
                        while (true)
                        {
                            bOK = AliveFunc(sName, ref nArrRst, ref dt, ref cnt);
                            for (int a = 0; a < m_nLotto; a++)
                            {
                                nArrRst[a] = GetValue();
                                bOK = CheckItem(a, nArrRst, nArrRst[a]);
                                if (bOK == false)
                                    break;
                            }
                            if (bOK) break;

                            TimeSpan ti = DateTime.Now - dt2;
                            if (ti.Minutes >= m_nMin)
                            {
                                sMsg = string.Format("[Alive] {0}_2 {1} {2} {3}", sName, b.ToString(), fTotal.ToString(m_sAdd), listTotal.Count);
                                Log.AddLog(sMsg);
                                if (ti.Minutes >= m_nMin)
                                    dt2 = DateTime.Now;
                            }
                        }
                    }
                    break;
                }
                EndWrite(sName, nArrRst);
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
    }
}
