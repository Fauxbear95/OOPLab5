﻿/* Lab Question  (Test 2)
 * 
 * A bottom-up approach is typically used in OOP languages. This is the general design approach that has been used
 * for this software. Do you think it would have been easier or harder to approach this project using a top-down approach?
 * Why do you think a bottom up approach is generally more natural when using OOP languages?
 *
 */

using System;
using System.Collections.Generic;

using Psim.ModelComponents;
using Psim.Materials;

namespace Psim
{
	// Model is comprised of a single material. Hardcoding the time step & number of phonons for now.
	class Model
	{
		private const double TIME_STEP = 5e-12;
		private const int NUM_PHONONS = 10000000;
		private Material material;
		public List<Cell> cells = new() { };
		public List<Sensor> sensors = new() { };
		private readonly double highTemp;
		private readonly double lowTemp;
		private readonly double simTime;
		private readonly double tEq;

		public Model(Material material, double highTemp, double lowTemp, double simTime)
		{
			this.material = material;
			this.highTemp = highTemp;
			this.lowTemp = lowTemp;
			this.simTime = simTime;
			tEq = (highTemp + lowTemp) / 2;
		}

		public void AddSensor(int sensorID, double initTemp)
        {
			sensors.Add(new Sensor(sensorID, this.material, initTemp));
        }

		public void AddCell(double length, double width, int sensorID)
        {
			foreach (Sensor sensor in sensors)
            {
				if(sensor.ID == sensorID)
                {
					cells.Add(new Cell(length, width, sensor));
					sensor.AddToArea(cells[cells.Count - 1].Area);
                }
            }
        }
		
		/// <summary>
		/// Automatically sets all the surfaces in the cells that constitute this model.
		/// Should be called after all the cells have been added
		/// </summary>
		/// <param name="tEq">The equilibrium temperature of the system</param>
		public void SetSurfaces(double tEq)
		{
			// TODO: Implemenent -> Assume that the system is linear!!
			int numCells = cells.Count;
			if (numCells < 2)
			{
				throw new InvalidNumberOfCells();
			}
			//assign the surfaces to the first and last cells because they are unique
			cells[0].SetEmitSurface(SurfaceLocation.left, highTemp);
			cells[0].SetTransitionSurface(SurfaceLocation.right, cells[1]);

			cells[cells.Count - 1].SetEmitSurface(SurfaceLocation.right, lowTemp);
			cells[cells.Count - 1].SetTransitionSurface(SurfaceLocation.left, cells[cells.Count - 2]);

			for (int cell = 1; cell < cells.Count - 1; ++cell)
			{
				cells[cell].SetTransitionSurface(SurfaceLocation.left, cells[cell - 1]);
				cells[cell].SetTransitionSurface(SurfaceLocation.right, cells[cell + 1]);
			}
		}

		/// <summary>
		/// Calibrates the emitting surfaces in the model.
		/// </summary>
		/// <param name="tEq">System equilibrium temperature</param>
		/// <param name="effEnergy">Phonon packet effective energy</param>
		/// <param name="timeStep">Simulation time step</param>
		public void SetEmitPhonons(double tEq, double effEnergy, double timeStep)
		{
			foreach (Cell cell in cells)
            {
				cell.SetEmitPhonons(tEq, effEnergy, timeStep);
            }
		}

		/// <summary>
		/// Returns the total energy of the model (initial energy + emit energy)
		/// </summary>
		/// <returns>Total energy generated by the model over the course of the simulation</returns>
		public double GetTotalEnergy()
		{
			double emitEnergy = 0; 
			foreach(Cell cell in cells)
            {
				emitEnergy += cell.EmitEnergy(tEq, simTime) + cell.InitEnergy(tEq);
            }
			return emitEnergy;
		}
		public override string ToString()
		{
			string output = $"Model total energy: {GetTotalEnergy()}\n";

			foreach (Cell cell in cells)
			{
				output += string.Format("{0} {1}\n", cell.ToString(), cell.EmitEnergy(tEq, simTime));
			}

			return output;
		}
	}
	class InvalidNumberOfCells : Exception
    {
		public InvalidNumberOfCells() { }
		public InvalidNumberOfCells(string description = "") : base(String.Format("You have entered an invalid number of cells", description)) { }
    }
}