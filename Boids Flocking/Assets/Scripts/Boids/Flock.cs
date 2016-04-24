using UnityEngine;
using System.Collections.Generic;

public class Flock : MonoBehaviour {

	[ReadOnly] public Boid Leader;
	public int MaxSize = 10;
	public Dictionary<Boid.TYPE,HashSet<Boid>> TypedMembers = new Dictionary<Boid.TYPE,HashSet<Boid>>();
	public int Size {
		get {
			int count = 0;
			foreach (Boid.TYPE type in this.TypedMembers.Keys)
			{
				count += this.TypedMembers[type].Count;
			}
			return count;
		}
	}

	public static Flock CreateFlock(Boid leader)
	{
		if (leader.Flock != null)
			{ Debug.LogWarningFormat("{0} attempted to create a flock when already a member of another", leader); return null; }

		GameObject flockObject = new GameObject();
		flockObject.name = "Flock";
		flockObject.transform.SetParent(leader.transform);
		flockObject.transform.localPosition = Vector3.zero;
		Flock flock = flockObject.AddComponent<Flock>();
		flock.Leader = leader;
		flock.MaxSize = leader.MaxFlockSize;
		leader.Flock = flock;

		return flock;
	}

	public bool CanMergeInto(Flock other)
	{
		if (other == null)
			{ return false; }

		return this.Size + other.Size <= other.MaxSize;
	}

	public bool ShouldMergeInto(Flock other)
	{
		if (other == null)
			{ return false; }

		return this.Size <= other.MaxSize/2;
	}

	public void MergeInto(Flock other)
	{
		if (this.Leader == other.Leader)
			{ return; }

		foreach (Boid.TYPE type in this.TypedMembers.Keys)
		{
			HashSet<Boid> otherTypeMembers;
			if (other.TypedMembers.ContainsKey(type))
				{ otherTypeMembers = other.TypedMembers[type]; }
			else
				{ otherTypeMembers = new HashSet<Boid>(); other.TypedMembers[type] = otherTypeMembers; }

			otherTypeMembers.UnionWith(this.TypedMembers[type]);

			foreach (Boid member in this.TypedMembers[type])
				{ member.Flock = other; }
		}

		this.TypedMembers.Clear();
	}

	public bool CanAddMember()
	{
		return this.Size + 1 <= this.MaxSize;
	}

	public void AddMember(Boid member)
	{
		if (member.Flock != null)
			{ member.Flock.RemoveMember(member); }

		HashSet<Boid> typedMembers;
		if (this.TypedMembers.ContainsKey(member.Type))
			{ typedMembers = this.TypedMembers[member.Type]; }
		else
			{ typedMembers = new HashSet<Boid>(); this.TypedMembers[member.Type] = typedMembers; }

		typedMembers.Add(member);
		member.Flock = this;
	}

	public void RemoveMember(Boid removing)
	{
		if (this.TypedMembers.ContainsKey(removing.Type))
		{
			this.TypedMembers[removing.Type].Remove(removing);

			if (removing.Flock == this)
				{ removing.Flock = null; }
		}
		// else
		// {
		// 	Debug.LogWarningFormat("{0} isn't a member of flock {1}", removing, this);
		// }
	}

	void Update()
	{
		if (this.TypedMembers.Count == 0)
			{ Destroy(this.gameObject); }
	}
}
