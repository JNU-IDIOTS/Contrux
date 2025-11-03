#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

public class WeaponPrefabGenerator : EditorWindow
{
    [MenuItem("Tools/무기 프리팹 생성")]
    public static void ShowWindow()
    {
        GetWindow<WeaponPrefabGenerator>("프리팹 생성기");
    }

    // 기본 설정 값
    string weaponName = "사자의 창";
    Sprite icon;
    WeaponGrade grade = WeaponGrade.Epic;
    WeaponType type = WeaponType.Spear;
    int level = 30;
    string description = "이 무기는 매우 날카롭다.";

    int price = 1000;
    int sellprice = 500;

    int reqHp, reqStr, reqDex, reqInt, reqLuk = 0;

    float power = 100f;
    float speed = 1.0f;
    float critChance = 8f;
    float critDamage = 180f;
    float range = 3.0f;

    private void OnGUI()
    {

        GUILayout.Label("기본 정보", EditorStyles.boldLabel);
        weaponName = EditorGUILayout.TextField("무기 이름", weaponName);
        grade = (WeaponGrade)EditorGUILayout.EnumPopup("등급", grade);
        type = (WeaponType)EditorGUILayout.EnumPopup("무기 종류", type);
        level = EditorGUILayout.IntSlider("레벨", level, 10, 50);
        icon = (Sprite)EditorGUILayout.ObjectField("무기 이미지 (Sprite)", icon, typeof(Sprite), false);
        description = EditorGUILayout.TextField("무기 설명", description);

        GUILayout.Space(10);
        GUILayout.Label("상점 정보", EditorStyles.boldLabel);
        price = EditorGUILayout.IntField("구매가", price);
        sellprice = EditorGUILayout.IntField("판매가", sellprice);

        GUILayout.Space(10);
        GUILayout.Label("요구 능력치", EditorStyles.boldLabel);
        reqHp = EditorGUILayout.IntField("체력", reqHp);
        reqStr = EditorGUILayout.IntField("힘", reqStr);
        reqDex = EditorGUILayout.IntField("민첩", reqDex);
        reqInt = EditorGUILayout.IntField("지력", reqInt);
        reqLuk = EditorGUILayout.IntField("운", reqLuk);

        GUILayout.Space(10);
        GUILayout.Label("전투 속성", EditorStyles.boldLabel);
        power = EditorGUILayout.FloatField("공격력(마법 공격력)", power);
        speed = EditorGUILayout.FloatField("공격속도", speed);
        critChance = EditorGUILayout.FloatField("크리티컬 확률", critChance);
        critDamage = EditorGUILayout.FloatField("크리티컬 데미지", critDamage);
        range = EditorGUILayout.FloatField("무기 사거리", range);

        if (GUILayout.Button("프리팹 생성"))
        {
            CreateWeaponPrefab();
        }
    }

    void CreateWeaponPrefab()
    {
        // (1) UI용 이름 먼저 검증
        if (string.IsNullOrWhiteSpace(weaponName))
        {
            Debug.LogError("무기 이름(weaponName)을 입력해야 합니다.");
            return;
        }

        // (2) 드랍 추적용 이름 = 고정 포맷
        string baseName = $"{grade}_{type}_{level}";
        string folder = "Assets/PreFab/GeneratedWeaponPrefabs";
        Directory.CreateDirectory(folder);

        // (3) 파일명 중복 검사 및 고유 파일 경로 확보
        string path = $"{folder}/{baseName}.prefab";
        int counter = 1;
        while (File.Exists(path))
        {
            path = $"{folder}/{baseName}_{counter}.prefab";
            counter++;
        }

        // (4) GameObject는 항상 baseName 유지
        GameObject obj = new GameObject(baseName);

        var renderer = obj.AddComponent<SpriteRenderer>();
        if (icon != null) renderer.sprite = icon;
        obj.tag = "Weapon";

        // (5) WeaponPrefabData 생성
        var groundItem = obj.AddComponent<GroundItem>();
        groundItem.data = new WeaponPrefabData
        {
            weaponName = weaponName,  // <- UI/툴팁용 이름
            grade = grade,
            type = type,
            level = level,
            icon = icon,
            description = description,
            price = price,
            sellprice = sellprice,
            requiredHp = reqHp,
            requiredStr = reqStr,
            requiredDex = reqDex,
            requiredInt = reqInt,
            requiredluk = reqLuk,
            attackPower = power,
            attackSpeed = speed,
            critChance = critChance,
            critDamage = critDamage,
            range = range
        };

        var col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // (6) 프리팹 저장
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);

        Debug.Log($"프리팹 생성 완료: {path}");
    }

}
