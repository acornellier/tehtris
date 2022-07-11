using System.Collections.Generic;
using UnityEngine;

// ReSharper disable StaticMemberInGenericType

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static bool Quitting { get; set; }

    private static readonly object Lock = new();
    private static Dictionary<System.Type, Singleton<T>> _instances;

    public static T Instance
    {
        get
        {
            if (Quitting)
                return null;

            lock (Lock)
            {
                _instances ??= new Dictionary<System.Type, Singleton<T>>();

                if (_instances.ContainsKey(typeof(T)))
                    return (T)_instances[typeof(T)];

                return null;
            }
        }
    }

    private void OnEnable()
    {
        if (Quitting)
            return;

        var iAmSingleton = false;

        lock (Lock)
        {
            _instances ??= new Dictionary<System.Type, Singleton<T>>();

            if (_instances.ContainsKey(GetType()))
            {
                Destroy(gameObject);
            }
            else
            {
                iAmSingleton = true;

                _instances.Add(GetType(), this);

                DontDestroyOnLoad(gameObject);
            }
        }

        if (iAmSingleton)
            OnEnableCallback();
    }

    private void OnApplicationQuit()
    {
        Quitting = true;

        OnApplicationQuitCallback();
    }

    protected virtual void OnApplicationQuitCallback()
    {
    }

    protected virtual void OnEnableCallback()
    {
    }
}
