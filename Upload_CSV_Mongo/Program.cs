using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Upload_CSV_Mongo
{
    internal class Config
    {
        public string path_CSV, api, log_file;
        public bool write_log, show_upload_result, pause_after_run;
    }
    internal class Program
    {
        public static Config currentConfig;
        public static string pathTo_MAOCSV = @"\\vnatshfs.intel.com\VNATAnalysis$\MAOATM\Config\VN\VNAT_AE\Direct Material\material_consumption.csv";
       
        public static void WriteLog(string log)
        {
            string logPath = Path.Combine(Environment.CurrentDirectory, "UploadCSV_Mongo.log");
            if (currentConfig.log_file != null)
            {
                logPath = currentConfig.log_file;
            }
            try
            {
                File.AppendAllText(logPath, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $"\t{log}{Environment.NewLine}");
            }
            catch { }
        }
        private static List<PasteData> _ReturnListData()
        {
            if (currentConfig.path_CSV != null)
            {
                pathTo_MAOCSV = currentConfig.path_CSV;
            }
            Console.WriteLine("Path to CSV: " + pathTo_MAOCSV);
            List<PasteData> listPasteData = new List<PasteData>();
            try
            {
                using (FileStream fs = new FileStream(pathTo_MAOCSV, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        int facility_idx = 0;
                        int module_idx = 0;
                        int commodity_class_idx = 0;
                        int month_idx = 0;
                        int ipn_idx = 0;
                        int consume_idx = 0;
                        int order_idx = 0;
                        int loss_idx = 0;
                        int process_waste_idx = 0;
                        int wip_inventory_idx = 0;
                        int other_idx = 0;

                        while ((line = sr.ReadLine()) != null)
                        {
                            line = Regex.Replace(line, @"\""", string.Empty);
                            string[] splLine = line.Split(',');

                            if (line.Contains("facility"))
                            {
                                facility_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("facility")).FirstOrDefault());
                                module_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("module")).FirstOrDefault());
                                commodity_class_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("commodity_class")).FirstOrDefault());
                                month_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("month")).FirstOrDefault());
                                ipn_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("ipn")).FirstOrDefault());
                                consume_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("consume")).FirstOrDefault());
                                order_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("order")).FirstOrDefault());
                                loss_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("loss")).FirstOrDefault());
                                process_waste_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("process_waste")).FirstOrDefault());
                                wip_inventory_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("wip_inventory")).FirstOrDefault());
                                other_idx = Array.IndexOf(splLine, splLine.Where(s => s.Equals("other")).FirstOrDefault());

                                Console.WriteLine("Get index of all columns success.");
                                continue;
                            }
                            PasteData currentData = new PasteData
                            {
                                facility = splLine[facility_idx],
                                module = splLine[module_idx],
                                commodity_class = splLine[commodity_class_idx],
                                month = splLine[month_idx].Split('.')[0],
                                ipn = splLine[ipn_idx],
                                consume = double.Parse(splLine[consume_idx].Trim()),
                                order = double.Parse(splLine[order_idx].Trim()),
                                loss = double.Parse(splLine[loss_idx].Trim()),
                                process_waste = double.Parse(splLine[process_waste_idx].Trim()),
                                wip_inventory = double.Parse(splLine[wip_inventory_idx].Trim()),
                                other = double.Parse(splLine[other_idx].Trim())
                            };
                            listPasteData.Add(currentData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parse csv err: " + ex.Message);
                WriteLog("Parse csv err: " + ex.Message);
            }
            Console.WriteLine("Data parser completed.");
            return listPasteData;

        }
        private static void UploadData()
        {
            try
            {
                var listData = _ReturnListData();

                MongoAPI.InsertData(listData);
            }
            catch (Exception ex)
            {
                WriteLog("Upload err: " + ex.Message);
            }
        }
        private static bool ReadConfig()
        {
            try
            {
                TextReader configData = new StringReader(File.ReadAllText("config.yaml"));
                var deserializer = new Deserializer();
                currentConfig = deserializer.Deserialize<Config>(configData);
                Console.WriteLine("Read connfig success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Read config err: " + ex.Message);
                WriteLog("Read config err: " + ex.Message);
                return false;
            }
        }
        static void Main(string[] args)
        {
            try
            {
                bool readConfig = ReadConfig();
                if (readConfig)
                {
                    UploadData();

                    if (currentConfig.pause_after_run == true)
                    {
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Read config error.");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                WriteLog("Main crash: " + ex.Message);
            }
        }
    }
}
