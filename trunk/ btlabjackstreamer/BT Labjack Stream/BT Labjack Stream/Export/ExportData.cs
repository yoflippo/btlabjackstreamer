using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;

namespace BT_Labjack_Stream
{
    class ExportData : FileHandling
    {
        #region MEMBERS
        private StreamWriter sw;
        private const string tab = "\t";
        private const string dubtab = tab + tab;
        private const string nul = "0.0";
        private const string headerPrefix = "**";
        private const string space = " ";
        private string _titel = null;
        private List<double>[] dataChannels = null;   
   
        //application specific
        public short AantalGeselecteerdeKanalen = 0;
        public bool[] IsHetKanaalGeselecteerd = null;
        public int SampleFrequentie = 0;
        public string LabjackID = null;
        #endregion

        #region CONSTRUCTOR
        public ExportData(string titel)
        {
            _titel = titel;
        }
        #endregion

        #region DATA
        public void Export(List<double>[] data)
        {
            if (data != null && FileLocationAndFileName != null)
            {
                dataChannels = data;
                makeHeader();
                writeData();

                System.Diagnostics.Process.Start("notepad.exe", @FileLocationAndFileName); //open txt-file
            }
            else
            {
                MessageBox.Show("Geen data!","Melding",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
        }


        private void makeHeader()
        {
            if (sw == null)
                sw = new StreamWriter(FileLocationAndFileName);
            sw.WriteLine(headerPrefix + _titel + " " + DateTime.Now.Year);
            sw.WriteLine(headerPrefix + "Datum: " + DateTime.Now.ToLocalTime());
            sw.WriteLine(headerPrefix + "SampleFrequentie: " + SampleFrequentie.ToString() + " [Hz]");
            sw.WriteLine(headerPrefix + "Aantal kanalen: " + AantalGeselecteerdeKanalen.ToString());
            sw.WriteLine(headerPrefix + "Labjack " + LabjackID);
            sw.WriteLine(); //lege regel

            //bouw string kanalen
            StringBuilder sb = new StringBuilder();
            sb.Append("Nummer" + tab);
            for (int i = 0; i < IsHetKanaalGeselecteerd.Length; i++)
			{
                if(IsHetKanaalGeselecteerd[i])
			        sb.Append("FIO"+ i.ToString() + tab);
			}
            sw.WriteLine(sb.ToString());

        }

        /// <summary>
        /// Name: writeData
        /// </summary>
        private void writeData()
        {
            for (int nummer = 0; nummer < dataChannels[0].Count; nummer++) //door List lopen
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(nummer.ToString() + tab);
                for (int kanaal = 0; kanaal < IsHetKanaalGeselecteerd.Length; kanaal++) //door kanalen lopen
                {
                    if (IsHetKanaalGeselecteerd[kanaal] && kanaal < AantalGeselecteerdeKanalen) //testen op kanaal is geselecteerd
                    {
                        double temp = Math.Round(dataChannels[kanaal][nummer], 5); //10*(1/(2^16)) = 0.00000#####
                        sb.Append(temp.ToString() + tab);
                    }
                }
                sw.WriteLine(sb.ToString()); //schrijf
                
            }
            sw.Close();
        }

        #endregion

        ///Einde klasse
    }
}
