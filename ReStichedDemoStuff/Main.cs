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
    private const float GreenscreenAnimTime = .8f;
    public GameObject? wardrobeToolbarRoot;

    private readonly Vector3 _greenscreenTargetPos = new(-1.94f, -205.38f, 73.44f),
        _greenscreenStartPos = new(-1.94f, -188.9583f, 73.44f);

    private bool _addedCustomButton, _goUp;
    private GameObject? _greenscreen, _customButtonGo;
    private UiManager? _uiManager;


    private void Update()
    {
        try
        {
            if (!_uiManager)
                _uiManager = FindFirstObjectByType<UiManager>();
            if (!wardrobeToolbarRoot)
                wardrobeToolbarRoot = _uiManager.WardrobeState.WardrobeHud!.transform.GetChild(2).gameObject;
            if (!_greenscreen)
            {
                _greenscreen = FindFirstObjectByType<WardrobeOrnamentRenderer>().transform.GetChild(3).gameObject;
                _greenscreen.transform.localPosition = _greenscreenStartPos;
            }
            else
            {
                _greenscreen!.SetActive(_greenscreen!.transform.localPosition != _greenscreenStartPos);
            }
        }
        catch
        {
            //IGNORE: I only know I can ignore any error's here as I am waiting for the UI and Wardrobe Scene to exist, and I am lazy to hook it up to the UI changes or scene load as each UI has its own callbacks and instance's
        } 

        if (_addedCustomButton || !_uiManager || !wardrobeToolbarRoot || !_greenscreen) return;
        AddCustomButton(wardrobeToolbarRoot!);
    }

    private void AddCustomButton(GameObject toolbarRoot)
    {
        var buttonsContainer = toolbarRoot.transform.GetChild(0).GetChild(4).GetChild(0);

        if (!buttonsContainer) return;

        var buttonPrefab = buttonsContainer.GetComponent<FunctionButtonsBehaviour>().FunctionButtonPrefab;

        _customButtonGo = Instantiate(buttonPrefab, buttonsContainer.transform);

        _customButtonGo.transform.GetChild(1).gameObject.SetActive(false);
        _customButtonGo.transform.GetChild(2).gameObject.SetActive(false);

        //for some reason I need to do this to refresh the Text, prob something with the custom rendering on UI they do
        _customButtonGo!.gameObject.SetActive(false);
        _customButtonGo!.GetComponentInChildren<TextMeshProUGUI>().text = "G";
        _customButtonGo!.gameObject.SetActive(true);

        var customButton = _customButtonGo.GetComponentInChildren<Button>();
        customButton.onClick.AddListener(ToggleGreenScreen);

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
        var to = _goUp ? _greenscreenTargetPos : _greenscreenStartPos;
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