// Processes CSV best-n-gram files produced for the Malware classification
// Program then reads labels from trainLabels.csv and creates a database using fileName, opCodes and Labels
// Created By: Ankit Sadana
// Created On: 04/29/2015
// Last Modified By: Ankit Sadana
// Last Modified On: 05/05/2015

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVFileProcessor
{
    // Class for storing data
    class DataExample
    {
        public string fileName;
        public List<uint> opCode;
        public string label;

        public DataExample(string file, List<uint> counts)
        {
            fileName = file;
            opCode = counts;
            label = "0";
        }
    }

    // Class for storing fileNames and labels from trainLabels.csv
    class fileNLabel
    {
        public string fileName;
        public string label;

        public fileNLabel(string file, string lbl)
        {
            fileName = file;
            label = lbl;
        }
    }

    // Class for storing opCodes and their respective counts
    class opNCount : IComparable<opNCount>
    {
        public string opCode;
        public uint count;

        public opNCount(string op, uint num)
        {
            opCode = op;
            count = num;
        }

        // Overriding system function to compare and sort this class' list
        public int CompareTo(opNCount value)
        {
            return this.count.CompareTo(value.count);
        }
    }

    // Main class
    class Program
    {
        // Main function
        static void Main(string[] args)
        {
            string ngram = "3";

            // List to store all file names in folder
            List<string> allFileNames = new List<string>();

            // Reading all files in the directory with specific ending
            DirectoryInfo dInfo = new DirectoryInfo("..\\..\\csvFiles\\train");
            FileInfo[] fInfo = dInfo.GetFiles("*-" + ngram + "gram-best-100.csv");

            // Reading the best count n-gram csv file
            string[] bestFile = File.ReadAllLines("..\\..\\othercsvFiles\\best-" + ngram + "-gram-100.csv");

            // List to store opCodes and their respective counts
            List<opNCount> best = new List<opNCount>();

            foreach(string x in bestFile)
            {
                string[] temp = x.Split(',');
                best.Add(new opNCount(temp[0], Convert.ToUInt32(temp[1])));
            }

            // Sorting the list of opCodes in Descending order
            best.Sort();
            best.Reverse();

            // Change the size of string array for number of features to be selected from the top count
            // check stores all the opCodes we are using
            string[] check = new string[60];
            int i = 0;

            foreach(opNCount bg in best.Take(check.Length))
            {
                check[i] = bg.opCode;
                i++;
            }

/*
                    // Writing top 30 into a file
                      StreamWriter writer = new StreamWriter("..\\..\\outputCSV\\" + ngram + "gram-top15.csv");

                       foreach(opNCount bg in best.Take(15))
                       {
                           writer.WriteLine(bg.opCode + "," + bg.count);
                       }
                       writer.Close();
 */                     

            // Removing the extra elements from the name
            foreach (FileInfo file in fInfo)
            {
                allFileNames.Add(file.Name.Replace("-" + ngram + "gram-best-100.csv", ""));
            }

            // data stores all the data
            DataExample[] data = new DataExample[allFileNames.Count];
            int counter = 0;

            foreach(FileInfo file in fInfo)
            {
                Console.WriteLine("Processing file#" + counter);

                string[] fileText = File.ReadAllLines(file.FullName);

                // listForFile stores all counts and opCodes
                List<opNCount> listForFile = new List<opNCount>();

                foreach (string x in fileText)
                {
                    string[] temp = x.Split(',');
                    listForFile.Add(new opNCount(temp[0], Convert.ToUInt32(temp[1])));
                }

                // count stores all the counts found in check, in the same order of check
                List<uint> count = new List<uint>();
                
                bool found;
                
                foreach(string a in check)
                {
                    found = false;
                    
                    foreach(opNCount ops in listForFile)
                    {
                        // if current string in check is found in listForFile, it is added to the count list
                        if(a.Equals(ops.opCode))
                        {
                            count.Add(ops.count);
                            found = true;
                            break;
                        }
                    }

                    // if not found, 0 is added as a default value
                    if (!found)
                    {
                        count.Add(0);
                    }
                }

                // data is initialized with fileName and count list
                data[counter] = new DataExample(allFileNames[counter], count);
                counter++;
            }

            // Reading trainLabels.csv
            string[] labelFile = File.ReadAllLines("..\\..\\othercsvFiles\\trainLabels.csv");

            // List to store file names and their labels
            List<fileNLabel> lbls = new List<fileNLabel>();

            foreach (string x in labelFile)
            {
                string[] temp = x.Split(',');
                lbls.Add(new fileNLabel(temp[0], temp[1]));
            }

            for (counter = 0; counter < data.Length; counter++)
            {
                foreach (fileNLabel labels in lbls)
                {
                    // Checking file name match and adding it's respective label
                    if (data[counter].fileName.Equals(labels.fileName))
                    {
                        data[counter].label = labels.label;
                    }
                }
            }
           
            Console.WriteLine("Writing to the file");

            // Writing all data to a csv file
            StreamWriter writeMe = new StreamWriter("..\\..\\outputCSV\\test-" + ngram + "gram.csv");
            
            // Writing Headers
            //string line = "";
            string line = "FileName,";

            foreach(string x in check)
            {
                line = line + x + ",";
            }

            // For test set, removing the last ','
            // line = line.TrimEnd(',');

            writeMe.WriteLine(line);

            // Reading and writing data
            foreach (DataExample de in data)
            {
                line = "";
                line = de.fileName + ",";
                foreach(uint counts in de.opCode)
                {
                    line = line + counts + ",";
                }

                // For test set, removing the last ','
                // line = line.TrimEnd(',');

                writeMe.WriteLine(line);
            }
            
            writeMe.Close();
          
            Console.ReadLine();
        }
    }
}
