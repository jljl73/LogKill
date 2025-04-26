using UnityEngine;
using UnityEngine.EventSystems;

namespace LogKill.UI
{
    public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject _joystick;
        public void OnPointerDown(PointerEventData eventData)
        {
            _joystick.SetActive(true);
            _joystick.transform.position = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _joystick.SetActive(false);
        }
    }
}
