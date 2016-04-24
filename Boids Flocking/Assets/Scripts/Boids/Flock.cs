using UnityEngine;
using System.Collections.Generic;

public class Flock : MonoBehaviour {

	[ReadOnly] public Boid Leader;
	private HashSet<Boid> Members;

	public void MergeInto(Flock other)
	{
		if (this.Leader == other.Leader)
			{ return; }

		other.Members.UnionWith(this.Members);

		foreach (Boid member in this.Members)
			{ member.Flock = other; }

		this.Members.Clear();
	}

	public void Join(Boid member)
	{
		if (member.Flock != null)
			{ member.Flock.Leave(member); }

		this.Members.Add(member);
		member.Flock = this;
	}

	public void Leave(Boid member)
	{
		this.Members.Remove(member);

		if (member.Flock == this)
			{ member.Flock = null; }
	}

	void Update()
	{
		if (this.Members.Count == 0)
			{ Destroy(this.gameObject); }
	}
}
