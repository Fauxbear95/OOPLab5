using System;

using Psim.Particles;
using Psim.ModelComponents;
using Psim.Materials;

namespace Psim
{
	class Program
	{
		static void Main(string[] args)
		{
			DispersionData dData;
			dData.LaData = new double[] { -2.22e-7, 9260.0, 0.0};
			dData.TaData = new double[] { -2.28e-7, 5240.0, 0.0};
			dData.WMaxLa = 7.63916048e13;
			dData.WMaxTa = 3.0100793072e13;

			RelaxationData rData;
			rData.Bl = 1.3e-24;
			rData.Btn = 9e-13;
			rData.Btu = 1.9e-18;
			rData.BI = 1.2e-45;
			rData.W = 2.42e13;

			Material silicon = new (in dData, in rData);

			// Test the AddSensor, AddCell and SetSurfaces implementations. 

			Model model = new(silicon, 4000, 100, 30);

			int numCells = 10;
			for (int i = 0; i < numCells; ++i)
			{
				model.AddSensor(i, 400);
				model.AddCell(10, 10, i);
			}

			// Display cell surface types
			Console.WriteLine("Surfaces before setting them\n");
			PrintSurfaceAllocations(model);

			model.SetSurfaces(200);
			model.SetEmitPhonons(300, 3, 5e-9);

			// Display sensor and cell info
			Console.WriteLine("\n\n" + model);

			// Display cell surface types again after assignment
			Console.WriteLine("Surfaces after setting them\n");
			PrintSurfaceAllocations(model);

			// Calling add sensor and add cell methods and then displaying the changes
			model.AddSensor(model.sensors.Count, 310);
			model.AddCell(10, 10, model.cells.Count);
			model.SetSurfaces(300);

			Console.WriteLine("\n\nAdded new sensor and cell\n");
			Console.WriteLine(model);
			PrintSurfaceAllocations(model);
		}

		private static void PrintSurfaceAllocations(Model model)
		{
			Console.Write(String.Format("{0, -5} {1, 25} {2, 40}\n\n", "Cell", "Left Surface", "Right Surface"));

			int cellNum = 1;
			foreach (Cell cell in model.cells)
			{
				Console.Write(String.Format("{0, -5} {1, -40} {2, -50}\n", cellNum++, cell.GetSurface(SurfaceLocation.left), cell.GetSurface(SurfaceLocation.right)));
			}
		}
	}
}
