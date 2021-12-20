using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//景點清單狀態檢查
public class LocationChecked : MonoBehaviour{

    public Sprite[] allSprites;
    public bool collectComplete;


    static LocationChecked _instance;
    public static LocationChecked Instance {
        get {
            return _instance;
        }
    }
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }
    }

    void Start(){
        StartChecking();
    }


    public void StartChecking() {
        collectComplete = true;
        for (int i = 0; i < transform.childCount; i++) {
            if (PlayerPrefs.GetInt("Location" + i) == 1) {
                transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = allSprites[1];
            }
            else {
                collectComplete = false;
                transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = allSprites[0];
            }
        }
    }

}//End
