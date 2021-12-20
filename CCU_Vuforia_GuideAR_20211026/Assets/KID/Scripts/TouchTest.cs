
using DigitalRubyShared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//AR拍照中的手勢控制
public class TouchTest : MonoBehaviour {

    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    //private TapGestureRecognizer tripleTapGesture;
    //private SwipeGestureRecognizer swipeGesture;
    //private PanGestureRecognizer panGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    private LongPressGestureRecognizer longPressGesture;

    public GameObject UICam;
    public GameObject currentTarget;

    public GameObject map;
    public GameObject canvas_Map;
    public ScrollRect scroll_Map;

    Vector3 currentPivot;

    void Start() {
        // don't reorder the creation of these :)
        //CreatePlatformSpecificViewTripleTapGesture();
        //CreateDoubleTapGesture();
        CreateTapGesture();
        //CreateSwipeGesture();
        //CreatePanGesture();
        CreateScaleGesture();
        CreateRotateGesture();
        CreateLongPressGesture();

        // pan, scale and rotate can all happen simultaneously
        //panGesture.AllowSimultaneousExecution(scaleGesture);
        //panGesture.AllowSimultaneousExecution(rotateGesture);
        scaleGesture.AllowSimultaneousExecution(rotateGesture);

        // prevent the one special no-pass button from passing through,
        //  even though the parent scroll view allows pass through (see FingerScript.PassThroughObjects)
    }

    // void Update() {
    //    if (Input.touchCount != 0) {
    //        touched = true;
    //    }
    //    if (!touched) {
    //        if (canvas_Map.activeSelf) {
    //            if (map.transform.parent.localScale.IsLesserOrEqual(Vector3.one)) {
    //                map.transform.parent.localScale = Vector3.one;
    //                map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
    //            }
    //            scroll_Map.enabled = true;
    //        }
    //    }
    //}

    private void ScaleGestureCallback(GestureRecognizer gesture) {
        if (gesture.State == GestureRecognizerState.Began) {
            if (canvas_Map.activeSelf) {
                //currentPivot = (UICam.GetComponent<Camera>().ScreenToViewportPoint(new Vector3(scaleGesture.CurrentTrackedTouches[0].X
                //    , scaleGesture.CurrentTrackedTouches[0].Y,0))
                //    + UICam.GetComponent<Camera>().ScreenToViewportPoint(new Vector3(scaleGesture.CurrentTrackedTouches[1].X
                //    , scaleGesture.CurrentTrackedTouches[2].Y, 0))) / 2;
                //map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(currentPivot.x, currentPivot.y);
                //UICam.GetComponent<Camera>().rect = new Rect(0, map.transform.parent.GetComponent<RectTransform>().rect.yMax, UICam.GetComponent<Camera>().pixelWidth, map.transform.parent.GetComponent<RectTransform>().rect.yMin); 
                currentPivot = (Input.GetTouch(0).position+ Input.GetTouch(1).position) / 2;
                currentPivot = UICam.GetComponent<Camera>().ScreenToViewportPoint(currentPivot);
                map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(currentPivot.x, currentPivot.y);
            }
        }
            if (gesture.State == GestureRecognizerState.Executing) {
            if (currentTarget != null) {
                currentTarget.transform.localScale *= scaleGesture.ScaleMultiplier;
            }
            if (canvas_Map.activeSelf) {
                scroll_Map.enabled = false;
             
                if (map.transform.parent.localScale.IsLesserOrEqual(Vector3.one)) {
                    map.transform.parent.localScale = Vector3.one;
                    map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                }
                //if (scroll_Map.verticalNormalizedPosition>=1f|| scroll_Map.horizontalNormalizedPosition>=1f) {
                //    scroll_Map.normalizedPosition = new Vector2( 0.5f,0.5f);
                //}        
                //Vector3 pos = new Vector3(scaleGesture.StartFocusX, scaleGesture.StartFocusY, 0.0f);
                map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(currentPivot.x, currentPivot.y);
                map.transform.parent.localScale *= scaleGesture.ScaleMultiplier;
            }
        }

        if (gesture.State == GestureRecognizerState.Ended) {
            if (canvas_Map.activeSelf) {
                if (map.transform.parent.localScale.IsLesserOrEqual(Vector3.one)) {
                    map.transform.parent.localScale = Vector3.one;
                    map.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                }
                scroll_Map.enabled = true;
                //UICam.GetComponent<Camera>().rect = new Rect(0, 0, UICam.GetComponent<Camera>().pixelWidth, UICam.GetComponent<Camera>().pixelHeight);
            }
        }
        else {
            longPressGesture.Reset();
        }
    }


    private void CreateScaleGesture() {
        scaleGesture = new ScaleGestureRecognizer();
        scaleGesture.StateUpdated += ScaleGestureCallback;
        FingersScript.Instance.AddGesture(scaleGesture);
    }



    private void LongPressGestureCallback(GestureRecognizer gesture) {
        if (gesture.State == GestureRecognizerState.Began) {
            BeginDrag(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Executing) {
            DragTo(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Failed || gesture.State == GestureRecognizerState.Ended) {
            EndDrag(longPressGesture.VelocityX, longPressGesture.VelocityY);
        }
    }

    private void CreateLongPressGesture() {
        longPressGesture = new LongPressGestureRecognizer();
        longPressGesture.MaximumNumberOfTouchesToTrack = 1;
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    private void BeginDrag(float screenX, float screenY) {
        Vector3 pos = new Vector3(screenX, screenY, 0.0f);
        pos = UICam.GetComponent<Camera>().ScreenToWorldPoint(pos);
        RaycastHit2D hit = Physics2D.CircleCast(pos, 1.0f, Vector2.zero);
        //scroll_Map.enabled = true;
        if (hit.transform != null) {
            if (hit.transform.gameObject.tag == "ARObject") {
                if (currentTarget != null) {
                    currentTarget.transform.GetChild(0).gameObject.SetActive(false);
                    currentTarget = null;
                }
                currentTarget = hit.transform.gameObject;
                currentTarget.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else {
            longPressGesture.Reset();
        }
    }

    private void DragTo(float screenX, float screenY) {
        if (currentTarget == null) {
            return;
        }

        Vector3 pos = new Vector3(screenX, screenY, 0.0f);
        pos = UICam.GetComponent<Camera>().ScreenToWorldPoint(pos);
        currentTarget.GetComponent<Rigidbody2D>().MovePosition(pos);

    }

    private void EndDrag(float velocityXScreen, float velocityYScreen) {
        if (currentTarget == null) {
            return;
        }
        Vector3 origin = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(velocityXScreen, velocityYScreen, 0.0f));
        Vector3 velocity = (end - origin);
        currentTarget.GetComponent<Rigidbody2D>().velocity = velocity;
        //currentTarget = null;
    }

    private void RotateGestureCallback(GestureRecognizer gesture) {
        if (gesture.State == GestureRecognizerState.Executing) {
            currentTarget.transform.Rotate(0.0f, 0.0f, rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);
        }
    }

    private void CreateRotateGesture() {
        rotateGesture = new RotateGestureRecognizer();
        rotateGesture.StateUpdated += RotateGestureCallback;
        FingersScript.Instance.AddGesture(rotateGesture);
    }



    private void TapGestureCallback(GestureRecognizer gesture) {
        if (gesture.State == GestureRecognizerState.Ended) {
            if (currentTarget) {
                currentTarget.transform.GetChild(0).gameObject.SetActive(false);
            }
            if (currentTarget != null) {
                currentTarget = null;
            }

        }
    }

    private void CreateTapGesture() {
        tapGesture = new TapGestureRecognizer();
        tapGesture.StateUpdated += TapGestureCallback;
        tapGesture.RequireGestureRecognizerToFail = null;
        FingersScript.Instance.AddGesture(tapGesture);
    }




}//End

