# Reverse-Stream Copilot Instructions

## Project Overview
This is a Unity 2D action RPG with procedural enemy AI, inventory synergies, and time-based progression systems. Core gameplay revolves around combat with courage-based enemy behavior, item collection with tag-based synergies, and soul store upgrades.

## Architecture Patterns

### Enemy AI System (Assets/Something/MonsterScript/FSM/)
- **Finite State Machine**: All enemies use IEnemyState interface with states: IdleState, ChaseState, AttackState, FleeState, SearchState
- **Courage System**: Enemies have courage stat (1-10) that determines aggression. Damage reduces courage; low courage triggers flee
- **Pack Mentality**: Nearby allies provide courage/speed/cooldown bonuses. Check `allyCheckRadius` in SpeciesData
- **Species Data**: Use ScriptableObjects (SpeciesData) for monster stats. Load via `LoadDataByTag()` in EnemyAI.Awake()
- **Leader System**: Designated leaders provide damage multipliers and command radius effects

### Data Persistence
- **JSON Serialization**: All save data uses JsonUtility.ToJson/File.WriteAllText
- **Persistent Path**: Save files go to Application.persistentDataPath (e.g., "Item_data.json", "SoulShop_Data.json")
- **Singleton Managers**: ItemDataManager, SoulDataManager handle loading/saving with DontDestroyOnLoad

### Inventory & Synergies (Assets/Inventory/)
- **Tag-Based Synergies**: Items have tags; SynergyManager counts tags to activate bonuses
- **SynergyData ScriptableObjects**: Define required tag counts and effects
- **Real-time Updates**: Call SynergyManager.UpdateSynergies() after inventory changes

### UI Systems
- **UI Toolkit**: Primary UI framework for complex interfaces (Soul Store, Inventory)
- **Interaction Flow**: NPCs detect player within `detectRange`, show hint, open UI on KeyManager.Instance.KeyInteract

## Key Conventions

### Naming & Structure
- **Folders**: Assets/FeatureName/Scripts/Subfolders (Data/, Managers/, Systems/, UI/)
- **Singletons**: Static Instance property with Awake() null check and Destroy()
- **ScriptableObjects**: [CreateAssetMenu] with descriptive menuName like "AI/SpeciesData"

### Enemy Behavior
- **State Transitions**: Always call currentState.Exit() before switching states
- **Detection**: Use DetectionArea component for trigger-based player detection
- **Attack Patterns**: SpeciesData.attackType determines special attacks (Buster, Wolf variants)

### Input Handling
- **KeyManager**: Centralized input with customizable keys. Use KeyManager.Instance.KeyInteract instead of KeyCode.E
- **InputSystem**: New Input System actions in InputSystem_Actions.inputactions

## Development Workflows

### Adding New Enemy Type
1. Create SpeciesData asset: Assets > Create > AI > SpeciesData
2. Set speciesTag to match GameObject tag
3. Configure stats, attack patterns, pack bonuses
4. EnemyAI automatically loads data by tag in Awake()

### Adding Synergy
1. Create SynergyData ScriptableObject
2. Set requiredTag and thresholds
3. Add to SynergyManager.allSynergies array
4. Implement effects in SynergyManager.ApplySynergyEffects()

### Save Data Changes
- Always update serializable classes (ItemStats, SoulStats) for new fields
- Call SaveData() after modifications
- Handle file existence in LoadData()

## Common Patterns

### Component References
```csharp
// Cache in Awake, use properties for lazy loading
private Animator ani;
void Awake() {
    ani = GetComponent<MonsterAnimatorController>();
    rb = GetComponent<Rigidbody2D>();
}
```

### Distance Checks
```csharp
// Use Vector2.Distance for 2D games
float distance = Vector2.Distance(transform.position, player.position);
if (distance <= detectRange) { /* interact */ }
```

### Gizmos for Debugging
```csharp
void OnDrawGizmosSelected() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectRange);
}
```

### Coroutines for Timing
```csharp
// Use for cooldowns, animations, delayed actions
private Coroutine cooldownCoroutine;
IEnumerator CooldownTimer() {
    canAttack = false;
    yield return new WaitForSeconds(cooldown);
    canAttack = true;
}
```

## Build & Debug
- **Unity Version**: 2022+ with URP 17.0.4
- **Packages**: Input System 1.13.1, Visual Scripting 1.9.5, UI Toolkit
- **Scenes**: Test scenes in Assets/Scenes/ for isolated testing
- **Debug Logs**: Use Debug.Log with context objects for clickable links

## Integration Points
- **Player Stats**: SoulDataManager.stats affects all player abilities
- **Item Effects**: ItemDataManager.itemData bonuses stack with synergies
- **Enemy Scaling**: Courage system scales difficulty based on player performance
- **UI Events**: SoulStoreUI.Instance.StartInteraction() for NPC dialogs</content>
<parameter name="filePath">c:\Users\tbvjd\Reverse-Stream\.github\copilot-instructions.md