using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ExtensionMethods;

public class KDTree<T> {

    private KDTree<T> LeftTree = null;
    private KDTree<T> RightTree = null;
    private T Data = default(T);
    private int Depth;
    private int Dimensions;
    private int Dimension { get { return this.Depth % this.Dimensions; } }
    public delegate float DimensionDataExtractor(T data);
    private DimensionDataExtractor[] DataExtractors;
    private DimensionDataExtractor DataExtractor { get { return this.DataExtractors[this.Dimension]; } }
    private float[] Minimums;
    private float[] Maximums;

    public KDTree(List<T> collection, DimensionDataExtractor[] dataExtractors, int depth, float[] minimums = null, float[] maximums = null)
    {
        this.Depth = depth;
        this.Dimensions = dataExtractors.Length;
        this.DataExtractors = dataExtractors;
        this.Minimums = minimums;
        this.Maximums = maximums;

        if (this.Minimums == null)
        {
            this.Minimums = new float[this.Dimensions];
            this.Minimums.Populate(float.NegativeInfinity);
        }

        if (this.Maximums == null)
        {
            this.Maximums = new float[this.Dimensions];
            this.Maximums.Populate(float.PositiveInfinity);
        }

        Comparison<T> comparison = (a, b) => { return (int)(this.DataExtractor(a) - this.DataExtractor(b)); };
        collection.Sort(comparison);
        int median = collection.Count/2;
        // int leftUpperBound  = Math.Max(0,median-1);
        int rightLowerBound = Math.Min(collection.Count-1,median+1);
        List<T> leftItems   = collection.GetRange(0, median);
        List<T> rightItems  = collection.GetRange(rightLowerBound,collection.Count-rightLowerBound);

        this.Data = collection[median];
        // Recurse
        if (collection.Count <= 1) { return; }
        if (leftItems.Count > 0)
        {
            float[] updatedMaximums = (float[])this.Maximums.Clone();
            updatedMaximums[this.Dimension] = this.DataExtractor(this.Data);
            this.LeftTree = new KDTree<T>(leftItems, dataExtractors, depth+1, (float[])this.Minimums.Clone(), updatedMaximums);
        }
        if (rightItems.Count > 0)
        {
            float[] updatedMinimums = (float[])this.Minimums.Clone();
            updatedMinimums[this.Dimension] = this.DataExtractor(this.Data);
            this.RightTree = new KDTree<T>(rightItems, dataExtractors, depth+1, updatedMinimums, (float[])this.Maximums.Clone());
        }
    }

    public HashSet<T> NearestNeighboursSearch(float minRadius, Vector3 point,  List<KeyValuePair<float,Boid>> nearest, int numNeighbours)
    {
        HashSet<T> neighbours = new HashSet<T>();

        return neighbours;
    }

    public HashSet<T> RangeSearch(float[] dimensionMins, float[] dimensionMaxs)
    {
        HashSet<T> output = new HashSet<T>();
        this.RangeSearch(dimensionMins, dimensionMaxs, ref output);
        return output;
    }

    private void RangeSearch(float[] dimensionMins, float[] dimensionMaxs, ref HashSet<T> output)
    {
        if (this.Data.Equals(default(T)))
            { return; }

        // Add whole subtree if within the ranges
        // /*
        if (this.WholeTreeWithinRange(dimensionMins, dimensionMaxs))
        {
            this.AddWholeTree(ref output);
            return;
        }
        // */

        // Add node's data to output if within range
        if (this.DataWithinRange(dimensionMins, dimensionMaxs))
            { output.Add(this.Data); }

        // Determine if the branches are within dimension range
        bool potentialInLeft  = dimensionMins[this.Dimension] < this.DataExtractor(this.Data);
        bool potentialInRight = dimensionMaxs[this.Dimension] > this.DataExtractor(this.Data);

        // Recurse
        if (this.LeftTree != null && potentialInLeft)
            { this.LeftTree.RangeSearch(dimensionMins, dimensionMaxs, ref output); }

        if (this.RightTree != null && potentialInRight)
            { this.RightTree.RangeSearch(dimensionMins, dimensionMaxs, ref output); }
    }

    private bool WholeTreeWithinRange(float[] dimRangeMins, float[] dimRangeMaxs)
    {
        for (int i = 0; i < this.Minimums.Length; i++)
            { if (dimRangeMins[i] > this.Minimums[i]) return false; }

        for (int i = 0; i < this.Maximums.Length; i++)
            { if (dimRangeMaxs[i] < this.Maximums[i]) return false; }

        return true;
    }

    private bool DataWithinRange(float[] dimRangeMins, float[] dimRangeMaxs)
    {
        for (int i = 0; i < dimRangeMins.Length; i++)
            { if (dimRangeMins[i] > this.DataExtractors[i](this.Data)) return false; }

        for (int i = 0; i < dimRangeMaxs.Length; i++)
            { if (dimRangeMaxs[i] < this.DataExtractors[i](this.Data)) return false; }

        return true;
    }

    private void AddWholeTree(ref HashSet<T> output)
    {
        if (!this.Data.Equals(default(T)))
            { output.Add(this.Data); }

        if (this.LeftTree != null)
            { this.LeftTree.AddWholeTree(ref output); }

        if (this.RightTree != null)
            { this.RightTree.AddWholeTree(ref output); }
    }
}
