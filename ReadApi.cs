using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WebScrap
{
    public partial class ReadApi : Form
    {
        public ReadApi()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.xxxxx.net/api/identity/login");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                // httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        Username = "xxxxx",
                        Password = "xxxx"
                    });
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }


                Item authObj;
                string sResult;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    sResult = streamReader.ReadToEnd();
                    
                    JObject jo = JObject.Parse(sResult);
                    authObj = jo.SelectToken("auth", false).ToObject<Item>();
                }


                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.xxxxxxxx.net/api/exchangerate/get");
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", authObj.token).ToString());

                IEnumerable<FxRates> convertJson;
                string jsonResult;
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonResult = (streamReader.ReadToEnd());
                    
                    JObject jo = JObject.Parse(jsonResult);
                    convertJson = jo.SelectToken("data", false).ToObject<List<FxRates>>()
                        .Where(a => a.effectiveDate == DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).ToString("MM/dd/yyyy 00:00:00"))
                        .Select(a => new FxRates { id = a.id, currencyFrom = a.currencyFrom, currencyTo=a.currencyTo,effectiveDate = a.effectiveDate,xRate = a.xRate });

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

    public class FxRates
    {
        public int id { get; set; }
        public string currencyFrom { get; set; }

        public string currencyTo { get; set; }

        public string effectiveDate { get; set; }

        public float xRate { get; set; }
    }

    public class Item
    {
        public string token { get; set; }
    }
}
