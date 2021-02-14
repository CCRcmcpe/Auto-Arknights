using System;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.CV;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.Core
{
    public partial class Remote
    {
        public class Graphic : IDisposable
        {
            private const double ConfidenceThreshold = 0.8;
            private readonly ImageAssets _assets;
            private readonly TemplateRegister _register = new();
            private readonly Remote _remote;

            public Graphic(Remote remote)
            {
                _remote = remote;
                _assets = new ImageAssets(remote._resolution);
            }

            public void Dispose()
            {
                _assets.Dispose();
            }

            private static bool IsSuccessful(RegisterResult result) => result.Confidence > ConfidenceThreshold;

            public void Click(string assetExpr)
            {
                Click(Asset(assetExpr));
            }

            public void Click(Mat model)
            {
                _remote.Click(X.While(() => Locate(model), IsSuccessful,
                                      TimeSpan.FromSeconds(2))!.CircumRect);
            }

            public RegisterResult Locate(string assetExpr)
            {
                Mat model = Asset(assetExpr);
                return Locate(model);
            }

            public RegisterResult Locate(Mat model)
            {
                using Mat scrn = _remote.GetScreenshot();
                RegisterResult result = _register.Register(model, scrn, 1)[0];
                return result;
            }

            public RegisterResult[] LocateMulti(Mat model, int minMatchCount = 1)
            {
                using Mat scrn = _remote.GetScreenshot();
                RegisterResult[] result = _register.Register(model, scrn, minMatchCount);
                return result;
            }

            public bool TestAppear(string assetExpr) => IsSuccessful(Locate(assetExpr));

            public void WaitAppear(string assetExpr, double waitSec = 5) =>
                X.While(() => Locate(assetExpr), IsSuccessful, TimeSpan.FromSeconds(waitSec));

            private Mat Asset(string assetExpr) => _assets.Get(assetExpr);
        }
    }
}