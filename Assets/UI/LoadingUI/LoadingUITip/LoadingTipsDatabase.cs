// LoadingTipsDatabase.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadingTips", menuName = "Game/Loading Tips")]
public class LoadingTipsDatabase : ScriptableObject
{
    [Serializable]
    public class Tip
    {
        [TextArea(2, 5)] public string text;
        [Range(0f, 5f)] public float weight = 1f; // 가중치(자주 나올수록 크게)
        public string tag;   // "controls","world","boss" 등
        public SystemLanguage language = SystemLanguage.Korean; // 다국어용
    }

    public List<Tip> tips = new();
}