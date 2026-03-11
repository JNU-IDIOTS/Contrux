using UnityEngine;
using System.Collections;

public class SelfDeactivator : MonoBehaviour
{
    private void Awake()
    {
        DeactivateSelf();
    }

    private void FixedUpdate()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(SelfDeativate());
        }

    }

    public void DeactivateSelf()
    {
        gameObject.SetActive(false);
    }
    private IEnumerator SelfDeativate()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("다시 실행");
        DeactivateSelf();
    }
}