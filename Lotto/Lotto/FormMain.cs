using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Lotto
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                string sDir = Log.m_sLogDir;
                if (Directory.Exists(sDir) == false)
                    this.Close();
                string sFileName = Log.m_sLogFileName;
                string sLottoPath = sDir + "\\" + sFileName;
                if (File.Exists(sLottoPath) == false)
                    this.Close();
                List<string[]> szHistoryList = new List<string[]>();
                if (Log.LoadText(ref szHistoryList, sLottoPath) == false)
                    return;
                int nTotalCnt = szHistoryList.Count;
                int nLottoCnt = Create_Lotto.m_nLotto;
                Create_Lotto.m_nLottoHistoryCnt = nTotalCnt;
                Create_Lotto.m_ArrLottoHistory = new int[nTotalCnt, nLottoCnt];
                for (int a = 0; a < nTotalCnt; a++)
                {
                    string[] sArrLottoData = szHistoryList[a];
                    for (int b = 0; b < nLottoCnt; b++)
                    {
                        if (sArrLottoData.Length != nLottoCnt ||
                            int.TryParse(sArrLottoData[b], out Create_Lotto.m_ArrLottoHistory[a, b]) == false)
                        {
                            this.Close();
                            break;
                        }
                    }
                }
                lbTitle.Text = nTotalCnt.ToString();

                Create_Lotto.CreateLotto();
                this.Close();
            }
            catch (Exception ex)
            {
                Log.AddLog(ex);
            }
        }
    }
}