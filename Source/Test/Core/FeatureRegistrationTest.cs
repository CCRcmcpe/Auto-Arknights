using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.CV;
using Xunit;
using Xunit.Abstractions;

namespace REVUnit.AutoArknights.Core.Test
{
    public class FeatureRegistrationTest
    {
        private static readonly FeatureRegistration FeatureRegistration = new();
        private readonly ITestOutputHelper _output;

        public FeatureRegistrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> GetTest1Data()
        {
            return Directory.EnumerateDirectories(@"Data\FeatureRegistrationTest\StandardRegister").Select(s =>
                new object[]
                    {new Mat(Path.Combine(s, "model.png")), new Mat(Path.Combine(s, "scene.png"))});
        }

        [MemberData(nameof(GetTest1Data))]
        [Theory]
        public void StandardRegister(Mat model, Mat scene)
        {
            RegistrationResult[] results = FeatureRegistration.Register(model, scene, 1);
            Assert.Single(results);
            RegistrationResult result = results[0];
            Assert.InRange(result.Confidence, 0.8, 1);

            Mat diagram = scene.Clone();

            Quadrilateral32 region = result.Region.ScaleTo(0.8f);
            for (var i = 0; i < 1000; i++)
            {
                Point point = region.PickRandomPoint();
                diagram.DrawMarker(point, Scalar.Red);
            }

            diagram.Polylines(new[] {region.Vertices.Select(p => p.ToPoint())}, true, Scalar.Red, 2,
                LineTypes.AntiAlias);
            Cv2.ImEncode(".png", diagram, out byte[] bytes);

            string tmpFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tmpFilePath, bytes);
            string diagramFilePath = Path.ChangeExtension(tmpFilePath, "png");
            File.Move(tmpFilePath, diagramFilePath);

            _output.WriteLine($"Diagram written to {diagramFilePath}");
        }
    }
}