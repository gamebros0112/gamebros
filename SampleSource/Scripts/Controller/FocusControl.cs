using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFocus(bool b)
    {
        //브라우저의 모든 인풋을 잡아먹음 = false
        WebGLInput.captureAllKeyboardInput = b;    
    }
    
}
