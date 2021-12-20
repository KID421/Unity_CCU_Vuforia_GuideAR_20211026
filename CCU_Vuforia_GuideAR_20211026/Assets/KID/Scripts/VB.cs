using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AR辨識後出現的控制按鈕
public class VB : MonoBehaviour {


    public GameObject myModel;


    void Update() {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject == gameObject) {
                    switch (myModel.activeSelf) {
                        case true:
                            myModel.SetActive(false);
                            break;
                        case false:
                            myModel.SetActive(true);
                            break;
                    }
                }
            }
        }
    }
}//End
