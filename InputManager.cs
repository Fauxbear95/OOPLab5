// https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Psim.Materials;

namespace Psim.IOManagers
{
	public static class InputManager
	{
        public static Model InitializeModel(string path)
		{
            JObject modelData = LoadJson(path);
			// This model can only handle 1 material
			Material material = GetMaterial(modelData["materials"][0]);
			Model model = GetModel(material, modelData["settings"]);
			AddSensors(model, modelData["sensors"]);
			AddCells(model, modelData["cells"]);
            return model;
		}
        private static JObject LoadJson(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                JObject modelData = JObject.Parse(json);
                return modelData;
            }
        }

        private static void AddCells(Model m, JToken cellData)
        {
            foreach (var cell in cellData)
            {
                m.AddCell(cell.Value<double>("length"), cell.Value<double>("width"), cell.Value<int>("sensorID"));
                System.Console.WriteLine($"Successfully added a {cell.Value<double>("length")} x {cell.Value<double>("width")} cell to the model. The cell is linked to sensor {cell.Value<int>("sensorID")}");
            }
        }

        private static void AddSensors(Model m, JToken sensorData)
		{
            foreach (var sensor in sensorData)
            {
                m.AddSensor(sensor.Value<int>("id"), sensor.Value<double>("t_init"));
                System.Console.WriteLine($"Successfully added sensor ID: {sensor.Value<int>("id")} with an initial temperature of {sensor.Value<double>("t_init")}");
            }
        }

        private static Model GetModel(Material material, JToken settingsData)
		{
            var htemp = (double)settingsData["high_temp"];
            var ltemp = (double)settingsData["low_temp"];
            var simTime = (double)settingsData["sim_time"]; 

            return new Model(material, htemp, ltemp, simTime);
        }

        private static Material GetMaterial(JToken materialData)
		{
            var dData = GetDispersionData(materialData["d_data"]);
            var rData = GetRelaxationData(materialData["r_data"]);
            return new Material(dData, rData);
		}

        private static DispersionData GetDispersionData(JToken dData)
		{
            var wMaxla = (double)dData["max_freq_la"];
            var wMaxTa = (double)dData["max_freq_ta"];
            var laData = dData["la_data"].ToObject<double[]>();
            var taData = dData["ta_data"].ToObject<double[]>();

            return new DispersionData(laData, wMaxla, taData, wMaxTa);
        }

        private static RelaxationData GetRelaxationData(JToken rData)
		{
            var b_L = (double)rData["b_l"];
            var b_Tn = (double)rData["b_tn"];
            var b_Tu = (double)rData["b_tu"];
            var b_I = (double)rData["b_i"];
            var w = (double)rData["w"];

            return new RelaxationData(b_L, b_Tn, b_Tu, b_I, w);
		}
    }
}
