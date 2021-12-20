using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//用來顯示景點清單的AR拍照縮圖
public class ChangeToPhoto : MonoBehaviour {

    //目前景點的排序
    int picIndex;
    //讀取圖片的路徑
    DirectoryInfo dir;
    //預設要顯示的圖片
    public Sprite savedSprite;

    private void Start() {
        savedSprite = GetComponent<Image>().sprite;
        picIndex = transform.parent.GetSiblingIndex();
    }

    private void OnEnable() {
        picIndex = transform.parent.GetSiblingIndex();
#if UNITY_EDITOR
        string folderPath = Directory.GetCurrentDirectory() + "/ScreenShots/" + picIndex;
        if (Directory.Exists(folderPath)) {
            dir = new DirectoryInfo(folderPath);
            StartCoroutine("GetT");
        }
#endif
#if UNITY_ANDROID
        string myPath = GetAndroidExternalStoragePath() + "/ARScreenShots/" + picIndex;
        if (Directory.Exists(myPath)) {
            dir = new DirectoryInfo(myPath);
            StartCoroutine("GetT");         
        }
#endif
    }

    IEnumerator GetT() {
        List<FileInfo> info = dir.GetFiles("*.*").ToList();
        if (info.Count == 0) {
            savedSprite = GetComponent<Image>().sprite;
            yield return null;
        }
        info.OrderByDescending(x => x);
        string filePath = info[info.Count - 1].FullName;
        byte[] fileData;
        if (File.Exists(filePath)) {
            fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(128, 128);
            tex.LoadImage(fileData);
            Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            gameObject.GetComponent<Image>().sprite = s;
            savedSprite = s;
        }
        else {
            savedSprite = GetComponent<Image>().sprite;
        }
        yield return null;
    }

    private string GetAndroidExternalStoragePath() {
        if (Application.platform != RuntimePlatform.Android)
            return Application.persistentDataPath;

        var jc = new AndroidJavaClass("android.os.Environment");
        var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory",
            jc.GetStatic<string>("DIRECTORY_DCIM"))
            .Call<string>("getAbsolutePath");
        return path;
    }
}//End
