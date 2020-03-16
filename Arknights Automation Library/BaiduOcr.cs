using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Flurl;
using Flurl.Http;
using OpenCvSharp;

namespace REVUnit.AutoArknights.GUI.Core
{
    public static class BaiduOcr
    {
        private const string OcrApi = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic";
        private const string OcrAccApi = "https://aip.baidubce.com/rest/2.0/ocr/v1/accurate_basic";
        private const string FindWithLocApi = "https://aip.baidubce.com/rest/2.0/ocr/v1/general";
        private const string FindWithLocAccApi = "https://aip.baidubce.com/rest/2.0/ocr/v1/accurate";
        private const string GetTokenApi = "https://aip.baidubce.com/oauth/2.0/token";
        private static string _accessToken;
        public static string ApiKey { get; set; }
        public static string SecretKey { get; set; }
        public static bool AccurateMode { get; set; }

        private static async Task EnsureTokenInit()
        {
            if (_accessToken == null)
            {
                dynamic result = await GetTokenApi.SetQueryParam("grant_type",
                        "client_credentials")
                    .SetQueryParam("client_id",
                        ApiKey)
                    .SetQueryParam("client_secret",
                        SecretKey)
                    .GetAsync()
                    .ReceiveJson();
                _accessToken = result.access_token;
            }
        }
        
        public static async Task<dynamic> Ocr(Mat image)
        {
            await EnsureTokenInit();
            return await (AccurateMode ? OcrAccApi : OcrApi).SetQueryParam("access_token",
                    _accessToken)
                .PostAsync(new ShitBaiduContent(new Dictionary<string, string>
                {
                    {"image", image.ToBase64()}
                })).ReceiveJson();
        }

        public static async Task<dynamic> FindWithLoc(Mat image)
        {
            await EnsureTokenInit();
            return await (!AccurateMode ? FindWithLocApi : FindWithLocAccApi).SetQueryParam("access_token",
                    _accessToken)
                .PostAsync(new ShitBaiduContent(new Dictionary<string, string>
                {
                    {"image", image.ToBase64()}
                })).ReceiveJson();
        }

        private static string ToBase64(this Mat image)
        {
            return Convert.ToBase64String(image.ToBytes());
        }

        public class ShitBaiduContent : ByteArrayContent
        {
            public ShitBaiduContent(IEnumerable<KeyValuePair<string, string>> content)
                : base(GetCollectionBytes(content, Encoding.UTF8))
            {
            }

            private static byte[] GetCollectionBytes(IEnumerable<KeyValuePair<string, string>> c, Encoding encoding)
            {
                string str = string.Join("&",
                    c.Select(i => string.Concat(HttpUtility.UrlEncode(i.Key), '=', HttpUtility.UrlEncode(i.Value)))
                        .ToArray());
                return encoding.GetBytes(str);
            }
        }
    }
}