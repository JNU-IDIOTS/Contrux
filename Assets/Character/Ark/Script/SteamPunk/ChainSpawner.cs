using System.Collections.Generic;
using UnityEngine;

public class ChainSpawner : MonoBehaviour
{
    [Header("링크 설정")]
    public GameObject chainLinkPrefab;    // 체인 링크 프리팹
    public float linkSpacing = 0.2f;      // 링크 간 간격
    public int maxLinks = 20;

    [Header("연결 대상")]
    public Transform startPoint; // 플레이어(또는 손)
    public Transform endPoint;   // 갈고리 끝

    private List<GameObject> linkPool = new List<GameObject>();

    private void Awake()
    {
        // 풀링을 위해 최대 maxLinks만큼 미리 생성
        for (int i = 0; i < maxLinks; i++)
        {
            GameObject link = Instantiate(chainLinkPrefab, Vector3.zero, Quaternion.identity);
            link.SetActive(false);
            linkPool.Add(link);
        }
    }

    private void Update()
    {
        // 갈고리가 비활성화된 상태라면 풀에 있는 모든 링크를 꺼 두고 리턴
        if (!endPoint.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < linkPool.Count; i++)
                if (linkPool[i].activeSelf)
                    linkPool[i].SetActive(false);
            return;
        }

        // 1) startPoint와 endPoint 사이의 벡터 (Vector3)
        Vector3 delta = endPoint.position - startPoint.position;
        float chainLength = delta.magnitude;

        // 2) neededLinks: 필요한 링크 개수
        int neededLinks = Mathf.FloorToInt(chainLength / linkSpacing);
        neededLinks = Mathf.Clamp(neededLinks, 0, maxLinks);

        // 3) 방향벡터 dir (Vector3, 길이 1)
        Vector3 dir = delta.normalized;

        // 4) 각 링크를 배치
        for (int i = 0; i < neededLinks; i++)
        {
            GameObject link = linkPool[i];
            if (!link.activeSelf) link.SetActive(true);

            // ** 이 한 줄이 원래 문제의 코드였던 부분 **
            //Vector3 pos = (Vector2)startPoint.position + dir * (i * linkSpacing);

            // → 아래처럼 수정합니다.
            Vector3 pos = startPoint.position + dir * (i * linkSpacing);

            link.transform.position = pos;

            // 링크 하나하나가 dir 방향을 바라보도록 회전
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            link.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // 5) 남은 링크 풀은 모두 비활성화
        for (int i = neededLinks; i < linkPool.Count; i++)
        {
            if (linkPool[i].activeSelf)
                linkPool[i].SetActive(false);
        }
    }
}
