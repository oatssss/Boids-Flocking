using UnityEngine;
using System.Collections.Generic;

public class BenchmarkManager : UnitySingletonPersistent<BenchmarkManager> {

	private static readonly ulong MinimumNeighbourSamples = 5000;
	public static readonly string Key_SpatialStructureConstruction = "SpatialConstruction";
	public static readonly string Key_NeighbourSearchAverage = "NeighbourSearchAverage";
	public static readonly string Key_SpatialQueryAverage = "SpatialQueryAverage";
	public static readonly string Key_VelocityUpdateAverage = "VelocityUpdateAverage";
	public static readonly string Key_DeltaTimeAverage = "DeltaTimeAverage";
	public static readonly string Key_CohesionAverage = "CohesionAverage";
	public static Dictionary<string,KeyValuePair<ulong,double>> CalculatedAverages = new Dictionary<string,KeyValuePair<ulong,double>> {
		{Key_SpatialStructureConstruction, new KeyValuePair<ulong,double>(0,0)},
		{Key_NeighbourSearchAverage, new KeyValuePair<ulong,double>(0,0)},
		{Key_VelocityUpdateAverage, new KeyValuePair<ulong,double>(0,0)},
		{Key_DeltaTimeAverage, new KeyValuePair<ulong,double>(0,0)},
		{Key_CohesionAverage, new KeyValuePair<ulong,double>(0,0)},
		{Key_SpatialQueryAverage, new KeyValuePair<ulong,double>(0,0)}
	};

	private static Dictionary<string,Dictionary<int,double>> SavedAverages = new Dictionary<string,Dictionary<int,double>> {
		{ Key_NeighbourSearchAverage, new Dictionary<int,double>() },
		{ Key_SpatialStructureConstruction, new Dictionary<int,double>() },
		{ Key_VelocityUpdateAverage, new Dictionary<int,double>() },
		{ Key_DeltaTimeAverage, new Dictionary<int,double>() },
		{ Key_CohesionAverage, new Dictionary<int,double>() },
		{ Key_SpatialQueryAverage, new Dictionary<int,double>() }
	};

#if !SKIP_BENCHMARK
	/*void Start()
	{
		Invoke("ResetAverages", 5f);
		Time.maximumDeltaTime = 10;
	}

	private void ResetAverages()
	{
		CalculatedAverages = new Dictionary<string,KeyValuePair<ulong,double>> {
			{Key_SpatialStructureConstruction, new KeyValuePair<ulong,double>(0,0)},
			{Key_NeighbourSearchAverage, new KeyValuePair<ulong,double>(0,0)},
			{Key_VelocityUpdateAverage, new KeyValuePair<ulong,double>(0,0)},
			{Key_DeltaTimeAverage, new KeyValuePair<ulong,double>(0,0)},
			{Key_CohesionAverage, new KeyValuePair<ulong,double>(0,0)},
			{Key_SpatialQueryAverage, new KeyValuePair<ulong,double>(0,0)}
		};
	}

	private static int CurrentPartitionTest = 0;
	private void NextPartitionerTest()
	{
		GameObject spawnedFish = GameObject.Find("Spawned Fish");
		Destroy(spawnedFish);

		CurrentPartitionTest++;
		BoidsPartitioner nextPartitioner = null;
		if (CurrentPartitionTest == 1)
			{ nextPartitioner = BoidsManager.Instance.GetComponent<KDTreePartitioner>(); }
		else if (CurrentPartitionTest == 2)
			{ nextPartitioner = BoidsManager.Instance.GetComponent<SpatialHashPartitioner>(); }
		else
			{ Application.Quit(); }
		nextPartitioner.enabled = true;
		BoidsManager.Instance.SpatialPartitioner = nextPartitioner;
	}

	void Update()
	{
		// Debug.LogFormat("Neighbour Search Average: {0} | {2}\nSpatial Construction Average: {1} | {3}", Averages[Key_NeighbourSearchAverage].Value, Averages[Key_SpatialStructureConstruction].Value, Averages[Key_NeighbourSearchAverage].Key, Averages[Key_SpatialStructureConstruction].Key);

		// if (BoidsManager.Instance.BoidCount > 102)
		// {
		// 	this.NextPartitionerTest();
		// 	Invoke("ResetAverages", 5f);
		// 	return;
		// }

		if (BoidsManager.Instance.BoidCount > 700)
		{
			Application.Quit();
		}

        KeyValuePair<ulong,double> deltaTimeAverage = BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_DeltaTimeAverage];
        ulong n = deltaTimeAverage.Key + 1;
        double newAverage = (deltaTimeAverage.Value + Time.deltaTime)/2;
        KeyValuePair<ulong,double> newPair = new KeyValuePair<ulong,double>(n,newAverage);
        BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_DeltaTimeAverage] = newPair;

		if (CalculatedAverages[Key_NeighbourSearchAverage].Key > MinimumNeighbourSamples)
		{
			Dictionary<int,double> neighbourAverages = SavedAverages[Key_NeighbourSearchAverage];
			neighbourAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_NeighbourSearchAverage].Value;

			Dictionary<int,double> updateAverages = SavedAverages[Key_VelocityUpdateAverage];
			updateAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_VelocityUpdateAverage].Value;

			Dictionary<int,double> deltaTimeAverages = SavedAverages[Key_DeltaTimeAverage];
			deltaTimeAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_DeltaTimeAverage].Value;

			Dictionary<int,double> cohesionAverages = SavedAverages[Key_CohesionAverage];
			cohesionAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_CohesionAverage].Value;

			Dictionary<int,double> structureAverages = SavedAverages[Key_SpatialStructureConstruction];
			structureAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_SpatialStructureConstruction].Value;

			Dictionary<int,double> queryAverages = SavedAverages[Key_SpatialQueryAverage];
			queryAverages[BoidsManager.Instance.BoidCount] = CalculatedAverages[Key_SpatialQueryAverage].Value;


			foreach (string averageType in SavedAverages.Keys)
			{
				List<string> writeLines = new List<string>();
				Dictionary<int,double> typedAverages = SavedAverages[averageType];
				foreach (int boidCount in typedAverages.Keys)
				{
					double averageTicks = typedAverages[boidCount];
					// float sampledAverage = 0;
					// for (int i = 0; i < 10; i++)
					// {
					// 	sampledAverage +=
					// }
					writeLines.Add(boidCount + "," + averageTicks);
				}

				System.IO.File.WriteAllLines(Application.persistentDataPath + "/" + averageType + "_" + BoidsManager.Instance.SpatialPartitioner.GetType() + "_" + MinimumNeighbourSamples + ".csv", writeLines.ToArray());
			}

			// +5
			BoidsManager.Instance.SpawnFish();
			BoidsManager.Instance.SpawnFish();
			BoidsManager.Instance.SpawnFish();
			BoidsManager.Instance.SpawnFish();
			BoidsManager.Instance.SpawnFish();
			// +5
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// +5
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// +5
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			// BoidsManager.Instance.SpawnFish();
			this.ResetAverages();
		}
	}*/

	void OnGUI()
	{
		GUI.Label (new Rect (10,10,500,50), "Boid Count: " + BoidsManager.Instance.BoidCount);
	}
#endif
}
