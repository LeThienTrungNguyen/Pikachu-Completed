using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    public Transform gameOverPanel; 
    public void OnClick()
    {
        gameOverPanel.gameObject.SetActive(false);
        GameController.instance.RestartGame();
    }
}
