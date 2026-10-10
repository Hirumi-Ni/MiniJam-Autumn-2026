using UnityEngine;

[CreateAssetMenu(fileName = "NewLeafSO", menuName = "Incremental/Leaf Data")]
public class LeafSO : ScriptableObject
{
    public string leafName = "Jamal Leaf";
    public Sprite leafSprite;
    public int coinAmount = 1;
    public float spawnWeight = 100f;
}