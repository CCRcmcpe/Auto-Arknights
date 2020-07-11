using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using Refit;

namespace REVUnit.AutoArknights.Core
{
    public static class TxOcr
    {
        private static readonly ITxOcr Api;

        static TxOcr()
        {
            Api = RestService.For<ITxOcr>("https://ai.qq.com");
        }

        public static string Ocr(Mat image)
        {
            string[] results = OcrMulti(image);
            return results.Length == 0 ? string.Empty : results[0];
        }

        public static string[] OcrMulti(Mat image)
        {
            return ((JArray) OcrInner(image)["data"]!["item_list"])!.Select(it => (string) it["itemstring"]).ToArray();
        }

        private static JObject OcrInner(Mat image)
        {
            using var ms = image.ToMemoryStream();
            return Api.Ocr(new StreamPart(ms, "file.png", "image/png")).Result;
        }

        public interface ITxOcr
        {
            [Post("/cgi-bin/appdemo_generalocr")]
            [Multipart("------WebKitFormBoundary")]
            Task<JObject> Ocr([AliasAs("image_file")] StreamPart stream);
        }
    }
}