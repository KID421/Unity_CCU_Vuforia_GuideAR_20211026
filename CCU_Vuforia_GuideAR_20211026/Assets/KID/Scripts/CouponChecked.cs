using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//檢查優惠券的掃描狀態
public class CouponChecked : MonoBehaviour {

    //兌換按鈕
    public GameObject btn_Exchange;
    //不同狀態的圖 已兌換 已取得未兌換  未取得
    public Sprite[] statusSprtes;
    //目前優惠券的編號
    int currentCoupon;

    void Start() {
        StartChecking();
    }


    public void StartChecking() {
     
        for (int i = 0; i < transform.childCount; i++) {
            //已兌換
            if (PlayerPrefs.GetInt("Coupon" + i) == 2) {
                //print(PlayerPrefs.GetInt("Coupon" + i));
                transform.GetChild(i).GetChild(1).GetComponent<Image>().sprite = statusSprtes[2];
            }
            //已取得
            else if (PlayerPrefs.GetInt("Coupon" + i) == 1) {
                transform.GetChild(i).GetChild(1).GetComponent<Image>().sprite = statusSprtes[1];
            }
            //未取得
            else {
                transform.GetChild(i).GetChild(1).GetComponent<Image>().sprite = statusSprtes[0];
            }
        }
    }

    public void Exchange() {
        //已取得
        PlayerPrefs.SetInt("Coupon" + currentCoupon, 2);
        PlayerPrefs.Save();
    }

    public void ShowCouponBox(int i) {
        currentCoupon = i;
        if (PlayerPrefs.GetInt("Coupon" + i) == 1) {
            btn_Exchange.GetComponent<Button>().interactable = true;
        }
        else {
            btn_Exchange.GetComponent<Button>().interactable = false;
        }
    }

}//End
