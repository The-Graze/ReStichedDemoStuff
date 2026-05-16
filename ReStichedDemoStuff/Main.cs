using System.Collections;
using BepInEx;
using BepInEx.Logging;
using Bootstrap;
using HarmonyLib;
using TMPro;
using UI;
using UI.Craftbook.CraftbookGeneration.FunctionButtons;
using UI.WardrobeDemoSplash;
using UI.WindowManagement;
using UnityEngine;
using UnityEngine.UI;
using Wardrobe;
using Object = UnityEngine.Object;

namespace ReStichedDemoStuff;

[BepInPlugin(Constants.Name, Constants.Guid, Constants.Version)]
public class Main : BaseUnityPlugin
{
    public static Main? Instance;
    public readonly ManualLogSource Logs;
    
    private readonly Harmony _harmony;
    
    private const float GreenscreenAnimTime = .8f;
    public GameObject? wardrobeToolbarRoot;

    private readonly Vector3 
        _greenscreenTargetPos = new(-1.94f, -205.38f, 73.44f),
        _greenscreenStartPos = new(-1.94f, -188.9583f, 73.44f),
        _standStartPos = Vector3.zero,
        _standTargetPos = new(0, -10, 0);

    private bool _addedCustomButton, _goUp;
    private GameObject? _greenscreen, _customButtonGo, _stuffyStand;
    private UiManager? _uiManager;

    private Main()
    {
        _harmony = new Harmony(Constants.Guid);
        _harmony.PatchAll();
        Instance = this;
        Logs = Logger;
    }

    private void OnDisable() =>_harmony.UnpatchSelf();
    


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
                if (!_stuffyStand)
                    if (_greenscreen.transform.parent.GetChild(1).GetChild(3).GetChild(0).name == "ThreadBobbin")
                        _stuffyStand = _greenscreen.transform.parent.GetChild(1).GetChild(3).GetChild(0).gameObject;
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
        var gfrom = _goUp ? _greenscreenStartPos : _greenscreenTargetPos;
        var gto = _goUp ? _greenscreenTargetPos : _greenscreenStartPos;
        
        var pfrom = _goUp ? _standStartPos : _standTargetPos;
        var pto = _goUp ? _standTargetPos : _standStartPos;
        
        var elapsed = 0f;
        while (elapsed < GreenscreenAnimTime)
        {
            elapsed += Time.deltaTime;
            _greenscreen!.transform.localPosition = Vector3.Lerp(gfrom, gto, elapsed / GreenscreenAnimTime);
            _stuffyStand!.transform.localPosition = Vector3.Lerp(pfrom, pto, elapsed / GreenscreenAnimTime);
            yield return null;
        }
        _greenscreen!.transform.localPosition = gto;
    }
    
    public abstract class Patches
    {
        [HarmonyPatch(typeof(WardrobeSplashController))]
        public abstract class WardrobeSplashPatches
        {
            [HarmonyPatch(nameof(WardrobeSplashController.ShowEULAPopup))]
            [HarmonyPrefix]
            // ReSharper disable once InconsistentNaming
            private static bool WindowAwakePatch(WardrobeSplashController __instance)
            {
                __instance.buttonsRowAnimator?.gameObject.SetActive(true);
                return false;
            }
        }
    }
}
