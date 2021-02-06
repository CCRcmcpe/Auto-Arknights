using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class TemplateRegister : ImageRegister
    {
        public float Threshold { get; set; } = 0.85f;

        protected override RegisterResult[] RegisterInternal(Mat model, Mat scene, int minMatchCount)
        {
            Mat diff = scene.MatchTemplate(model, TemplateMatchModes.CCoeffNormed);
            var matches = new List<(Rect rect, double confidence)>();
            for (var i = 0; i < minMatchCount; i++)
            {
                diff.MinMaxLoc(out _, out double maxVal, out _, out Point maxLoc);
                if (maxVal > Threshold)
                {
                    matches.Add((new Rect(maxLoc, model.Size()), maxVal));
                    diff.At<float>(maxLoc.Y, maxLoc.X) = 0;
                }
                else
                {
                    break;
                }
            }

            return matches.Any()
                ? matches.Select(match => RegisterResult.Succeed(match.rect, match.confidence))
                         .OrderBy(match => match.Confidence).ToArray()
                : new[] { RegisterResult.Failed() };
        }
    }
}