using DanielLochner.Assets.SimpleScrollSnap;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class MainController : MonoBehaviour {

    //Menu視窗上部顯示文字 如:活動資訊、景點導覽等
    public GameObject upperInfoText;
    //全部視窗
    public GameObject canvas_Openning;
    public GameObject canvas_Menu;
    public GameObject canvas_ARCam;
    public GameObject canvas_LocationTracking;
    public GameObject canvas_Activities;
    public GameObject canvas_Coupon;
    public GameObject canvas_Map;
    public GameObject canvas_Exchange;
    //景點清單中的照片預覽
    public GameObject picBox;

    //AR貼圖
    public GameObject[] ARObjects;
    //AR貼圖出生座標點
    public GameObject ARSpawnSpot;
    //AR群組(在拍照截圖時需要隱藏的UI們)
    public GameObject ARUI;
    //控制手勢的物件
    public GameObject touchControl;
    //最終兌換的按鈕
    public Button btnFinalExchange;
    //截圖儲存路徑
    string pcPath;

    //活動頁切換顯示文字
    public string[] txt_Activities;
    public GameObject text_Activities;
    //活動頁拖拉圖片
    public SimpleScrollSnap scrollSnap;

    //APP簡介
    public GameObject appIntro;

    //記錄全部景點照片
    List<int> list_showPic;

    //辨別目前AR掃描到的圖片編號 預設100
    int currentTrackedNum = 100;

    //按鈕音效
    public AudioClip buttonClip;


    //首頁影片播放暫停紐
    public GameObject btn_Play;
    //首頁影片重播紐
    public GameObject btn_Replay;
    //首頁影片按鈕群組
    public GameObject btn_VideoGroup;
    //按鈕持續顯示的時間
    int ani_BtnPlay_Timer = 3;
    //影片播放器
    public VideoPlayer VideoRawImage;
    //播放圖
    public Sprite sprite_Play;
    //暫停圖
    public Sprite sprite_Pause;

    //是否已兌換最終大禮包
    bool FinalGiftGot;

    //最終禮包兌換成功提示
    public GameObject finalExchangedNotice;

    


    //本腳本單例實例
    static MainController _instance;
    public static MainController Instance {
        get {
            return _instance;
        }
    }

    //單例實例
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }
    }

    void Start() {
        //初始狀態關閉VuforiaAR功能
        VuforiaBehaviour.Instance.enabled = false;
        //因為要取得景點的單例實體 預設將Canvas_Location開啟  所以需在Start將其關閉
        canvas_LocationTracking.SetActive(false);
        //取得手機檔案存取權限
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);

        //先將清單填10個空格
        list_showPic = new List<int>();
        for (int j = 0; j < 11; j++) {
            list_showPic.Add(0);
        }
        //將所有按鈕加上音效
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button b in allButtons) {
            if (b.gameObject.tag != "NoneClickSoundButton") {
                b.onClick.AddListener(PlayButtonSound);
            }
        }
        //首頁影片淡出動畫
        StartCoroutine("AnimationBTN_Play");
    }

    //播放聲音
    void PlayButtonSound() {
        MyAudioManager.Instance.PlaySound(buttonClip);
    }

    void Update() {
        //手機按下返回鍵回到上一頁 根據當前頁面位置判斷為上一頁 回地圖 或關閉APP
        if (Input.GetKey(KeyCode.Escape)) {
            if (canvas_Openning.activeSelf) {
                Input.backButtonLeavesApp = true;
            }
            else if (canvas_Map.activeSelf) {
                StartOpenning();
            }
            else {
                Input.backButtonLeavesApp = false;
                if (picBox.activeSelf) {
                    picBox.SetActive(false);
                }
                else {
                    OpenMap();
                }
            }
        }
        //測試用  同時4指模擬全景點收集 5指清除所有紀錄
        if (Input.touchCount == 4) {
            for (int i = 0; i < 11; i++) {
                if (PlayerPrefs.GetInt("Coupon" + i) != 2) {
                    PlayerPrefs.SetInt("Coupon" + i, 1);
                }
                PlayerPrefs.SetInt("Location" + i, 1);
            }
            PlayerPrefs.Save();
            LocationChecked.Instance.collectComplete = true;
        }

        if (Input.touchCount == 5) {
            LocationChecked.Instance.collectComplete = false;
            PlayerPrefs.SetInt("FinalGift", 0);
            ClearPrefs();
        }
        //避免Vuforia拍照時對焦失效   強制把對焦模式改成自動對焦
        //if (!CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO)) {
        //    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        //}
    }

    //開啟AR相機頁面
    public void StartARCam() {
        CloseAllWindows();
        canvas_Menu.SetActive(false);
        OpenAR();
        canvas_ARCam.SetActive(true);
    }

    //開啟優惠券功能
    public void StartCoupon() {
        CloseAllWindows();
        canvas_Coupon.SetActive(true);
        SetTitleText("優惠券");
    }

    //開啟活動頁面
    public void StartActivities() {
        CloseAllWindows();
        canvas_Activities.SetActive(true);
        SetTitleText("AR訊息");
    }

    //開啟景點清單頁面
    public void StartLocationTracking() {
        CloseAllWindows();
        canvas_LocationTracking.SetActive(true);
        SetTitleText("我的足跡");
    }

    //開啟首頁
    public void StartOpenning() {
        CloseAllWindows();
        canvas_Openning.SetActive(true);
        canvas_Menu.SetActive(false);

        VuforiaBehaviour.Instance.enabled = false;
    }

    //進入最終兌換頁面
    public void StartExchange() {
        CloseAllWindows();
        btnFinalExchange.interactable = false;
        LocationChecked.Instance.StartChecking();
        if (LocationChecked.Instance.collectComplete) {
            if (PlayerPrefs.GetInt("FinalGift") == 1) {
                btnFinalExchange.interactable = false;
                finalExchangedNotice.SetActive(true);
            }
            else {
                btnFinalExchange.interactable = true;
            }
        }
        canvas_Exchange.SetActive(true);
    }

    //打開導覽地圖
    public void OpenMap() {
        VuforiaBehaviour.Instance.enabled = false;
        CloseAllWindows();
        canvas_Map.SetActive(true);
        canvas_Openning.SetActive(false);
        canvas_Menu.SetActive(true);
        SetTitleText("嶺東地圖");
    }

    //關閉全部視窗  將Menu打開
    public void CloseAllWindows() {
        canvas_Menu.SetActive(true);

        appIntro.SetActive(false);
        canvas_Map.SetActive(false);
        canvas_Openning.SetActive(false);
        canvas_ARCam.SetActive(false);
        canvas_LocationTracking.SetActive(false);
        canvas_Activities.SetActive(false);
        canvas_Coupon.SetActive(false);
        canvas_Exchange.SetActive(false);

        picBox.SetActive(false);
    }

    //更改Menu上半部的標題文字
    public void SetTitleText(string s) {
        upperInfoText.GetComponent<Text>().text = s;
    }

    //開啟AR功能
    public void OpenAR() {
        AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        AndroidRuntimePermissions.Permission result2 = AndroidRuntimePermissions.RequestPermission("android.permission.READ_EXTERNAL_STORAGE");
        currentTrackedNum = 100;
        VuforiaBehaviour.Instance.enabled = true;
        //CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

    }

    //關閉AR功能
    public void CloseAR() {
        currentTrackedNum = 100;
        VuforiaBehaviour.Instance.enabled = false;
        canvas_Activities.SetActive(true);
        canvas_Menu.SetActive(true);
        SetTitleText("優惠活動");
    }

    //PC的AR拍照功能
    public void ScreenShot() {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ARUI.SetActive(false);
        string folderPath;
        if (currentTrackedNum == 100) {
            folderPath = Directory.GetCurrentDirectory() + @"/ScreenShots/";
        }
        else {
            folderPath = Directory.GetCurrentDirectory() + @"/ScreenShots/" + currentTrackedNum;
        }

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        var screenshotName = "ARScreenShots_" + System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
        pcPath = System.IO.Path.Combine(folderPath, screenshotName);
        ScreenCapture.CaptureScreenshot(pcPath);
        ARUI.SetActive(true);
#endif

#if UNITY_ANDROID
        StartCoroutine("Co_ScreenShot");
#endif
    }

    //Android的AR拍照功能
    IEnumerator Co_ScreenShot() {
        ARUI.SetActive(false);
        if (touchControl.GetComponent<TouchTest>().currentTarget != null) {
            touchControl.GetComponent<TouchTest>().currentTarget.transform.GetChild(0).gameObject.SetActive(false);
        }
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        yield return new WaitForEndOfFrame();
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string myPath;

        if (currentTrackedNum == 100) {
            myPath = GetAndroidExternalStoragePath() + "/ARScreenShots";
        }
        else {
            myPath = GetAndroidExternalStoragePath() + "/ARScreenShots/" + currentTrackedNum;
        }

        if (!Directory.Exists(myPath)) {
            Directory.CreateDirectory(myPath);
        }
        string filePath = Path.Combine(myPath, timeStamp + ".jpg");
        if (Directory.Exists(myPath)) {
            File.WriteAllBytes(filePath, ss.EncodeToPNG());
        }
        ARUI.SetActive(true);
        Destroy(ss);
        yield return new WaitUntil(() => File.Exists(filePath));
        string[] paths = new string[1];
        paths[0] = filePath;
        ScanFile(paths);
        yield return null;
    }



    //取得安卓的路徑  DCIM
    private string GetAndroidExternalStoragePath() {
        if (Application.platform != RuntimePlatform.Android) {
            return Application.persistentDataPath;
        }

        var jc = new AndroidJavaClass("android.os.Environment");
        var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory",
            jc.GetStatic<string>("DIRECTORY_DCIM")).Call<string>("getAbsolutePath");
        return path;
    }

    //要求Android進行圖片刷新掃描
    void ScanFile(string[] path) {
        using (AndroidJavaClass PlayerActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            AndroidJavaObject playerActivity = PlayerActivity.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaObject Conn = new AndroidJavaObject("android.media.MediaScannerConnection", playerActivity, null)) {
                Conn.CallStatic("scanFile", playerActivity, path, null, null);
            }
        }

    }

    //掃描到AR景點的資訊紀錄
    public void TargetFoundSave(int i) {

        if (PlayerPrefs.GetInt("Coupon" + i) != 2) {
            PlayerPrefs.SetInt("Coupon" + i, 1);
        }
        PlayerPrefs.SetInt("Location" + i, 1);
        PlayerPrefs.Save();
        list_showPic[i] = 1;
        currentTrackedNum = i;
    }

    //測試用   清除所有AR掃描紀錄
    public void ClearPrefs() {
        for (int i = 0; i < 20; i++) {
            PlayerPrefs.SetInt("Location" + i, 0);
            PlayerPrefs.SetInt("Coupon" + i, 0);
        }
        PlayerPrefs.SetInt("FinalGift", 0);
    }


    //產生拍照時的可愛貼圖
    public void SpawnARObjects() {
        GameObject c = EventSystem.current.currentSelectedGameObject;
        int i = c.transform.GetSiblingIndex();
        Instantiate(ARObjects[i], ARSpawnSpot.transform.position, Quaternion.identity, canvas_ARCam.transform);
    }

    //清除所有可愛貼圖
    public void ClearARObjects() {
        GameObject[] ARO = GameObject.FindGameObjectsWithTag("ARObject");

        foreach (GameObject o in ARO) {
            Destroy(o);
        }
    }

    //活動頁面的圖片尋覽   控制標示點顯示
    //public void CheckDots() {
    //    for (int i = 0; i < allDots.Length; i++) {
    //        if (i == currentAct) {
    //            allDots[i].GetComponent<UnityEngine.UI.Image>().sprite = pressedDot;
    //        }
    //        else {
    //            allDots[i].GetComponent<UnityEngine.UI.Image>().sprite = oriDot;
    //        }
    //    }
    //}

    //活動頁的圖片切換
    public void ChangeActivities() {
        //img_Activities.GetComponent<UnityEngine.UI.Image>().sprite = tex_Activities[i];
        text_Activities.GetComponent<UnityEngine.UI.Text>().text = txt_Activities[scrollSnap.CurrentPanel];
  
    }

    //首頁學校連結按鈕
    public void OpenSchoolurl() {
        Application.OpenURL("http://www.ltu.edu.tw/");
    }

    //Vuforia AR遺失 掃描圖片時 將目前掃描的紀錄數字改成100
    public void RemoveCurrentTracked() {
        currentTrackedNum = 100;
    }

    //首頁影片按鈕的淡入淡出
    public void VideoBTN() {
        btn_VideoGroup.SetActive(true);
        ani_BtnPlay_Timer = 3;
        btn_Play.GetComponent<Animator>().Play("Default");
        btn_Replay.GetComponent<Animator>().Play("Default");

    }

    //首頁影片的播放暫停控制
    public void BTN_Play() {
        switch (VideoRawImage.isPlaying) {
            case true:
                VideoRawImage.Pause();
                btn_Play.GetComponent<Image>().sprite = sprite_Play;
                break;
            case false:
                VideoRawImage.Play();
                btn_Play.GetComponent<Image>().sprite = sprite_Pause;
                break;
        }

    }
    //首頁影片的重播
    public void BTN_Replay() {
        VideoRawImage.Stop();
        VideoRawImage.time = 0;
        VideoRawImage.Play();
        btn_Play.GetComponent<Image>().sprite = sprite_Pause;
    }

    //首頁影片按鈕的淡出動畫的協程
    IEnumerator AnimationBTN_Play() {
        while (true) {
            if (canvas_Openning.activeSelf) {
                if (ani_BtnPlay_Timer > 0) {
                    ani_BtnPlay_Timer -= 1;
                }
                else if (ani_BtnPlay_Timer <= 0) {
                    btn_Play.GetComponent<Animator>().Play("btn_Play");
                    btn_Replay.GetComponent<Animator>().Play("btn_Play");
                }
                yield return new WaitForSeconds(1);
            }
            yield return null;
        }
    }

    //最終兌換
    public void FinalExchange() {
        if (LocationChecked.Instance.collectComplete) {
            FinalGiftGot = true;
            btnFinalExchange.interactable = false;
            finalExchangedNotice.SetActive(true);
            PlayerPrefs.SetInt("FinalGift", 1);
        }
    }
}//End
