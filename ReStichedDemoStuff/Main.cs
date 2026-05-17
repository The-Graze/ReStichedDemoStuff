using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using HarmonyLib;
using HighLevelWardrobe;
using TMPro;
using TrixelCreative.TrixelAudio.Core;
using TrixelCreative.TrixelAudio.Data;
using UI;
using UI.Craftbook.CraftbookGeneration.FunctionButtons;
using UI.WardrobeDemoSplash;
using UnityEngine;
using UnityEngine.UI;

namespace ReStichedDemoStuff;

[BepInPlugin(Constants.Name, Constants.Guid, Constants.Version)]
public class Main : BaseUnityPlugin
{
    private static Main? _instance;
    private readonly Harmony _harmony;

    private const float GreenscreenAnimTime = .8f;

    public GameObject? wardrobeToolbarRoot;

    private readonly Vector3
        _greenscreenTargetPos = new(-1.94f, -205.38f, 73.44f),
        _greenscreenStartPos = new(-1.94f, -188.9583f, 73.44f),
        _standStartPos = Vector3.zero,
        _standTargetPos = new(0, -10, 0);


    private SoundEffectAsset? _curtainSound;
    private TrixelAudioSource? _curtainSoundSource;

    private bool _goUp;
    private GameObject? _greenscreen, _customButtonGo, _stuffyStand;

    private Main()
    {
        _harmony = new Harmony(Constants.Guid);
        _harmony.PatchAll();
        _instance = this;
    }

    private void OnDisable()
    {
        _harmony.UnpatchSelf();
    }

    private void AddCustomButton()
    {
        var buttonsContainer = wardrobeToolbarRoot?.transform.GetChild(0).GetChild(4).GetChild(0);

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
    }

    private void ToggleGreenScreen()
    {
        _goUp = !_goUp;
        _curtainSoundSource?.PlayOneShot(_curtainSound!, false);
        _greenscreen!.SetActive(true);
        StartCoroutine(GreenScreenAnim());
    }


    private IEnumerator GreenScreenAnim()
    {
        var greenscreenFrom = _goUp ? _greenscreenStartPos : _greenscreenTargetPos;
        var greenscreenTo = _goUp ? _greenscreenTargetPos : _greenscreenStartPos;

        var standFrom = _goUp ? _standStartPos : _standTargetPos;
        var standTo = _goUp ? _standTargetPos : _standStartPos;

        var elapsed = 0f;
        while (elapsed < GreenscreenAnimTime)
        {
            elapsed += Time.deltaTime;
            _greenscreen!.transform.localPosition = Vector3.Lerp(greenscreenFrom, greenscreenTo, elapsed / GreenscreenAnimTime);
            _stuffyStand!.transform.localPosition = Vector3.Lerp(standFrom, standTo, elapsed / GreenscreenAnimTime);
            yield return null;
        }
        _greenscreen!.transform.localPosition = greenscreenTo;
        _greenscreen!.SetActive(_greenscreen!.transform.localPosition != _greenscreenStartPos);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class Patches
    {
        [HarmonyPatch(typeof(WardrobeSplashController))]
        public abstract class WardrobeSplashPatches
        {
            [HarmonyPatch(nameof(WardrobeSplashController.Awake))]
            [HarmonyPostfix]
            private static void WardrobeSplashAwakePatch(WardrobeSplashController __instance)
            {
                if(!_instance) return;
                
                if(!_instance._curtainSound)
                    _instance._curtainSound = __instance.curtainOpenSound;
                
                if(_instance._curtainSoundSource) return;
                _instance._curtainSoundSource = Instantiate(__instance.audioSource);
                _instance._curtainSoundSource.transform.position = __instance.audioSource.transform.position;
                _instance._curtainSoundSource.transform.rotation = __instance.audioSource.transform.rotation;
            }
            
            [HarmonyPatch(nameof(WardrobeSplashController.ShowEULAPopup))]
            [HarmonyPrefix]
            private static bool EULAPatch(WardrobeSplashController __instance)
            {
                //Skips the check and animates the Splash buttons in
                __instance.StartCoroutine(__instance.AnimateInButtons());
                return false;
            }
        }

        [HarmonyPatch(typeof(WardrobeManager))]
        public abstract class WardrobeManagerPatches
        {
            [HarmonyPatch(nameof(WardrobeManager.SetupWardrobe))]
            [HarmonyPostfix]
            private static void WardrobeSetupPatch(WardrobeManager __instance)
            {
                if(!_instance) return;
                _instance._greenscreen = __instance.transform.GetChild(3).gameObject;
                _instance._greenscreen.transform.localPosition = _instance._greenscreenStartPos;
                _instance._stuffyStand = _instance._greenscreen.transform.parent.GetChild(1).GetChild(3).GetChild(0).gameObject;
            }
        }

        [HarmonyPatch(typeof(UiManager))]
        public abstract class UiManagerPatches
        {
            [HarmonyPatch(nameof(UiManager.InstantiateWardrobeUi))]
            [HarmonyPostfix]
            private static void InstantiateWardrobeUiPatch(UiManager __instance)
            {
                if(!_instance) return;
                _instance.wardrobeToolbarRoot = __instance.WardrobeState.WardrobeHud?.transform.GetChild(2).gameObject;
                _instance.AddCustomButton();
            }
        }
    }
}