using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchableObject : MonoBehaviour,IPinchHandler{
    public void OnPinchEnd() {
    }

    public void OnPinchStart() {
    }

    public void OnPinchZoom(float gapDelta) {
        _processPinch(gapDelta);
    }

    void _processPinch(float delta) {
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x+delta, gameObject.GetComponent<RectTransform>().sizeDelta.y+delta);
    }


}//End
