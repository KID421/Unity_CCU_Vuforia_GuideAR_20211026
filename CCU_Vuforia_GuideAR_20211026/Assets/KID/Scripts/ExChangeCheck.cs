using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//最終兌換景點掃描確認
public class ExChangeCheck : MonoBehaviour {

    public Sprite[] allSprites;
    void Start() {
        StartChecking();
    }

    private void OnEnable() {
        StartChecking();
    }


    public void StartChecking() {
        for (int i = 0; i < transform.childCount; i++) {
            if (PlayerPrefs.GetInt("Location" + i) == 1) {
                transform.GetChild(i).GetComponent<Image>().sprite = allSprites[1];
            }
            else {
                transform.GetChild(i).GetComponent<Image>().sprite = allSprites[0];
            }
        }
    }

}//End
