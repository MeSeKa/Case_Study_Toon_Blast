using UnityEngine;

public class MonoBehaviourSingletonSceneOnly<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            //DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

public class MonoBehaviourSingletonDontDestroyOnLoad<T> : MonoBehaviour
	where T : Component
{
	public static T Instance { get; private set; }

	public virtual void Awake()
	{
		if (Instance == null)
		{
			Instance = this as T;
            transform.parent = null; // Ensure the singleton is not a child of another object
			DontDestroyOnLoad(this);
        }
		else
		{
			Destroy(gameObject);
		}
	}
}

public class Singleton<T> where T : class, new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) instance = new T();
            return instance;
        }
    }
}