using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using OpenNI;
using BTKinect.DataType;
using BTKinect.DataIO;
using System.Windows.Forms;

namespace BTKinect
{
    class KinectExportData : FileHandling
    {
        #region MEMBERS
        private KinectData kd;
        private KinectBasicCalculations kbc;
        private StreamWriter sw;
        private string _disk = "C:\\";
        private bool _blWithMarkerData = false;
        private List<string> _lstDataString;
        private int _roundingNumberOfDecimals = 0;
        const string tab = " \t ", nul = "0.0", dubtab = tab + tab;
        #endregion

        #region CONSTRUCTOR
        public KinectExportData() { }
        public KinectExportData(KinectData k)
        {
            kd = k;
            _lstDataString = new List<string>(kd.DataAndTime.Count); //make a string list
        }
        public KinectExportData(KinectData k, int RoundingData)
        {
            kd = k;
            _lstDataString = new List<string>(kd.DataAndTime.Count); //make a string list
            _roundingNumberOfDecimals = RoundingData;
        }
        public KinectExportData(KinectData k, KinectBasicCalculations kb)
        {
            kd = k;
            _lstDataString = new List<string>(kd.DataAndTime.Count); //make a string list
            kbc = kb;
        }
        public KinectExportData(KinectData k, KinectBasicCalculations kb, int RoundingData)
        {
            kd = k;
            _lstDataString = new List<string>(kd.DataAndTime.Count); //make a string list
            kbc = kb;
            _roundingNumberOfDecimals = RoundingData;
        }
        #endregion

        #region RAW DATA
        public void ExportRawData(bool WithMarkerData)
        {
            _blWithMarkerData = WithMarkerData;
            if (FileFound)
            {
                setupFileNameLocationAndStreamWriter();
                writeHeaderRawData();
                saveData();
                //open txt-file
                System.Diagnostics.Process.Start("notepad.exe", @FileLocationAndFileName);
            }
            else
                MessageBox.Show("Geen bestand gevonden of aangemaakt", "Melding", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void writeHeaderRawData()
        {
            //writing header
            sw.WriteLine("**BTKinect " + DateTime.Now.Year.ToString());
            sw.WriteLine("**Ruwe Data");
            if (_blWithMarkerData)
                sw.WriteLine(StringHeaderRaw);
            else
                sw.WriteLine(StringHeaderRawMarker);

            sw.WriteLine("**");
        }
        private void setupFileNameLocationAndStreamWriter()
        {
            if (FileLocation == "" || FileLocation == null)
                FileLocationAndFileName = _disk + "BTKinect_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();

                sw = new StreamWriter(FileLocationAndFileName);
        }
        private void saveData()
        {
            int length = kd.DataAndTime.Count;

            string x, y, z;
            for (int i = 0; i < length; i++)
            {
                List<Dictionary<SkeletonJoint, KinectPoint3D>> d = kd.Data;
                StringBuilder mainString = new StringBuilder();

                //Time
                mainString.Append(kd.Time[i].ToString() + tab);

                //marker
                if (_blWithMarkerData)
                    mainString.Append(kd.Marker[i].ToString() + tab);


                foreach (KeyValuePair<SkeletonJoint, KinectPoint3D> kvp in d[i])
                {
                    kvp.Value.RoundMe(_roundingNumberOfDecimals);
                    if (kvp.Value.X == 0)
                        x = nul;
                    else
                        x = kvp.Value.X.ToString();

                    if (kvp.Value.Y == 0)
                        y = nul;
                    else
                        y = kvp.Value.Y.ToString();

                    if (kvp.Value.Z == 0)
                        z = nul;
                    else
                        z = kvp.Value.Z.ToString();
                    mainString.Append(x + tab + y + tab + z + tab);
                }

                //save and export converted data
                _lstDataString.Add(mainString.ToString());
                kd.DataInStringForm = _lstDataString;
                sw.WriteLine(mainString);
            }
            //close streamwriter
            sw.Close();
            kd.DataInStringForm = _lstDataString;
        }
        public KinectData MakeTxtFromRawData(KinectData kinectData)
        {
            List<string> tmpListString = new List<string>(1800);
            List<Dictionary<SkeletonJoint, KinectPoint3D>> d = kinectData.Data;

            //writing header
            tmpListString.Add("**BTKinect " + DateTime.Now.Year.ToString());
            tmpListString.Add("**");
            if (kinectData.WitMarkerData)
                tmpListString.Add("**Tijd \t Marker \t HoofdX \t HoofdY \t HoofdZ \t NekX \t NekY \t NekZ \t LinkerSchouderX \t LinkerSchouderY \t LinkerSchouderZ \t LinkerElleboogX \t LinkerElleboogY \t LinkerElleboogZ \t LinkerHandX \t LinkerHandY \t LinkerHandZ \t RechterSchouderX \t RechterSchouderY \t RechterSchouderZ \t RechterElleboogX \t RechterElleboogY \t RechterElleboogZ \t RechterHandX \t RechterHandY \t RechterHandZ \t TorsoX \t TorsoY \t TorsoZ \t LinkerHeupX \t LinkerHeupY \t LinkerHeupZ \t LinkerKnieY \t LinkerKnieY \t LinkerKnieZ \t LinkerVoetX \t LinkerVoetY \t LinkerVoetZ \t RechterHeupX \t RechterHeupY \t RechterHeupZ \t RechterKnieX \t RechterKnieY \t RechterKnieZ \t RechterVoetX \t RechterVoetY \t RechterVoetZ");
            else
                tmpListString.Add("**Tijd \t HoofdX \t HoofdY \t HoofdZ \t NekX \t NekY \t NekZ \t LinkerSchouderX \t LinkerSchouderY \t LinkerSchouderZ \t LinkerElleboogX \t LinkerElleboogY \t LinkerElleboogZ \t LinkerHandX \t LinkerHandY \t LinkerHandZ \t RechterSchouderX \t RechterSchouderY \t RechterSchouderZ \t RechterElleboogX \t RechterElleboogY \t RechterElleboogZ \t RechterHandX \t RechterHandY \t RechterHandZ \t TorsoX \t TorsoY \t TorsoZ \t LinkerHeupX \t LinkerHeupY \t LinkerHeupZ \t LinkerKnieY \t LinkerKnieY \t LinkerKnieZ \t LinkerVoetX \t LinkerVoetY \t LinkerVoetZ \t RechterHeupX \t RechterHeupY \t RechterHeupZ \t RechterKnieX \t RechterKnieY \t RechterKnieZ \t RechterVoetX \t RechterVoetY \t RechterVoetZ");
            tmpListString.Add("**");

            //writing data to string
            int length = d.Count;
            string mainString = null, x, y, z;
            for (int i = 0; i < length; i++)
            {
                //time
                mainString = kinectData.Time[i].ToString() + tab;

                //marker
                if (kinectData.WitMarkerData)
                    mainString += kinectData.Marker[i].ToString() + tab;

                //data, make a long string of data
                foreach (KeyValuePair<SkeletonJoint, KinectPoint3D> kvp in d[i])
                {
                    kvp.Value.RoundMe(_roundingNumberOfDecimals);
                    if (kvp.Value.X == 0)
                        x = nul;
                    else
                        x = kvp.Value.X.ToString();

                    if (kvp.Value.Y == 0)
                        y = nul;
                    else
                        y = kvp.Value.Y.ToString();

                    if (kvp.Value.Z == 0)
                        z = nul;
                    else
                        z = kvp.Value.Z.ToString();
                    mainString += x + dubtab + y + dubtab + z + dubtab;
                }

                //save converted data
                tmpListString.Add(mainString);
            }
            //save it in KinectData
            kinectData.DataInStringForm = tmpListString;
            return kinectData;
        }
        #endregion

        #region OPTIMISE DATA
        public void ExportOptimisedData(bool WithMarkerData)
        {
            _blWithMarkerData = WithMarkerData;
            setupFileNameLocationAndStreamWriter();
            writeHeaderOptimisedData();
            saveOptimisedData();
            //open txt-file
            System.Diagnostics.Process.Start("notepad.exe", @FileLocationAndFileName);
        }
        private void saveOptimisedData()
        {
            //temp var
            int length = kd.MaxLengthOptimisedDataset;

            //convert optimised data to string 
            for (int i = 0; i < length; i++)
            {
                StringBuilder mainString = new StringBuilder();

                //write time
                mainString.Append(kd.Time[i].ToString() + tab);

                //write marker
                if (_blWithMarkerData)
                    mainString.Append(kd.Marker[i].ToString() + tab);

                //convert data to string
                foreach (var item in kd.SkeletonJointsToBeOptimised) //iterate through list with optimised points
                {
                    switch (item)
                    {
                        case SkeletonJoint.LeftElbow:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedElbowLeft[i]) + tab);
                            break;
                        case SkeletonJoint.LeftKnee:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedKneeLeft[i]) + tab);
                            break;
                        case SkeletonJoint.RightElbow:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedElbowRight[i]) + tab);
                            break;
                        case SkeletonJoint.RightKnee:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedKneeRight[i]) + tab);
                            break;
                        case SkeletonJoint.RightShoulder:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedShoulderRight[i]) + tab);
                            break;
                        case SkeletonJoint.LeftShoulder:
                            mainString.Append(walkThroughKinectPoint3DArray(kd.OptimisedShoulderLeft[i]) + tab);
                            break;
                        default:
                            break;
                    }
                }

                //save and export converted data
                _lstDataString.Add(mainString.ToString());
                sw.WriteLine(mainString);
            }
            //close streamwriter
            kd.DataInStringForm = _lstDataString;
            sw.Close();
        }
        private string walkThroughKinectPoint3DArray(KinectPoint3D[] pnts)
        {
            string tmp = null;
            //make string from every KinectPoint3D
            foreach (var item in pnts)
            {
                tmp += item.ToString() + tab;
            }
            return tmp;
        }
        private void writeHeaderOptimisedData()
        {
            string tmp = null;

            //writing header
            sw.WriteLine("**BTKinect " + DateTime.Now.Year.ToString());
            sw.WriteLine("**Geoptimaliseerde data");

            //string them up
            foreach (var item in kd.SkeletonJointsToBeOptimised)
            {
                string t = item.ToString();
                tmp += t + tab;
            }

            //actually write headerline
            if (_blWithMarkerData)
                sw.WriteLine("**Tijd \t Marker \t" + tmp);
            else
                sw.WriteLine("**Tijd \t" + tmp);

            sw.WriteLine("**");
        }
        public KinectData MakeTxtFromOptimisedData(KinectData Kd)
        {
            _lstDataString = new List<string>(1800);
            string tmp = null;

            //writing header
            _lstDataString.Add("**BTKinect " + DateTime.Now.Year.ToString() + "Optimised data");

            //string them up
            foreach (var item in Kd.SkeletonJointsToBeOptimised)
            {
                string t = item.ToString();
                tmp += t + tab;
            }

            //actually write headerline
            if (Kd.WitMarkerData)
                _lstDataString.Add("**Tijd \t Marker \t" + tmp);
            else
                _lstDataString.Add("**Tijd \t" + tmp);

            _lstDataString.Add("**");

            //convert data to string
            //temp var
            string mainString;
            int length = Kd.MaxLengthOptimisedDataset;

            //convert optimised data to string 
            for (int i = 0; i < length; i++)
            {
                mainString = null;
                //write time
                mainString = Kd.Time[i].ToString() + tab;

                //write marker
                if (Kd.WitMarkerData)
                    mainString += Kd.Marker[i].ToString() + tab;

                //convert data to string
                foreach (var item in Kd.SkeletonJointsToBeOptimised) //iterate through list with optimised points
                {
                    switch (item)
                    {
                        case SkeletonJoint.LeftElbow:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedElbowLeft[i]) + tab;
                            break;
                        case SkeletonJoint.LeftKnee:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedKneeLeft[i]) + tab;
                            break;
                        case SkeletonJoint.RightElbow:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedElbowRight[i]) + tab;
                            break;
                        case SkeletonJoint.RightKnee:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedKneeRight[i]) + tab;
                            break;
                        case SkeletonJoint.RightShoulder:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedShoulderRight[i]) + tab;
                            break;
                        case SkeletonJoint.LeftShoulder:
                            mainString += walkThroughKinectPoint3DArray(Kd.OptimisedShoulderLeft[i]) + tab;
                            break;
                        default:
                            break;
                    }
                }
                //save converted data
                _lstDataString.Add(mainString);
            }
            Kd.DataInStringForm = _lstDataString;
            return Kd;
        }
        #endregion

        #region SELECTED DATA
        public void ExportSelectedData(bool WithMarkerData)
        {
            _blWithMarkerData = WithMarkerData;
            if (FileFound)
            {
                setupFileNameLocationAndStreamWriter();
                int lengthData = kd.Data.Count;

                //write header
                sw.WriteLine("**BTKinect " + DateTime.Now.Year.ToString());
                sw.WriteLine("**Geselecteerde data (posities t.o.v. de Kinect) in millimeters");
                sw.WriteLine("**Interpretatie kolommen: Tijd kolomnaam1[X-coordinaat] kolomnaam1[Y-coordinaat] kolomnaam1[Z-coordinaat] kolomnaam2[X-coordinaat] enzv.");
                //build column names
                StringBuilder strb = new StringBuilder();
                strb.Append("**tijd[ms]" + tab + tab);
                foreach (var item in kd.SkeletonJointsToBeOptimised)
                {
                    strb.Append(kd.getJointsInStringForm(item));
                }
                //write column names
                sw.WriteLine(strb.ToString());

                //select data
                foreach (var item in kd.SkeletonJointsToBeOptimised)
                {
                    KinectSelectDesiredData ksd = new KinectSelectDesiredData(kd);
                    switch (item)
                    {
                        case SkeletonJoint.LeftElbow:
                            ksd.GetJointsData(kd.getJointsLeftElbow);
                            break;
                        case SkeletonJoint.LeftKnee:
                            ksd.GetJointsData(kd.getJointsLeftKnee);
                            break;
                        case SkeletonJoint.LeftShoulder:
                            ksd.GetJointsData(kd.getJointsLeftShoulder);
                            break;
                        case SkeletonJoint.RightElbow:
                            ksd.GetJointsData(kd.getJointsRightElbow);
                            break;
                        case SkeletonJoint.RightKnee:
                            ksd.GetJointsData(kd.getJointsRightKnee);
                            break;
                        case SkeletonJoint.RightShoulder:
                            ksd.GetJointsData(kd.getJointsRightShoulder);
                            break;
                        default:
                            break;
                    }
                }

                //write data
                for (int i = 0; i < lengthData; i++) //each line of data
                {
                    strb = new StringBuilder();
                    strb.Append(kd.Time[i].ToString() + tab);
                    foreach (var item in kd.SkeletonJointsToBeOptimised) //for each selected joint
                    {
                        strb.Append(kd.ToString(item, i));
                    }
                    sw.WriteLine(strb.ToString()); //writeline
                }


                //close sw
                sw.Close();

                //open txt-file
                System.Diagnostics.Process.Start("notepad.exe", @FileLocationAndFileName);
            }
            else
                MessageBox.Show("Geen bestand gevonden of aangemaakt", "Melding", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #endregion

        #region ANGLE DATA
        public void ExportAngleData()
        {
            setupFileNameLocationAndStreamWriter();
            KinectSelectDesiredData ksd = new KinectSelectDesiredData(kd);
            Dictionary<SkeletonJoint, List<string>> tmpData = new Dictionary<SkeletonJoint, List<string>>();
            int lengthData = kd.Data.Count;

            //hoek en bijbehorende joints kiezen
            foreach (var item in kd.SkeletonJointsToBeOptimised)
            {
                List<KinectPoint3D[]> tmpD = ksd.GetJointsData(kd.GetJoints(item));
                List<string> tmpAngles = new List<string>(tmpD.Count);
                //berekenen van de hoek
                foreach (var i in tmpD)
                {
                    tmpAngles.Add((Math.Round(kbc.CalculateAngle(i[0], i[1], i[2]), 1).ToString())); //make a string of it
                }
                //voeg angle data toe aan dictionary
                tmpData.Add(item, tmpAngles);
            }

            //write header
            sw.WriteLine("**BTKinect " + DateTime.Now.Year.ToString());
            sw.WriteLine("**Hoeken in graden");
            //build column names
            StringBuilder strb = new StringBuilder();
            strb.Append("**tijd[ms]" + tab + tab);
            foreach (var item in tmpData)
            {
                strb.Append(item.Key.ToString() + tab);
            }
            //write column names
            sw.WriteLine(strb.ToString());

            //write angles
            strb = new StringBuilder();
            for (int i = 0; i < lengthData; i++)                    //each angle
            {
                strb.Append(kd.Time[i].ToString() + tab);           //add time
                foreach (var item in tmpData)                       //each item in dictionary
                {
                    strb.Append(tmpData[item.Key][i] + tab + tab);  //one line
                }
                sw.WriteLine(strb.ToString());                      //write it
                strb = new StringBuilder();
            }

            //close sw
            sw.Close();

            //open txt-file
            System.Diagnostics.Process.Start("notepad.exe", @FileLocationAndFileName);
        }
        #endregion
    }
}
