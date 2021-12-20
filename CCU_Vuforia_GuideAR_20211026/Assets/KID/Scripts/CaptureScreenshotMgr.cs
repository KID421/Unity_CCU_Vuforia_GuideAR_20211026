using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Screenshot saves Android phone photo album
/// </summary>
public class CaptureScreenshotMgr : MonoBehaviour {
    public Text text;
    string _name = "";

    /// <summary>
    /// Save the screenshot and refresh the album Android
    /// </summary>
    /// <param name="name">If it's empty, name it by time</param>
    public void CaptureScreenshot() {
        _name = "";
        _name = "Screenshot_" + GetCurTime() + ".png";


#if UNITY_STANDALONE_WIN //PC platform
               // under the editor
       // string path = Application.persistentDataPath + "/" + _name;       
        string path = Application.dataPath + "/" + _name;
        ScreenCapture.CaptureScreenshot(path, 0);
                 Debug.Log("image save address" + path);

#elif UNITY_ANDROID //Android Platform
        StartCoroutine(CutImage(_name));
                 // Show the path on the phone
                 // text.text = "image save address" + Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")) + "/DCIM/Camera/" + _name;
                 text.text = "Image Save Address" + Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")) + "/Screenshot /" + _name;
#endif
    }
    //Screen capture and save
    IEnumerator CutImage(string name) {
        //size of picture     
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        tex.Apply();
        yield return tex;
        byte[] byt = tex.EncodeToPNG();

        string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android"));

        // File.WriteAllBytes(path + "/DCIM/Camera/" + name, byt); //Save to the Camera folder under DCIM/ on Android phones
        File.WriteAllBytes(path + "/screenshot /" + name, byt); //Save to the "Screenshot" folder under File Management on Android Phone      
        string[] paths = new string[1];
        paths[0] = path;
        ScanFile(paths);
    }
    //Refresh the image and display it in the album.
    void ScanFile(string[] path) {
        using (AndroidJavaClass PlayerActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            AndroidJavaObject playerActivity = PlayerActivity.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaObject Conn = new AndroidJavaObject("android.media.MediaScannerConnection", playerActivity, null)) {
                Conn.CallStatic("scanFile", playerActivity, path, null, null);
            }
        }

    }
    /// <summary>
    /// Get the current year, month, day, hour, minute, second, such as 20181001444
    /// </summary>
    /// <returns></returns>
    string GetCurTime() {
        return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
            + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
    }

}