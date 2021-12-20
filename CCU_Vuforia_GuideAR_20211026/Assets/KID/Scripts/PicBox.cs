using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//顯示目前點擊景點的AR照片
public class PicBox : MonoBehaviour
{
   
    public void GetPic() {
        GameObject c= EventSystem.current.currentSelectedGameObject;
        gameObject.GetComponent<Image>().sprite = c.transform.GetChild(1).GetComponent<ChangeToPhoto>().savedSprite; 
    }
}
