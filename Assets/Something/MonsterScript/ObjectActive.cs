using UnityEngine;
using System.Collections;
public class ObjectActive : MonoBehaviour
{
    [SerializeField] private GameObject targetToActivate;

    private IEnumerator ActivateTarget()
    {
        targetToActivate.SetActive(true);
        yield return null;
    }
    public void StarActivateTarget()
    {
        StartCoroutine(ActivateTarget());
    }
}