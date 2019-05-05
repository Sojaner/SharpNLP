using System;
using System.IO;
using System.Linq;
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

            int iterations = 0;

            float percentage = 0;

            GisModel model = null;

            while (percentage < 90f)
            {
                iterations += 500;

                model = MaximumEntropyPosTagger.Train(new PosEventReader(File.OpenText(@".\data\postagger-talbanken-dep-train.conll")), iterations, 5);

                MaximumEntropyPosTagger tagger = new MaximumEntropyPosTagger(model);

                string[] lines = File.ReadAllLines(@".\data\postagger-talbanken-dep-test.conll");

                int equals = 0;

                foreach (string line in lines)
                {
                    string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split("_")[0]).ToArray();

                    string[] tags = tagger.Tag(parts);

                    string sentence = "";

                    for (int i = 0; i < parts.Length; i++)
                    {
                        sentence += $"{parts[i]}_{tags[i]} ";
                    }

                    sentence = sentence.Remove(sentence.Length - 1);

                    equals += sentence == line ? 1 : 0;
                }

                percentage = (equals / (float)lines.Length) * 100f;
            }

            PlainTextGisModelWriter writer = new PlainTextGisModelWriter();

            writer.Persist(model, @".\pos.model");

            //TODO: Use the train data for training and use the test data to confirm the accuracy
            //TODO: Use the proper EventReader/TextReader
            //TODO: Figure out the problem with the +A/+B tokens

            return;

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
