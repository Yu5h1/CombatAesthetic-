using UnityEngine;
using UnityEngine.Events;
using static Yu5h1Lib.GameManager.IDispatcher;

public abstract class LoadAsyncBehaviour : MonoBehaviour
{
    public abstract void OnProcessing(float percentage);    
}
public abstract class LoadAsyncBehaviour<T> : MonoBehaviour where T : Component
{
    private T _component;
    protected T component => _component ?? TryGetComponent(out _component) ? _component : null;
    public abstract void OnProcessing(float percentage);
}