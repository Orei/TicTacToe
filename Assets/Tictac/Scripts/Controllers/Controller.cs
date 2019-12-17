using UnityEngine;

public abstract class Controller : ScriptableObject
{
    public abstract void Process(GameManager manager);
}