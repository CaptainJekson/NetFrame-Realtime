using UnityEngine;
using UnityEngine.EventSystems;

namespace Examples.Scripts.UI
{
    public class ButtonHoldAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [HideInInspector] public bool buttonHeld;

        public void OnPointerDown(PointerEventData eventData)
        {
            buttonHeld = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            buttonHeld = false;
        }
    }
}