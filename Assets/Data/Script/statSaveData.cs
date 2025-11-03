using System.Collections.Generic;
using UnityEngine;


// JSON 저장을 위한 순수 데이터 구조체
[System.Serializable]
public class statSaveData 
{
    public int health = 0;
    public int strength = 0;
    public int dexterity = 0;
    public int intelligence = 0;
    public int luck = 0;
    public int remainingStatPoints = 5;
    public int totalStatPoints;
}

