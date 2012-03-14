using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;


namespace BTKinect
{
    class FileHandling
    {
        private string fileName = null;
        private string fileLocation = null;
        private string fileLocationAndFileName = null;
        private bool _blFileFound = false;
        private SaveFileDialog saveFileDialog;
        private OpenFileDialog openFileDialog;
        public const string StringHeaderRaw = "**Tijd \t Marker \t HoofdX \t HoofdY \t HoofdZ \t NekX \t NekY \t NekZ \t LinkerSchouderX \t LinkerSchouderY \t LinkerSchouderZ \t LinkerElleboogX \t LinkerElleboogY \t LinkerElleboogZ \t LinkerHandX \t LinkerHandY \t LinkerHandZ \t RechterSchouderX \t RechterSchouderY \t RechterSchouderZ \t RechterElleboogX \t RechterElleboogY \t RechterElleboogZ \t RechterHandX \t RechterHandY \t RechterHandZ \t TorsoX \t TorsoY \t TorsoZ \t LinkerHeupX \t LinkerHeupY \t LinkerHeupZ \t LinkerKnieY \t LinkerKnieY \t LinkerKnieZ \t LinkerVoetX \t LinkerVoetY \t LinkerVoetZ \t RechterHeupX \t RechterHeupY \t RechterHeupZ \t RechterKnieX \t RechterKnieY \t RechterKnieZ \t RechterVoetX \t RechterVoetY \t RechterVoetZ";
        public const string StringHeaderRawMarker = "**Tijd \t HoofdX \t HoofdY \t HoofdZ \t NekX \t NekY \t NekZ \t LinkerSchouderX \t LinkerSchouderY \t LinkerSchouderZ \t LinkerElleboogX \t LinkerElleboogY \t LinkerElleboogZ \t LinkerHandX \t LinkerHandY \t LinkerHandZ \t RechterSchouderX \t RechterSchouderY \t RechterSchouderZ \t RechterElleboogX \t RechterElleboogY \t RechterElleboogZ \t RechterHandX \t RechterHandY \t RechterHandZ \t TorsoX \t TorsoY \t TorsoZ \t LinkerHeupX \t LinkerHeupY \t LinkerHeupZ \t LinkerKnieY \t LinkerKnieY \t LinkerKnieZ \t LinkerVoetX \t LinkerVoetY \t LinkerVoetZ \t RechterHeupX \t RechterHeupY \t RechterHeupZ \t RechterKnieX \t RechterKnieY \t RechterKnieZ \t RechterVoetX \t RechterVoetY \t RechterVoetZ";

        private void handleDialog(FileDialog fileDialog)
        {
            // Default file extension
            fileDialog.DefaultExt = "*.*";

            // Available file extensions
            fileDialog.Filter = "file Allfiles (*.*)|*.*";

            // Adds a extension if the user does not
            fileDialog.AddExtension = true;

            // Restores the selected directory, next time
            fileDialog.RestoreDirectory = true;

            // Dialog title
            fileDialog.Title = "Geef de locatie van het bestand op";

            // Startup directory
            fileDialog.InitialDirectory = @"C:/";

            fileDialog.ShowDialog();

            extractStrings(fileDialog);
        }
        private void handleDialog(FileDialog fileDialog, string extension)
        {
            // Default file extension
            fileDialog.DefaultExt = extension;

            // Available file extensions
            fileDialog.Filter = "file (*." + extension + ")|*." + extension + "|All files (*.*)|*.*";

            // Adds a extension if the user does not
            fileDialog.AddExtension = true;

            // Restores the selected directory, next time
            fileDialog.RestoreDirectory = true;

            // Dialog title
            fileDialog.Title = "Geef de locatie van het bestand op";

            // Startup directory
            fileDialog.InitialDirectory = @"C:/";

            fileDialog.ShowDialog();

            extractStrings(fileDialog);
        }
        private void handleDialog(FileDialog fileDialog, string extension1, string extension2)
        {
            // Default file extension
            fileDialog.DefaultExt = extension1;

            // Available file extensions
            fileDialog.Filter = "file (*." + extension1 + ")|*." + extension1 + "|" + "file (*." + extension2 + ")|*." + extension2 + "|All files (*.*)|*.*";

            // Adds a extension if the user does not
            fileDialog.AddExtension = true;

            // Restores the selected directory, next time
            fileDialog.RestoreDirectory = true;

            // Dialog title
            fileDialog.Title = "Geef de locatie van het bestand op";

            // Startup directory
            fileDialog.InitialDirectory = @"C:/";

            fileDialog.ShowDialog();

            extractStrings(fileDialog);
        }
        private void extractStrings(FileDialog fileDialog)
        {
            try
            {
                fileLocationAndFileName = fileDialog.FileName;
                fileLocation = Path.GetDirectoryName(fileLocationAndFileName);
                fileName = Path.GetFileName(fileLocationAndFileName);
                _blFileFound = true;
            }
            catch (Exception)
            {
                _blFileFound = false;
            }
        }

        #region ACCESSORS
        public bool FileFound
        {
            get { return _blFileFound; }
        }
        public string FileName
        {
            get
            {

                return fileName;
            }
            set { fileName = value; }
        }
        public string FileLocation
        {
            get
            {
                return fileLocation;
            }
        }
        public string FileLocationAndFileName
        {
            get
            {
                return fileLocationAndFileName;
            }
            set { fileLocationAndFileName = value; }
        }
        public void OpenFile(string extension)
        {
            openFileDialog = new OpenFileDialog();
            handleDialog(openFileDialog, extension);
        }
        public void OpenFile()
        {
            openFileDialog = new OpenFileDialog();
            handleDialog(openFileDialog);
        }
        public string SaveFile()
        {
            saveFileDialog = new SaveFileDialog();
            handleDialog(saveFileDialog);
            return fileLocationAndFileName;
        }
        #endregion
    }
}
