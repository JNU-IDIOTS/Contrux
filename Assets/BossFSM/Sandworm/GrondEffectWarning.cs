using UnityEngine;
using System.Collections;

public class GrondEffectWarning : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(bomb());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator bomb()
    {
        yield return new WaitForSeconds(3.5f);
        Destroy(gameObject);
    }
}
