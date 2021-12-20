using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour,ISingleFingerHandler{

    public void OnSingleFingerDown(Vector2 position) {

    }

    public void OnSingleFingerDrag(Vector2 delta) {
        _processSwipe(delta);
    }

    public void OnSingleFingerUp(Vector2 position) {

    }

    void Start(){
        
    }

    
    void Update(){
        
    }

    private void _processSwipe(Vector2 screenTravel) {
        transform.Translate(screenTravel);
         }


}//End
