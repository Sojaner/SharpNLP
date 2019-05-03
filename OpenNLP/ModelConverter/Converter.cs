using System;
using System.IO;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
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
            string project = "I rollen som försäkringsförmedlare hjälper du kunder med att hitta den bästa lösningen utifrån deras behov. I samband med utbildningsstart får du genomgå certifiering för försäkringsförmedling där du får alla verktyg för att du ska känna dig trygg i din roll. Du agerar som specialist där du ger professionell rådgivning och tar fram olika försäkringslösningar till privatkunder. ";

            JavaBinaryGisModelReader sentenceModelReader = new JavaBinaryGisModelReader(@".\bins\sent.bin");

            GisModel sentenceModel = new GisModel(sentenceModelReader);

            MaximumEntropySentenceDetector detector = new MaximumEntropySentenceDetector(sentenceModel);

            JavaBinaryGisModelReader tokenModelReader = new JavaBinaryGisModelReader(@".\bins\token.bin");

            GisModel tokenModel = new GisModel(tokenModelReader);

            MaximumEntropyTokenizer tokenizer = new MaximumEntropyTokenizer(tokenModel);

            JavaBinaryGisModelReader posModelReader = new JavaBinaryGisModelReader(@".\bins\pos.bin");

            GisModel posModel = new GisModel(posModelReader);

            MaximumEntropyPosTagger tagger = new MaximumEntropyPosTagger(posModel);

            string[] sentences = detector.SentenceDetect(project);

            foreach (string sentence in sentences)
            {
                string[] tokens = tokenizer.Tokenize(sentence);

                string[] tags = tagger.Tag(tokens);
            }

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
