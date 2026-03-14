using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static  T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                SetupInstance();
            }
            return _instance;
        }
    }

    protected virtual void Awake() 
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private static void SetupInstance()
    {
        _instance = FindAnyObjectByType<T>();

        if (_instance == null)
        {
            var gameObject = new GameObject();
            gameObject.name = nameof(T);
            _instance = gameObject.AddComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }
}
