using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerControl 
{
    void OnMoveInput(float x, float y);
    void OnJumpInput();
    void OnPaused(bool status);
    void MovementPlayer();

}
