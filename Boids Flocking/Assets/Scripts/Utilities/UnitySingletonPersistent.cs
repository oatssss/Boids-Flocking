using UnityEngine;

public class UnitySingletonPersistent<T> : MonoBehaviour
	where T : Component
{
	private static T instance;
	public static T Instance {
		get {
			if (instance == null) {
				instance = FindObjectOfType<T> ();
				if (instance == null) {
					GameObject obj = new GameObject ();
					instance = obj.AddComponent<T> ();
				}
			}
			return instance;
		}
	}

	protected virtual void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
		if (instance == null) {
			instance = this as T;
		} else {
			Destroy (gameObject);
		}
	}
}