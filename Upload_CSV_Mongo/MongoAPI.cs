using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Collections.Generic;

namespace Upload_CSV_Mongo
{
    internal class PasteData
    {
        public string _id { get; set; }
        public string facility { get; set; }
        public string module { get; set; }
        public string commodity_class { get; set; }
        public string month { get; set; }
        public string ipn { get; set; }
        public double consume { get; set; }
        public double order { get; set; }
        public double loss { get; set; }
        public double process_waste { get; set; }
        public double wip_inventory { get; set; }
        public double other { get; set; }
        //public string action { get; set; }

    }
    internal class MongoAPI
    {
        static string api = @"http://slam-api.intel.com/material/";
        static HttpClient client = new HttpClient();
        //public MongoAPI()
        //{
        //    if (client.BaseAddress == null)
        //    {
        //        client.BaseAddress = new Uri(api);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    }
        //}
        private static void StartClient()
        {
            try
            {
                if (Program.currentConfig.api != null)
                {
                    api = Program.currentConfig.api;
                }
                if (client.BaseAddress == null)
                {
                    client.BaseAddress = new Uri(api);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog("Start client err: " + ex.Message);
            }
        }
        public static async void InsertData(List<PasteData> listData)//PasteData pasteData)
        {
            StartClient();
            try
            {
                var data = JsonConvert.SerializeObject(listData, Formatting.Indented);

                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(api, content);
                if (result.IsSuccessStatusCode)
                {
                    if (Program.currentConfig.show_upload_result == true)
                    {
                        Console.WriteLine("Upload success. Status response: " + result.StatusCode);
                    }
                    if (Program.currentConfig.write_log == true)
                    {
                        Program.WriteLog($"Insert data success.Folder: {Path.GetDirectoryName(Program.pathTo_MAOCSV)}");
                    }
                }
                else
                {
                    if (Program.currentConfig.show_upload_result == true)
                    {
                        Console.WriteLine("Upload failed: " + result.StatusCode);
                    }
                    if (Program.currentConfig.write_log == true)
                    {
                        Program.WriteLog($"Insert data failed.Status code: {result.StatusCode.ToString()}.Folder: {Path.GetDirectoryName(Program.pathTo_MAOCSV)}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Program.currentConfig.show_upload_result == true)
                {
                    Console.WriteLine("Insert data err: " + ex.Message);
                }
                Program.WriteLog("Insert data err: " + ex.Message);
            }
        }
    }
}
