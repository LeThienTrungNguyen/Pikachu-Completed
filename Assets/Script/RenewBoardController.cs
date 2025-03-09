using UnityEngine;

public class RenewBoardController : MonoBehaviour
{
    private void OnMouseDown()
    {
        if(GameController.life > 0)
        {
            GameController.instance.ChangeLivingBlocks();
            GameController.life -= 1;
            GameController.instance.UpdateLife(GameController.life);
        }
    }
}
