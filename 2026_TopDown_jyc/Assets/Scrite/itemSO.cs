using UnityEngine;

[CreateAssetMenu(fileName = "itemso", menuName = "Game/Create item")]
public class itemso : ScriptableObject
{
    [Header("Score Value")]
    public int point = 10;
    public string itemName = string.Empty;
}