using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemyMasterDebugger : EditorWindow
{
    private Vector2 _leftScrollPos;
    private Vector2 _rightScrollPos;
    private List<string> _stateHistory = new List<string>();
    private EnemyAI _selectedAI;

    [MenuItem("Window/Game Debug/Enemy Master Debugger")]
    public static void ShowWindow() => GetWindow<EnemyMasterDebugger>("Enemy Master");

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // --- 왼쪽 영역: 씬에 있는 모든 EnemyAI 리스트 ---
        DrawEnemyList();

        // --- 오른쪽 영역: 선택된 적의 상세 정보 및 제어 ---
        DrawDetails();

        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying) Repaint();
    }

    private void DrawEnemyList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
        GUILayout.Label("몬스터 리스트", EditorStyles.boldLabel);
        _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos);

        EnemyAI[] allEnemies = GameObject.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            GUI.color = (_selectedAI == enemy) ? Color.cyan : Color.white;
            if (GUILayout.Button($"{enemy.gameObject.name}\n({GetCurrentStateName(enemy)})", GUILayout.Height(40)))
            {
                if (_selectedAI != enemy)
                {
                    _selectedAI = enemy;
                    _stateHistory.Clear(); // 적이 바뀌면 히스토리 초기화
                    _stateHistory.Add($"[{Time.time:F1}s] {enemy.gameObject.name} 추적 시작");
                }
                Selection.activeGameObject = enemy.gameObject;
            }
        }
        GUI.color = Color.white;

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDetails()
    {
        // 오른쪽 영역 시작
        EditorGUILayout.BeginVertical();

        // 1. 아무것도 선택 안 됐을 때 처리
        if (_selectedAI == null)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("왼쪽 리스트에서 적을 선택하면 상세 정보가 표시됨!", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        // 2. 스크롤 뷰 시작 (선택된 적이 있을 때만)
        _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos);

        // --- 기즈모 시각화 제어 섹션 ---
        EditorGUILayout.Space();
        GUILayout.Label("기즈모 시각화 제어", EditorStyles.boldLabel);

        // 버튼들을 가로로 배치하려면 다시 Horizontal을 열어줌
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(_selectedAI.showDebugGizmos ? "개별 기즈모 끄기" : "개별 기즈모 켜기"))
        {
            _selectedAI.showDebugGizmos = !_selectedAI.showDebugGizmos;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("모든 기즈모 끄기"))
        {
            EnemyAI[] allEnemies = GameObject.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var e in allEnemies) e.showDebugGizmos = false;
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal(); // 가로 배치 끝

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 구분선

        // 3. 기존 상세 정보들
        DrawStateBanner();
        DrawStateForceButtons();
        DrawStatSection();
        DrawHistorySection();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical(); // 오른쪽 영역 끝
    }

    private void DrawStateBanner()
    {
        string state = GetCurrentStateName(_selectedAI);
        GUIStyle style = new GUIStyle(EditorStyles.helpBox) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        // 상태 변화 감지 및 히스토리 기록
        if (_stateHistory.Count > 0 && !_stateHistory[_stateHistory.Count - 1].Contains(state))
        {
            _stateHistory.Add($"[{Time.time:F1}s] 상태 변경: {state}");
            if (_stateHistory.Count > 15) _stateHistory.RemoveAt(0); // 최근 15개만 유지
        }

        GUI.backgroundColor = GetStateColor(state);
        GUILayout.Box($"CURRENT: {state}", style, GUILayout.Height(40));
        GUI.backgroundColor = Color.white;
    }

    private void DrawStateForceButtons()
    {
        GUILayout.Label("강제 상태 전환 (Debug Force)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("IDLE")) _selectedAI.ChangeState(_selectedAI.idleState);
        if (GUILayout.Button("CHASE")) _selectedAI.ChangeState(_selectedAI.chaseState);
        if (GUILayout.Button("ATTACK")) _selectedAI.ChangeState(_selectedAI.attackState);
        if (GUILayout.Button("FLEE")) _selectedAI.ChangeState(_selectedAI.fleeState);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("KILL ENEMY", GUILayout.Height(30))) _selectedAI.TakeDamage(9999);
    }

    private void DrawStatSection()
    {
        EditorGUILayout.Space();
        GUILayout.Label("실시간 데이터", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("HP", $"{_selectedAI.currentHP} / {_selectedAI.speciesData?.maxHP}");
        EditorGUILayout.LabelField("용기", $"{_selectedAI.currentCourage:F1} (+{_selectedAI.currentCourage})");
        EditorGUILayout.LabelField("분대 랭크", _selectedAI.mySquadRank.ToString());
        EditorGUILayout.Toggle("플레이어 감지", _selectedAI.playerInRange);
        EditorGUILayout.EndVertical();
    }

    private void DrawHistorySection()
    {
        EditorGUILayout.Space();
        GUILayout.Label("상태 히스토리", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        for (int i = _stateHistory.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(_stateHistory[i]);
        }
        EditorGUILayout.EndVertical();
    }

    private string GetCurrentStateName(EnemyAI ai) => ai.currentState != null ? ai.currentState.GetType().Name : "NULL";

    private Color GetStateColor(string stateName)
    {
        if (stateName.Contains("Chase")) return Color.yellow;
        if (stateName.Contains("Attack")) return new Color(1f, 0.4f, 0.4f);
        if (stateName.Contains("Flee")) return Color.cyan;
        return Color.gray;
    }
}