using System;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.CV;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.Core
{
    public partial class UserInterface
    {
        public class GraphicalInterface
        {
            private readonly ImageAssets _assets = new();
            private readonly TemplateRegister _register = new();
            private readonly UserInterface _userInterface;

            public GraphicalInterface(UserInterface userInterface) => _userInterface = userInterface;
            public double ConfidenceThreshold { get; set; } = 0.75;

            private bool IsSuccessful(RegisterResult result) => result.Confidence > ConfidenceThreshold;

            public void Dispose()
            {
                _assets.Dispose();
            }

            public void Click(string assetExpr)
            {
                Click(Asset(assetExpr));
            }

            public void Click(Mat model)
            {
                _userInterface.Click(X.While(() => Locate(model), IsSuccessful,
                                             TimeSpan.FromSeconds(2))!.CircumRect);
            }

            public RegisterResult Locate(string assetExpr)
            {
                Mat model = Asset(assetExpr);
                return Locate(model);
            }

            public RegisterResult Locate(Mat model)
            {
                using Mat scrn = _userInterface.GetScreenshot();
                RegisterResult result = _register.Register(model, scrn, 1)[0];
                return result;
            }

            public RegisterResult[] LocateMulti(Mat model, int minMatchCount = 1)
            {
                using Mat scrn = _userInterface.GetScreenshot();
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