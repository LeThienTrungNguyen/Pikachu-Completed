using UnityEngine;

public class BlockInformation : MonoBehaviour
{
    private void Start()
    {
        
    }
    public int x;
    public int y;
    public Block block;
    public void PaintColor(Color color)
    {
        transform.GetComponent<SpriteRenderer>().color = color;
    }
}