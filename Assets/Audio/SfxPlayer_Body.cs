using UnityEngine;
using UnityEngine.Audio;

public class SfxPlayer_Body : MonoBehaviour
{
    [ContextMenu("Check Entries Count")]
    private void _CheckEntries()
    {
        Debug.Log($"[SfxPlayer_Body] {gameObject.name} entries count: " +
                  (entries != null ? entries.Length : 0));
    }

    [System.Serializable]
    public struct SfxEntry
    {
        public string key;     // "dash", "hit", "foot_L" 등
        public AudioClip clip; // 해당 클립
        [Range(0f, 1f)] public float volume; // 이 항목(클립)만의 볼륨
    }

    [Header("Output (Optional)")]
    public AudioMixerGroup outputGroup; // SFX 그룹 연결(없으면 비워도 됨)

    [Header("Clips")]
    public SfxEntry[] entries;          // 키로 찾을 때 사용
    public AudioClip[] randomSet;       // 인자 없이 Play() 호출 시 랜덤

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRandom = new Vector2(1.0f, 1.0f);
    [Tooltip("UI나 고정음=2D, 현장감 있는 소리=3D")]
    public bool play2D = false;

    private AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        if (!_src) _src = gameObject.AddComponent<AudioSource>();

        _src.playOnAwake = false;
        _src.loop = false;
        _src.dopplerLevel = 0f;
        _src.spatialBlend = play2D ? 0f : 1f; // 0=2D, 1=3D,  1f로 유지할 것
        if (outputGroup) _src.outputAudioMixerGroup = outputGroup;
    }

    // 애니메이션 이벤트: 인자 없이 → randomSet에서 한 개
    public void Play()
    {
        if (randomSet == null || randomSet.Length == 0) return;
        var clip = randomSet[Random.Range(0, randomSet.Length)];
        PlayOneShot(clip);
    }

    // 애니메이션 이벤트: int 인자 → entries[index]
    public void PlayByIndex(int index)
    {
        if (entries == null || index < 0 || index >= entries.Length) return;
        var e = entries[index];
        if (e.clip) PlayOneShot(e.clip, e.volume); // ← 엔트리 볼륨 전달
    }

    // 애니메이션 이벤트: string 인자 → key로 찾기
    public void PlayByKey(string key)
    {
        if (entries == null) return;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].key == key)
            {
                var e = entries[i];
                if (e.clip)
                {
                    PlayOneShot(e.clip, e.volume); // ← 엔트리 볼륨 전달
                }
                else
                {
                    Debug.LogWarning($"[SfxPlayer_Body] Clip is missing for key={key} in {gameObject.name}");
                }
                return;
            }
        }
        Debug.LogWarning($"[SfxPlayer_Body] No matching entry found for key={key} in {gameObject.name}");
    }

    private void PlayOneShot(AudioClip clip, float entryVolume = 1f)
    {
        _src.pitch = Random.Range(pitchRandom.x, pitchRandom.y);
        // 최종 볼륨 = 마스터(volume) × 엔트리(entryVolume)
        _src.PlayOneShot(clip, Mathf.Clamp01(volume * (entryVolume <= 0f ? 1f : entryVolume)));
    }

    // 필요 시 런타임 전환
    public void Set2D(bool is2D)
    {
        play2D = is2D;
        if (_src) _src.spatialBlend = is2D ? 0f : 1f;
    }
}

