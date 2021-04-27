using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMOD.Studio;
using FMODUnity;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{

    [FMODUnity.EventRef]
    public string hoverPath;
    [FMODUnity.EventRef]
    public string pressPath;

    EventInstance hover;
    EventInstance press;

    // Start is called before the first frame update
    void Start()
    {
        hover = RuntimeManager.CreateInstance(hoverPath);
        press = RuntimeManager.CreateInstance(pressPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover.start();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        press.start();
    }
}
