using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenCvSharp;
using Polly;
using Refit;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public static class TxOcr
    {
        private static readonly ITxOcr Api;

        static TxOcr() => Api = RestService.For<ITxOcr>("https://ai.qq.com");

        public static string Ocr(Mat image)
        {
            string[] results = OcrMulti(image);
            return results.Length == 0 ? string.Empty : results[0];
        }

        public static string[] OcrMulti(Mat image)
        {
            const int maxRetryTimes = 3;
            PolicyResult<JsonElement> result = Policy
                                              .HandleResult<JsonElement>(
                                                   root => root.GetProperty("ret").GetInt32() != 0)
                                              .WaitAndRetry(maxRetryTimes, i => TimeSpan.FromSeconds(3 * i),
                                                            (falutedResult, waitSpan, i, _) =>
                                                            {
                                                                Log.Error(Resources.TxOcr_Exception_Ocr,
                                                                            i, maxRetryTimes,
                                                                            falutedResult.Result.GetProperty("msg")
                                                                               .GetString(), waitSpan.Seconds);
                                                            })
                                              .ExecuteAndCapture(() =>
                                               {
                                                   using var ms = image.ToMemoryStream(
                                                       ".jpg", new ImageEncodingParam(ImwriteFlags.JpegQuality, 90));
                                                   string json = Api.Ocr(new StreamPart(ms, "file.png", "image/png"))
                                                                    .Result;
                                                   return JsonDocument.Parse(json).RootElement;
                                               });

            if (result.FaultType != null)
            {
                throw new Exception(Resources.TxOcr_Exception_OcrFailed);
            }

            return result.Result.GetProperty("data").GetProperty("item_list").EnumerateArray()
                         .Select(fields => fields.GetProperty("itemstring").GetString()!).ToArray();
        }

        public interface ITxOcr
        {
            [Headers(
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4321.0 Safari/537.36 Edg/88.0.702.0")]
            [Post("/cgi-bin/appdemo_generalocr")]
            [Multipart("------WebKitFormBoundary")]
            Task<string> Ocr([AliasAs("image_file")] StreamPart stream);
        }
    }
}