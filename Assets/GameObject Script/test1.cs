using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class test1 : MonoBehaviour
{

    private Transform playerT;

    void Start()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null) playerT = go.transform;
        else Debug.LogError("Player를 찾지 못함");
    }

    void Update()
    {
        /*if (playerT != null)
            Debug.Log("Player 위치: " + playerT.position);*/
    }


}
