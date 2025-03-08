using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapButtonHandler : MonoBehaviour
{
    public Animator Animator;
    public ButtonType ButtonType;
    public Image ButtonImage;

    public event Action _tap;
    public event Action _tap_sound;

    private bool _button_stop_down = false;
    private bool _button_exit = false;
    private bool _was_action_tap = false;
    private bool _block_button = false;

    private Vector3 downMousePosition;

    private bool _switch_status = false;

    public bool Interactable
    {
        get
        {
            return _block_button;
        }
        set
        {
            if (value)
            {
                GetComponent<PolygonCollider2D>().enabled = true;
                //ButtonImage.color = new Color(1, 1 , 1);
            }
            else
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                //ButtonImage.color = new Color(0.7f, 0.7f, 0.7f);
            }
            _block_button = !value;
        }
    }

    private void OnMouseDown()
    {
        if (_block_button)
            return;
            
        if (ButtonType == ButtonType.DownEvent)
        {
            if (Animator != null)
                Animator.SetInteger("cntrl", 1);
            _tap();
            _tap_sound();
        }
            
        else if(ButtonType==ButtonType.DownUpEvent)
        {
            downMousePosition = Input.mousePosition;
            Animator.SetInteger("cntrl", 1);
            _block_button=true;
        }
        else if (ButtonType == ButtonType.Switch)
        {
            UpdateStatus(!_switch_status);
            _tap();
            _tap_sound();
        }
    }

    private void OnMouseUp()
    {
        if (ButtonType == ButtonType.DownUpEvent)
        {
            if (Vector3.Distance(Input.mousePosition, downMousePosition) > 70)
            {
                _button_exit = true;
            }
            else
            {
                _tap_sound();
            }
            StartCoroutine("WaitUpButton");
        }

    }

    public void UpdateStatus(bool status)
    {
        if (ButtonType == ButtonType.Switch)
        {
            _switch_status = status;
            if (_switch_status)
                Animator.SetInteger("cntrl", 1);
            else
                Animator.SetInteger("cntrl", 2);
        }
    }

    public void ChangeSprite(Sprite sprite)
    {
        ButtonImage.sprite = sprite;
    }

    public void ButtonStopDown() // вызывается в конце анимации вжатия 
    {
        _button_stop_down = true;
    }
    public void ButtonStopUp() // вызывается в конце анимации отжатия 
    {
        _block_button = false;
        if (_was_action_tap)
        {
            _tap();
        }
        _button_stop_down = false;
        _button_exit = false;
        _was_action_tap = false;
}
    
    private IEnumerator WaitUpButton()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
            if (_button_stop_down && !_button_exit)
            {
                Animator.SetInteger("cntrl", 2);
                _was_action_tap = true;
                break;
            }
            else if(_button_stop_down)
            {
                Animator.SetInteger("cntrl", 2);
                _was_action_tap = false;
                break;
            }
        }
    }
}
public enum ButtonType : int
{
    DownEvent,
    DownUpEvent,
    Switch
}
