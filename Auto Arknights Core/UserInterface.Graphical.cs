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
            private readonly Assets _assets = new();
            private readonly UserInterface _userInterface;
            private readonly FeatureBasedImageRegister _register = new("Assets/Cache");

            public GraphicalInterface(UserInterface userInterface) => _userInterface = userInterface;

            public DeformationLevel DeformationLevel { get; set; }

            public void Dispose()
            {
                _assets.Dispose();
                _register.Dispose();
            }

            public void Click(string assetExpr)
            {
                Click(Asset(assetExpr));
            }

            public void Click(Mat model)
            {
                _userInterface.Click(X.While(() => Locate(model), result => result.IsSucceed,
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
                RegisterResult result = _register.Locate(model, scrn, DeformationLevel);
                return result;
            }

            public bool TestAppear(string assetExpr) => Locate(assetExpr).IsSucceed;

            public void WaitAppear(string assetExpr, double waitSec = 5)
            {
                X.While(() => Locate(assetExpr), result => result.IsSucceed, TimeSpan.FromSeconds(waitSec));
            }

            private Mat Asset(string assetExpr) => _assets.Get(assetExpr);
        }
    }
}