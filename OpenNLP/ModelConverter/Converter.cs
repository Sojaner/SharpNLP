using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenNLP.Tools.Chunker;
using OpenNLP.Tools.PosTagger;
using SharpEntropy;
using SharpEntropy.IO;

namespace ModelConverter
{
    /// <summary>
    /// Summary description for Converter.
    /// </summary>
    class Converter
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //https://cl.lingfil.uu.se/~nivre/swedish_treebank/

            //TODO: Use the train data for training and use the test data to confirm the accuracy
            //TODO: Use the proper EventReader/TextReader
            //TODO: Figure out the problem with the +A/+B tokens
            //TODO: Update the OpenNLP code with the latest available on https://github.com/AlexPoint/OpenNlp

            if (args.Length != 1)
            {
                Console.WriteLine("You need to specify 1 argument - the path of the folder to convert.");
            }
            else
            {
                string modelPath = args[0];

                if (ConvertFolder(modelPath))
                {
                    Console.WriteLine("conversion complete");
                }
                else
                {
                    Console.WriteLine("conversion failed");
                }
            }
            Console.ReadLine();
        }

        private static bool ConvertFolder(string folder)
        {
            try
            {
                BinaryGisModelWriter writer = new BinaryGisModelWriter();

                foreach (string file in Directory.GetFiles(folder))
                {
                    if (file.Substring(file.Length - 4, 4) == ".bin")
                    {
                        Console.Write("converting " + file + " ...");
                        writer.Persist(new GisModel(new JavaBinaryGisModelReader(file)), file.Replace(".bin", ".nbin"));
                        Console.WriteLine("done");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
                return false;
            }

            foreach (string childFolder in Directory.GetDirectories(folder))
            {
                if (!ConvertFolder(childFolder))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
