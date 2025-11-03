using System;
using System.Collections;
using UnityEngine;

public class BootstrapRunner : MonoBehaviour
{
    private static BootstrapRunner _inst;

    private void Awake()
    {
        if (_inst == null) { _inst = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public static void RunAfterOneFrame(Action cb)
    {
        if (_inst == null)
        {
            var go = new GameObject("BootstrapRunner");
            _inst = go.AddComponent<BootstrapRunner>();
            DontDestroyOnLoad(go);
        }
        _inst.StartCoroutine(_inst.InvokeNextFrame(cb));
    }

    private IEnumerator InvokeNextFrame(Action cb)
    {
        yield return null; // 한 프레임 대기: 씬 오브젝트 초기화 타이밍 보장
        cb?.Invoke();
    }
}
