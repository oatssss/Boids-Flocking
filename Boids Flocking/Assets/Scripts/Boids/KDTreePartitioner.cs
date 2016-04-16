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

        return output;
    }

    void FixedUpdate()
    {
        // Rebuild boid tree
        foreach (var pair in this.BoidsManager.AllBoids)
        {
            this.Boids[pair.Key] = new KDTree<Boid>(this.BoidsManager.AllBoids[pair.Key], DataExtractors, 0);
        }
    }
}
