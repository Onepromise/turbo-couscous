using UnityEngine;

[CreateAssetMenu(fileName = "New Minion", menuName = "CHEGG/Minion")]
public class MinionData : ScriptableObject
{
    public string minionName;
    public int health = 3;
    public int attack = 1;
    public int moveRange = 1;
    public Color color = Color.blue;
}