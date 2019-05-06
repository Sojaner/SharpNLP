using System;
using System.Diagnostics;
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

            GisModel model = null;// new GisModel(new JavaBinaryGisModelReader(@"C:\Users\Rojan\Desktop\Gis\pos.model"));

            Console.Clear();

            while (percentage <= 95f)
            {
                iterations +=10;

                Console.WriteLine($"------------------------------------");

                Console.Write($"Training {iterations} iterations... ");

                Stopwatch stopwatch = Stopwatch.StartNew();

                model = MaximumEntropyPosTagger.Train(new PosEventReader(File.OpenText(@".\data\postagger-talbanken-dep-train.conll")), iterations, 0);

                stopwatch.Stop();

                Console.WriteLine($"Trained in {stopwatch.Elapsed:g}.");

                MaximumEntropyPosTagger tagger = new MaximumEntropyPosTagger(model);

                string[] lines = File.ReadAllLines(@".\data\postagger-talbanken-dep-test.conll");

                //string[] lines = File.ReadAllLines(@"C:\Users\Rojan\Desktop\G.conll");

                int equals = 0;

                int total = 0;

                int top = Console.CursorTop;

                foreach (string line in lines)
                {
                    string[][] strings = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split("_")).ToArray();

                    string[] parts = strings.Select(s => s[0]).ToArray();

                    string[] reals = strings.Select(s => s[1]).ToArray();

                    try
                    {
                        string[] tags = tagger.Tag(parts);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            equals += reals[i] == tags[i] ? 1 : 0;

                            total++;
                        }
                    }
                    catch
                    {
                        total += parts.Length;
                    }

                    Console.CursorTop = top;

                    Console.CursorLeft = 0;

                    Console.WriteLine($"Equals: {equals}, Total: {total}, Percentage: {equals / (float)total * 100f}%");
                }

                percentage = equals / (float)total * 100f;
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
