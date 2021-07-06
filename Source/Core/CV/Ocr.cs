using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using Tesseract;
using Rect = OpenCvSharp.Rect;

namespace REVUnit.AutoArknights.Core.CV
{
    public class TextBlock
    {
        public TextBlock(string text, float confidence, Rect? rect = null)
        {
            Text = text;
            Confidence = confidence;
            Rect = rect;
        }

        public float Confidence { get; }
        public Rect? Rect { get; }
        public string Text { get; }
    }

    public static class Ocr
    {
        private static readonly TesseractEngine Engine;

        private const string TesseractDataPath = @".\Assets\Tesseract";

        static Ocr()
        {
            Engine = new TesseractEngine(Path.Combine(TesseractDataPath, "TrainedData"), "chi_sim+eng",
                EngineMode.LstmOnly);
            Engine.SetVariable("debug_file", "/dev/null");
        }

        public static TextBlock Single(Mat image, string? patternName = null)
        {
            if (patternName != null)
                Engine.SetVariable("user_patterns_file", Path.Combine(TesseractDataPath, "UserPatterns", patternName));

            using Page page = Engine.Process(image.ToPix(), PageSegMode.SingleLine);

            if (patternName != null)
                Engine.SetVariable("user_patterns_file", string.Empty);

            using ResultIterator iter = page.GetIterator();
            string text = iter.GetText(PageIteratorLevel.Block);
            float confidence = iter.GetConfidence(PageIteratorLevel.Block);
            return new TextBlock(text, confidence);
        }

        public static TextBlock[] Sparse(Mat image)
        {
            using Page page = Engine.Process(image.ToPix(), PageSegMode.SparseText);
            using ResultIterator iter = page.GetIterator();
            var results = new List<TextBlock>();
            while (iter.Next(PageIteratorLevel.Block))
            {
                string text = iter.GetText(PageIteratorLevel.Block);
                float confidence = iter.GetConfidence(PageIteratorLevel.Block);
                if (!iter.TryGetBoundingBox(PageIteratorLevel.Block, out Tesseract.Rect rectT))
                {
                    throw new Exception();
                }

                Rect rect = rectT.ToCvRect();
                results.Add(new TextBlock(text, confidence, rect));
            }

            return results.ToArray();
        }

        private static Rect ToCvRect(this Tesseract.Rect rect)
        {
            return new(rect.X1, rect.Y1, rect.Width, rect.Height);
        }

        private static Pix ToPix(this Mat mat)
        {
            return Pix.LoadFromMemory(mat.ToBytes());
        }
    }
}