using UnityEngine;

public class itemOB : MonoBehaviour
{
    [SerializeField] itemso data;

    public int GetCoin()
    {
        return data.point;
    }

    public string GetItemName()
    {
        return data.name; 
    }

}
