using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorAggregation
{
    class Program
    {
        
        public class Array
        {
            public double temperature { get; set; }
            public double humidity { get; set; }
            public string roomArea { get; set; }
            public int id { get; set; }
            public object timestamp { get; set; }
        }

        public class Root
        {
            public List<Array> array { get; set; }
        }

        public class AggData
        {
            public double temperature { get; set; }
            public double humidity { get; set; }
            public string roomArea { get; set; }            
            public string SensorDate { get; set; }
        }
        public class GroupAggData
        {
            public string roomArea { get; set; }
            public string SensorDate { get; set; }
        }
        public class ResultsAggData
        {
            
            public string roomArea { get; set; }
            public string SensorDate { get; set; }
            public double min_temperature { get; set; }
            public double max_temperature { get; set; }
            public double median_temperature { get; set; }
            public double avg_temperature { get; set; }
            public double min_humidity { get; set; }
            public double max_humidity { get; set; }
            public double median_humidity { get; set; }
            public double avg_humidity { get; set; }
        }


        static void Main(string[] args)
        {
            

            List<AggData> DateData = new List<AggData>();
            string workingDirectory = Environment.CurrentDirectory + "\\json_file\\sensor_data.json";
            using (StreamReader r = new StreamReader(workingDirectory))
            {
                var FileJson = r.ReadToEnd();
                var SensorArray = JsonConvert.DeserializeObject<Root>(FileJson);
                
                foreach (var DataSensor in SensorArray.array)
                {
                    AggData JoinData = new AggData();
                    JoinData.humidity = DataSensor.humidity;
                    JoinData.temperature = DataSensor.temperature;
                    JoinData.roomArea = DataSensor.roomArea;
                    var DateSen = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(Convert.ToInt64(DataSensor.timestamp) / 1000d)).ToLocalTime();
                    JoinData.SensorDate = DateSen.ToString("dd-MM-yyyy");
                    DateData.Add(JoinData);
                }
                
            }

            List<ResultsAggData> ResAggData = new List<ResultsAggData>();
            /// Grouping            
            var GroupData = DateData
            .Select(c => new GroupAggData
            {
                roomArea = c.roomArea,
                SensorDate = c.SensorDate
                
            }).GroupBy(x => new { x.roomArea, x.SensorDate })
            .ToList();            

            foreach (var groupMaster in GroupData)
            {
                string RoAr = groupMaster.Key.roomArea.ToString();
                string SeDa = groupMaster.Key.SensorDate.ToString();
                
                
                var DtMstr = DateData.Where(x => x.roomArea == RoAr && x.SensorDate == SeDa);

                // Temperature
                var min_temperature = DtMstr.Min(x => x.temperature);
                var max_temperature = DtMstr.Max(x => x.temperature);
                double median_temperature = 0;
                int CountTemp = DtMstr.Count();
                int halfIndexTemp = DtMstr.Count() / 2;
                var sortedTemp = DtMstr.Select(x => x.temperature).OrderBy(n => n);

                if ((CountTemp % 2) == 0)
                {
                    median_temperature = ((sortedTemp.ElementAt(halfIndexTemp) +
                    sortedTemp.ElementAt((halfIndexTemp - 1))) / 2);
                }
                else
                {
                    median_temperature = sortedTemp.ElementAt(halfIndexTemp);
                }

                var avg_temperature = DateData.Average(x => x.temperature);

                // humidity
                var min_humidity = DtMstr.Min(x => x.humidity);
                var max_humidity = DtMstr.Max(x => x.humidity);
                double median_humidity = 0;

                int Counthumidity = DtMstr.Count();
                int halfIndexhumidity = DtMstr.Count() / 2;
                var sortedHumidity = DtMstr.Select(x => x.humidity).OrderBy(n => n);

                if ((Counthumidity % 2) == 0)
                {
                    median_humidity = ((sortedHumidity.ElementAt(halfIndexhumidity) +
                    sortedHumidity.ElementAt((halfIndexhumidity - 1))) / 2);
                }
                else
                {
                    median_humidity = sortedHumidity.ElementAt(halfIndexhumidity);
                }

                var avg_humidity = DateData.Average(x => x.humidity);



                ResultsAggData JoinData = new ResultsAggData();
                JoinData.roomArea = RoAr;
                JoinData.SensorDate = SeDa;
                JoinData.min_temperature = min_temperature;
                JoinData.max_temperature = max_temperature;
                JoinData.median_temperature = median_temperature;
                JoinData.avg_temperature = avg_temperature;
                JoinData.min_humidity = min_humidity;
                JoinData.max_humidity = max_humidity;
                JoinData.median_humidity = median_humidity;
                JoinData.avg_humidity = avg_humidity;
                ResAggData.Add(JoinData);
            }
            var jsonObj = JsonConvert.SerializeObject(ResAggData);
            Console.WriteLine(jsonObj.ToString());
            Console.ReadLine();

        }
    }
}
