using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace ValkyrieHashAnalyzer
{
    class CsvFileReader
    {
        public static List<string> ReadFile(string fileName)
        {
            List<string> hashCodes = new List<string>();

            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    if (fields != null)
                        //foreach (string field in fields)
                        //{
                            hashCodes.Add(fields.First());
                        
                        //}
                }
            }

            var dist = hashCodes.Distinct().ToArray();
            var correct = hashCodes.Where(h => h.IsValidSHA1()).ToList();
            return correct; 
        }
    }
}
