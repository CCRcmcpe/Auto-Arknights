using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class TemplateRegistration : ImageRegistration
    {
        public override RegistrationResult[] Register(Mat model, Mat scene, int minMatchCount)
        {
            Mat diff = scene.MatchTemplate(model, TemplateMatchModes.CCoeffNormed);
            var matches = new List<(Rect rect, double confidence)>();

            for (var i = 0; i < minMatchCount; i++)
            {
                diff.MinMaxLoc(out _, out double maxVal, out _, out Point maxLoc);
                matches.Add((new Rect(maxLoc, model.Size()), maxVal));
                diff.At<float>(maxLoc.Y, maxLoc.X) = 0;
            }

            return matches.Select(match => new RegistrationResult(Quadrilateral.FromRect(match.rect), match.confidence))
                .OrderBy(match => match.Confidence).ToArray();
        }
    }
}