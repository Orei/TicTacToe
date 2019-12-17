using UnityEngine;

// quick and dirty
public class CellData : MonoBehaviour
{
    public int Index { get; set; }
    
    public void SetIndex(int index)
    {
        Index = index;
    }
}