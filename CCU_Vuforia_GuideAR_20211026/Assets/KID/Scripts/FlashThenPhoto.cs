using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//拍照的閃光動畫
public class FlashThenPhoto : MonoBehaviour
{
    public GameObject ARUI;
    public void Photo() {
        ARUI.SetActive(false);
        gameObject.SetActive(true);
        GetComponent<Animator>().Play("Flash");
    }

    public void Shot(){
        gameObject.SetActive(false);
        MainController.Instance.ScreenShot();
    }
}
