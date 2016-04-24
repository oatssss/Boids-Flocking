using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SpatialHashPartitioner : NoPartitioner
{
    private class Bin<T> : IEnumerable where T : MonoBehaviour {
        public readonly int x, y, z;
        public HashSet<T> Members = new HashSet<T>();

        public Bin(T first) {
            this.x = (int)Mathf.Floor(first.transform.position.x / SpatialHashPartitioner.BinSize);
            this.y = (int)Mathf.Floor(first.transform.position.y / SpatialHashPartitioner.BinSize);
            this.z = (int)Mathf.Floor(first.transform.position.z / SpatialHashPartitioner.BinSize);
            this.Members.Add(first);
        }

        public static int GetBinHash(T member) {
            int x = (int)Mathf.Floor(member.transform.position.x / SpatialHashPartitioner.BinSize);
            int y = (int)Mathf.Floor(member.transform.position.y / SpatialHashPartitioner.BinSize);
            int z = (int)Mathf.Floor(member.transform.position.z / SpatialHashPartitioner.BinSize);
            return GetHashCode(x, y, z);
        }

        public static Bin<T> GetDummyBin(T member) {
            return new Bin<T>(member);
        }

        public static int GetHashCode(int x, int y, int z) {
            return x + y + z;
        }

        public override int GetHashCode() {
            return GetHashCode(this.x, this.y, this.z);
        }

        public void AddMember(T member) {
            this.Members.Add(member);
        }

        public IEnumerator GetEnumerator()
        {
            return this.Members.GetEnumerator();
        }
    }

    private Dictionary<Boid.TYPE,Dictionary<int,Bin<Boid>>> SpatialHash = new Dictionary<Boid.TYPE,Dictionary<int,Bin<Boid>>>();
    private int _binSizeSetting {
        get { return (int)Mathf.Ceil(this.BoidsManager.FishNeighbourRadius); }
    }

    private static int BinSize;

    /// <summary>Performs a check based on spacial hashing for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE,HashSet<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid originBoid, int maximum = int.MaxValue)
    {
        Dictionary<Boid.TYPE,HashSet<Boid>> found = new Dictionary<Boid.TYPE,HashSet<Boid>>();

        Dictionary<Boid.TYPE,List<int>> surroundingBinHashes = this.GetSurroundingBinHashes(types, originBoid, radius);
        foreach (Boid.TYPE type in types)
        {
            Dictionary<int,Bin<Boid>> spatialHash = this.SpatialHash[type];
            List<int> typedHashes = surroundingBinHashes[type];
            if (typedHashes == null)
                { continue; }
            HashSet<Boid> neighbours = new HashSet<Boid>();
            foreach (int hash in typedHashes)
            {
                if (!spatialHash.ContainsKey(hash))
                    { continue; }

                Bin<Boid> bin = spatialHash[hash];
                foreach (Boid boid in bin)
                {
                    float distance = (boid.transform.position - originBoid.transform.position).magnitude;
                    if (distance < radius)
                        { neighbours.Add(boid); }
                }
            }

            if (neighbours.Count > 0)
                { found.Add(type,neighbours); }
        }

        return found;
    }

    private Dictionary<Boid.TYPE,List<int>> GetSurroundingBinHashes(Boid.TYPE[] types, Boid origin, float radius)
    {
        Dictionary<Boid.TYPE,List<int>> typedBinHashes = new Dictionary<Boid.TYPE,List<int>>();

        int xMin = (int)Mathf.Floor((origin.transform.position.x - radius)/BinSize);
        int xMax = (int)Mathf.Floor((origin.transform.position.x + radius)/BinSize);
        int yMax = (int)Mathf.Floor((origin.transform.position.y + radius)/BinSize);
        int yMin = (int)Mathf.Floor((origin.transform.position.y - radius)/BinSize);
        int zMax = (int)Mathf.Floor((origin.transform.position.z + radius)/BinSize);
        int zMin = (int)Mathf.Floor((origin.transform.position.z - radius)/BinSize);

        foreach (Boid.TYPE type in types)
        {
            List<int> binHashes = new List<int>();
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int z = zMin; z < zMax; z++)
                    {
                        binHashes.Add(Bin<Boid>.GetHashCode(x, y, z));
                    }
                }
            }

            if (binHashes.Count > 0)
                { typedBinHashes.Add(type, binHashes); }
            else
                { typedBinHashes.Add(type, null); }
        }

        return typedBinHashes;
    }

    private Dictionary<Boid.TYPE,List<int>> GetBinHashesInSphere(Boid.TYPE[] types, Boid origin, float radius)
    {
        Dictionary<Boid.TYPE,List<int>> typedBinHashes = new Dictionary<Boid.TYPE,List<int>>();

        // Integer-variant of Bresenham's Circle Algorithm
        Dictionary<int,int[]> sphereBounds = new Dictionary<int,int[]>();
        int intRadius = (int)Mathf.Floor(radius);
        int y = 0, x = intRadius, dp = 1 - intRadius;
        while (y < x)
        {
            if (dp < 0)
                { dp = dp + 2 * (++y) + 3; }
            else
                { dp = dp + 2 * (++y) - 2 * (--x) + 5; }

            // Add the min/max bounds for x and z (inclusive) coordinates at the given y level and reflect to get other octants
            if (!sphereBounds.ContainsKey(y)) { sphereBounds.Add(y, new int[] {-x,x}); }
            if (!sphereBounds.ContainsKey(-y)) { sphereBounds.Add(-y, new int[] {-x,x}); }
            if (!sphereBounds.ContainsKey(x)) { sphereBounds.Add(x, new int[] {-y,y}); }
            if (!sphereBounds.ContainsKey(-x)) { sphereBounds.Add(-x, new int[] {-y,y}); }
        }

        // sphereBounds maps an x/zMin and x/zMax for every y-value within the sphere radius, we'll use it to add the relevant bins along those y levels
        foreach (Boid.TYPE type in types)
        {
            List<int> hashesForType = new List<int>();
            foreach (var yPair in sphereBounds)
            {
                int[] minMaxPair = sphereBounds[yPair.Key];
                int min = minMaxPair[0];
                int max = minMaxPair[1];

                int yOffset = (int)Mathf.Floor(origin.transform.position.y) + yPair.Key;
                // min and max describe a square in the xz plane for the current y level containing the required bins
                for (int xCurrent = min; xCurrent <= max; xCurrent++)
                {
                    int xOffset = (int)Mathf.Floor(origin.transform.position.x) + xCurrent;
                    for (int zCurrent = min; zCurrent <= max; zCurrent++)
                    {
                        int zOffset = (int)Mathf.Floor(origin.transform.position.z) + zCurrent;
                        hashesForType.Add(Bin<Boid>.GetHashCode(xOffset, yOffset, zOffset));
                        if (origin.DebugFocus) Debug.LogWarningFormat("BIN: [{0},{1},{2}]", xOffset, yOffset, zOffset);
                    }
                }
            }

            if (hashesForType.Count > 0)
                { typedBinHashes.Add(type, hashesForType); }
        }

        if (origin.DebugFocus)
        {
            Debug.LogWarningFormat("RADIUS: {0}", x);
        }

        return typedBinHashes;
    }

    void FixedUpdate()
    {
        this.ConstructSpatialHash();
    }

    private void ConstructSpatialHash()
    {
        BinSize = this._binSizeSetting;
        this.SpatialHash = new Dictionary<Boid.TYPE,Dictionary<int,Bin<Boid>>>();

        foreach (var typeListing in this.BoidsManager.AllBoids)
        {
            Dictionary<int,Bin<Boid>> typedBins = new Dictionary<int,Bin<Boid>>();
            foreach (Boid boid in typeListing.Value)
            {
                int hash = Bin<Boid>.GetBinHash(boid);
                if (typedBins.ContainsKey(hash))
                {
                    // Get the bin and add this boid as a member
                    Bin<Boid> bin = typedBins[hash];
                    bin.AddMember(boid);
                }
                else
                {
                    // Make a new bin and add it to the dictionary
                    Bin<Boid> bin = new Bin<Boid>(boid);
                    typedBins.Add(hash, bin);
                }
            }
            this.SpatialHash.Add(typeListing.Key, typedBins);
        }
    }
}
