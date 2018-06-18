using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIScreenJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private RectTransform _JoyStickArea;

    [SerializeField]
    private RectTransform _JoyStickKnob;

    private bool _IsDown = false;

    void Start()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _IsDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _IsDown = false;
        _Direction = Vector2.zero;
        _OffsetPostion = Vector2.zero;
    }

    private Vector2 _Position;
    private Vector2 _Direction;
    private Vector2 _OffsetPostion;
    public Vector2 Direction { get { return _Direction; } }

    public void OnDrag(PointerEventData eventData)
    {
        if (_IsDown)
        {
            var center = _JoyStickArea.rect.center;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_JoyStickArea, eventData.position, null, out _Position); //point position does not take into account canvas scalar

            _OffsetPostion = (_Position - center);

            Vector2 half_size = new Vector2(_JoyStickArea.rect.width * 0.5f, _JoyStickArea.rect.height * 0.5f);
            _Direction.x = Mathf.Clamp(_OffsetPostion.x / half_size.x, -1, 1);
            _Direction.y = Mathf.Clamp(_OffsetPostion.y / half_size.y, -1, 1);
            _OffsetPostion = new Vector2(_Direction.x * half_size.x, _Direction.y * half_size.y);
            //Debug.LogFormat("{0} {1} {2} {3} {4}", offset, len, _Direction, _Position, _JoyStickArea.rect.center);
        }
        else
        {
            _Direction = Vector2.zero;
            _OffsetPostion = Vector2.zero;
        }
    }

    void Update()
    {
        _JoyStickKnob.anchoredPosition = _OffsetPostion;
        //Debug.LogFormat("{0}", _Direction);
    }
}
