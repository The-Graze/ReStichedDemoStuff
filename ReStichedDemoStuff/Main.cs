using System;
using System.Collections;
using BepInEx;
using TMPro;
using UI;
using UI.Craftbook.CraftbookGeneration.FunctionButtons;
using UnityEngine;
using UnityEngine.UI;
using Wardrobe;

namespace ReStichedDemoStuff;


[BepInPlugin(Constants.Name, Constants.Guid, Constants.Version)]
public class Main : BaseUnityPlugin
{
    private UiManager? _uiManager;
    private bool _addedCustomButton, _goUp;
    public GameObject? wardrobeToolbarRoot;
    private GameObject? _greenscreen, _cutstomButtonGo;

    private readonly Vector3 _greenscreenTargetPos = new(-1.94f, -205.38f, 73.44f), 
        _greenscreenStartPos = new (-1.94f, -188.9583f, 73.44f);

    private const float GreenscreenAnimTime = .8f;
    

    private void Update()
    {
        try
        {
            if (!_uiManager)
                _uiManager = FindFirstObjectByType<UiManager>();
            if (!wardrobeToolbarRoot)
                wardrobeToolbarRoot = _uiManager.WardrobeState.WardrobeHud!.transform.GetChild(2).gameObject;
            if (!_greenscreen)
                _greenscreen = FindFirstObjectByType<WardrobeOrnamentRenderer>().transform.GetChild(3).gameObject;
        }
        catch { /*IGNORE*/ }

        if (_addedCustomButton || !_uiManager || !wardrobeToolbarRoot || !_greenscreen) return;
        AddCustomButton(wardrobeToolbarRoot!);

        _greenscreen.transform.localPosition = _greenscreenStartPos;
        _greenscreen.SetActive(true);
    }

    private void AddCustomButton(GameObject toolbarRoot)
    {
        try
        {
            var buttonsContainer = toolbarRoot.transform.GetChild(0).GetChild(4).GetChild(0);

            if (!buttonsContainer) return;
        
            var buttonPrefab = buttonsContainer.GetComponent<FunctionButtonsBehaviour>().FunctionButtonPrefab;
            
             _cutstomButtonGo = Instantiate(buttonPrefab, buttonsContainer.transform);

             _cutstomButtonGo.transform.GetChild(1).gameObject.SetActive(false);
             _cutstomButtonGo.transform.GetChild(2).gameObject.SetActive(false);
             
             _cutstomButtonGo!.gameObject.SetActive(false);
             _cutstomButtonGo!.GetComponentInChildren<TextMeshProUGUI>().text = "G";
             _cutstomButtonGo!.gameObject.SetActive(true);
             
            var customButton = _cutstomButtonGo.GetComponentInChildren<Button>();
            customButton.onClick.AddListener(ToggleGreenScreen);
        }
        catch (Exception e)
        {
            Logger.LogError($"Messed up while setting up button, lying about making one {e.Message}");
        }
        _addedCustomButton = true;
    }

    private void ToggleGreenScreen()
    { 
        _goUp = !_goUp;
        StartCoroutine(GreenScreenAnim());
    } 

        
    private IEnumerator GreenScreenAnim()
    {
        var from = _goUp ? _greenscreenStartPos : _greenscreenTargetPos;
        var to   = _goUp ? _greenscreenTargetPos : _greenscreenStartPos;
        var elapsed = 0f;
        while (elapsed < GreenscreenAnimTime)
        {
            elapsed += Time.deltaTime;
            _greenscreen!.transform.localPosition = Vector3.Lerp(from, to, elapsed / GreenscreenAnimTime);
            yield return null;
        }
        _greenscreen!.transform.localPosition = to;
    }
}