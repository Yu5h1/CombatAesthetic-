using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEvent2D : MonoBehaviour
{
    [SerializeField]
    private UnityEvent OnCollisionEnter2DEvent;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnter2DEvent?.Invoke();
    }
}
public class CollisionEvent2D<T> : MonoBehaviour where T : Behaviour
{
    [Serializable]
    public class TEvent : UnityEvent<T> { }

    public TEvent OnCollisionEnter2DEvent;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out T behaviour))
            return;
        OnCollisionEnter2DEvent?.Invoke(behaviour);
    }
}