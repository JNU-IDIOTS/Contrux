// LoadingTipRotator.cs (drop-in 교체)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingTipRotator : MonoBehaviour
{
    [Header("Data & UI")]
    [SerializeField] private LoadingTipsDatabase db;
    [SerializeField] private LoadingUI ui;
    [SerializeField] private Typewriter typewriter;     // (선택) 있으면 타자기 효과로 출력

    [Header("Behaviour")]
    [SerializeField] private float intervalSec = 3f;    // 교체 주기
    [SerializeField] private bool unscaledTime = true;  // 로딩 중엔 보통 true
    [SerializeField] private string requiredTag = "";   // 특정 태그만(빈 값이면 전체)
    [SerializeField] private bool showImmediately = true;       // Begin() 시 즉시 1회 표시
    [SerializeField] private bool avoidImmediateRepeat = true;  // 직전 문구 바로 반복 금지

    readonly List<int> _bag = new();     // 가중치 추첨 가방
    Coroutine _co;
    int _lastIdx = -1;
    string _activeTag = "";

    void Awake()
    {
        if (!ui) ui = FindObjectOfType<LoadingUI>(true);
        if (!typewriter) typewriter = FindObjectOfType<Typewriter>(true); // 없으면 null이어도 OK
    }

    /// <summary>
    /// 로테이터 시작. tagOverride로 특정 태그만 보여줄 수 있음(미지정 시 requiredTag 사용)
    /// </summary>
    public void Begin(string sceneName = null, string tagOverride = null)
    {
        _activeTag = string.IsNullOrEmpty(tagOverride) ? requiredTag : tagOverride;
        Debug.Log("[Tip] Begin");
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Run());
    }

    public void End()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
        _bag.Clear();
        _lastIdx = -1;
        Debug.Log("[Tip] End");
        // 로딩 종료 시 팁 영역 비우기(원하면 유지해도 됨)
        if (typewriter != null) typewriter.Play("");
        else ui?.SetTip("");
    }

    IEnumerator Run()
    {
        if (!db || db.tips == null || db.tips.Count == 0)
        {
            Debug.LogWarning("[LoadingTipRotator] Tips database is empty.");
            yield break;
        }

        RefillBag();
        if (_bag.Count == 0)
        {
            Debug.LogWarning("[LoadingTipRotator] No tips matched filters.");
            yield break;
        }

        // 시작 즉시 1회 노출
        if (showImmediately)
        {
            ShowNextTip();
        }

        // 주기적으로 교체
        float t = 0f;
        while (true)
        {
            float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            if (t >= intervalSec)
            {
                t = 0f;
                ShowNextTip();
            }
            yield return null;
        }
    }

    void ShowNextTip()
    {
        if (_bag.Count == 0) RefillBag();
        if (_bag.Count == 0) return;

        int idx = Draw();
        if (idx < 0) return;

        string text = db.tips[idx].text;

        if (typewriter != null) typewriter.Play(text);
        else ui?.SetTip(text);

        _lastIdx = idx;
    }

    void RefillBag()
    {
        _bag.Clear();
        if (!db) return;

        var lang = Application.systemLanguage;

        // 조건(언어/태그) 맞는 항목만 가중치만큼 채우기
        for (int i = 0; i < db.tips.Count; i++)
        {
            var tip = db.tips[i];

            if (!string.IsNullOrEmpty(_activeTag) && tip.tag != _activeTag)
                continue;

            // 언어 매칭: 시스템 언어 우선, 없으면 영어 허용
            bool langOK = (tip.language == lang) ||
                          (tip.language == SystemLanguage.English) ||
                          (lang == SystemLanguage.Unknown);

            if (!langOK) continue;

            int count = Mathf.Max(1, Mathf.RoundToInt(tip.weight * 10f));
            for (int c = 0; c < count; c++) _bag.Add(i);
        }

        // 셔플
        for (int i = 0; i < _bag.Count; i++)
        {
            int j = Random.Range(i, _bag.Count);
            (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
        }
    }

    int Draw()
    {
        if (_bag.Count == 0) return -1;

        // 직전과 같은 문구가 나오지 않도록 한 번 더 시도
        if (avoidImmediateRepeat && _bag.Count > 1)
        {
            for (int tries = 0; tries < 3; tries++)
            {
                int pick = Random.Range(0, _bag.Count);
                int idx = _bag[pick];
                if (idx != _lastIdx)
                {
                    // 제거(pop) 후 반환
                    _bag[pick] = _bag[_bag.Count - 1];
                    _bag.RemoveAt(_bag.Count - 1);
                    return idx;
                }
            }
        }

        // 일반 추첨
        int p = Random.Range(0, _bag.Count);
        int result = _bag[p];
        _bag[p] = _bag[_bag.Count - 1];
        _bag.RemoveAt(_bag.Count - 1);
        return result;
    }
}
