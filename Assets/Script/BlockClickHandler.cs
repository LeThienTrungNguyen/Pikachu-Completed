using UnityEngine;

public class BlockClickHandler : MonoBehaviour
{
    // Tham chiếu tới thông tin của Block
    private BlockInformation blockInfo;
    public GameController gameController;
    private void Awake()
    {
        blockInfo = GetComponent<BlockInformation>();
        gameController = GameObject.FindAnyObjectByType<GameController>();
    }

    private void OnMouseDown()
    {
        if (blockInfo != null)
        {
            Debug.Log($"Clicked on Block at ({blockInfo.x}, {blockInfo.y}) with type: {blockInfo.block.name}");
            HandleBlockClick();
        }
    }

    void HandleBlockClick()
    {
        // Xử lý khi click vào block ở đây
        // Ví dụ: Đổi màu block khi click
        if (gameController.currentBlockChoose[0] == null)
        {
            gameController.currentBlockChoose[0] = GetComponent<BlockInformation>();
            gameController.currentBlockChoose[0].PaintColor(Color.green);
            gameController.PlayAudio(GameController.instance.click);
        }else
        {
            gameController.currentBlockChoose[1] = GetComponent<BlockInformation>();
            gameController.CheckPairBlockChoosen();
        }
    }
}
