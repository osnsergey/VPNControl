using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VPNControl
{
    class FileReader
    {
        private string[] fileContent;

        public FileReader(string path)
        {
            // Open the file to read from.
            fileContent = File.ReadAllLines(path);            
        }

        public string[] GetContent()
        {
            return fileContent;
        }
    }
}
