using UnityEngine;
using CHEGG;

[CreateAssetMenu(fileName = "New Minion", menuName = "CHEGG/Minion")]
public class MinionData : ScriptableObject
{
    [Header("Basic Info")]
    public string minionName;
    public MinionType minionType = MinionType.Normal;
    public int manaCost = 1;
    
    [Header("Stats")]
    public int health = 3;
    public int attack = 1;
    
    [Header("Movement")]
    public MovementPattern movementPattern = MovementPattern.Standard8;
    public int moveRange = 1;  // How many squares can move
    
    [Header("Attack")]
    public AttackPattern attackPattern = AttackPattern.Melee8;
    public int attackRange = 1;  // Range for ranged attacks
    
    [Header("Special Rules")]
    [Tooltip("For Villager: moving costs mana")]
    public bool movementCostsMana = false;
    [Tooltip("For abilities like Creeper explosion")]
    public bool diesWhenAttacking = false;
    
    [Header("Visual")]
    public Color color = Color.blue;
    public GameObject prefab;
}