using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

public class KDTreePartitioner : NoPartitioner
{
    [ReadOnly] [SerializeField] private Dictionary<Boid.TYPE,KDTree<Boid>> Boids = new Dictionary<Boid.TYPE,KDTree<Boid>>();
    private static KDTree<Boid>.DimensionDataExtractor XPosExtractor = boid => { return boid.transform.position.x; };
    private static KDTree<Boid>.DimensionDataExtractor YPosExtractor = boid => { return boid.transform.position.y; };
    private static KDTree<Boid>.DimensionDataExtractor ZPosExtractor = boid => { return boid.transform.position.z; };
    private static KDTree<Boid>.DimensionDataExtractor[] DataExtractors;

    void Awake()
    {
        DataExtractors = new KDTree<Boid>.DimensionDataExtractor[] { XPosExtractor, YPosExtractor, ZPosExtractor };
    }

    /// <summary>Performs a check based on K-Dimensional trees for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE, HashSet<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid originBoid, int maximum = int.MaxValue)
    {
#if !SKIP_BENCHMARK
        Stopwatch queryWatch = Stopwatch.StartNew();
#endif
        float minX = originBoid.transform.position.x - radius;
        float minY = originBoid.transform.position.y - radius;
        float minZ = originBoid.transform.position.z - radius;

        float maxX = originBoid.transform.position.x + radius;
        float maxY = originBoid.transform.position.y + radius;
        float maxZ = originBoid.transform.position.z + radius;

        float[] rangeMins = new float[] { minX, minY, minZ };
        float[] rangeMaxs = new float[] { maxX, maxY, maxZ };

        Dictionary<Boid.TYPE, HashSet<Boid>> output = new Dictionary<Boid.TYPE, HashSet<Boid>>();
        foreach (var type in types)
        {
            if (!this.Boids.ContainsKey(type))
                { continue; }

            HashSet<Boid> outputSet = this.Boids[type].RangeSearch(rangeMins, rangeMaxs);
            output[type] = outputSet;
        }
#if !SKIP_BENCHMARK
        queryWatch.Stop();
        KeyValuePair<ulong,double> queryAverages = BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_SpatialQueryAverage];
        ulong n = queryAverages.Key + 1;
        double newAverage = (queryAverages.Value + queryWatch.Elapsed.TotalMilliseconds)/2;
        KeyValuePair<ulong,double> newPair = new KeyValuePair<ulong,double>(n,newAverage);
        BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_SpatialQueryAverage] = newPair;
#endif

        foreach (Boid.TYPE type in output.Keys)
        {
            HashSet<Boid> nearby = output[type];
            foreach (Boid boid in nearby.ToArray())
            {
                float distance = (originBoid.transform.position - boid.transform.position).magnitude;
                if (distance > radius)
                    { nearby.Remove(boid); }
            }
        }

        return output;
    }

    void Update()
    {
#if !SKIP_BENCHMARK
        Stopwatch watch = Stopwatch.StartNew();
#endif

        // Rebuild boid tree
        foreach (var pair in this.BoidsManager.AllBoids)
        {
            this.Boids[pair.Key] = new KDTree<Boid>(this.BoidsManager.AllBoids[pair.Key], DataExtractors, 0);
        }

#if !SKIP_BENCHMARK
        watch.Stop();
        KeyValuePair<ulong,double> constructAverages = BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_SpatialStructureConstruction];
        ulong n = constructAverages.Key + 1;
        double newAverage = (constructAverages.Value + watch.Elapsed.TotalMilliseconds)/2;
        KeyValuePair<ulong,double> newPair = new KeyValuePair<ulong,double>(n,newAverage);
        BenchmarkManager.CalculatedAverages[BenchmarkManager.Key_SpatialStructureConstruction] = newPair;
#endif
    }
}
