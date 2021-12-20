using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//負責整個專案的聲音控制
public class MyAudioManager : MonoBehaviour {

    static MyAudioManager _instance;
    AudioSource myAudioSource;
    public static MyAudioManager Instance {
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
    void Start() {
        myAudioSource = GetComponent<AudioSource>();
    }


    public void PlaySound(AudioClip clip) {
        if (clip) {
            myAudioSource.clip = clip;
            myAudioSource.Play();
        }
    }

}//End
