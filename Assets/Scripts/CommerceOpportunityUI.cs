using System;
using System.Threading.Tasks;
using Unity.Commerce.Backend;
using Unity.Commerce.CommerceOpportunity;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using Walmart.Commerce.Sdk;

public class CommerceOpportunityUI : MonoBehaviour
{
    private AccountManager _accountManager;
    private InputField _inputField;
    private Button _showButton;
    private Text _statusText;
    private bool _sdkReady;

    private async void Awake()
    {
        _accountManager = GetComponent<AccountManager>();
        await InitializeSdk();
    }

    private void Start()
    {
        CreateUI();
    }

    private void CreateUI()
    {
        // Canvas
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Panel background
        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.85f);
        panelRect.anchorMax = new Vector2(0.9f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var layout = panelGo.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.padding = new RectOffset(10, 10, 5, 5);

        // InputField
        var inputGo = new GameObject("InputField");
        inputGo.transform.SetParent(panelGo.transform, false);
        var inputImage = inputGo.AddComponent<Image>();
        inputImage.color = Color.white;
        var inputLayout = inputGo.AddComponent<LayoutElement>();
        inputLayout.flexibleWidth = 1;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(inputGo.transform, false);
        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.supportRichText = false;
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 2);
        textRect.offsetMax = new Vector2(-5, -2);

        var placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(inputGo.transform, false);
        var placeholder = placeholderGo.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f);
        placeholder.text = "Enter Commerce Opportunity name...";
        var phRect = placeholderGo.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(5, 2);
        phRect.offsetMax = new Vector2(-5, -2);

        _inputField = inputGo.AddComponent<InputField>();
        _inputField.textComponent = text;
        _inputField.placeholder = placeholder;

        // Show Button
        var buttonGo = new GameObject("ShowButton");
        buttonGo.transform.SetParent(panelGo.transform, false);
        var btnImage = buttonGo.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 1f);
        var btnLayout = buttonGo.AddComponent<LayoutElement>();
        btnLayout.minWidth = 100;

        var btnTextGo = new GameObject("Text");
        btnTextGo.transform.SetParent(buttonGo.transform, false);
        var btnText = btnTextGo.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Show";
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        var btnTextRect = btnTextGo.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        _showButton = buttonGo.AddComponent<Button>();
        _showButton.onClick.AddListener(OnShowClicked);

        // Status Text
        var statusGo = new GameObject("StatusText");
        statusGo.transform.SetParent(canvasGo.transform, false);
        _statusText = statusGo.AddComponent<Text>();
        _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _statusText.color = Color.white;
        _statusText.fontSize = 14;
        _statusText.alignment = TextAnchor.UpperLeft;
        var statusRect = statusGo.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.1f, 0.75f);
        statusRect.anchorMax = new Vector2(0.9f, 0.85f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
    }

    private async Task InitializeSdk()
    {
        SetStatus("Initializing WalmartSdk...");

        if (!WalmartSdk.Instance.IsInitialized)
        {
            await WalmartSdk.Instance.InitializeAsync();
        }

        SetStatus("WalmartSdk initialized. Signing in...");

        await _accountManager.InitializeAsync();
        var result = await _accountManager.SignInAnonymously();

        if (result.Success)
        {
            bool linked = await WalmartSdk.Instance
                .SetupAuthorizationHeaderAndCheckAccountLinkStatus(
                    "Bearer", AuthenticationService.Instance.AccessToken);
            SetStatus("Ready. Account " + (linked ? "linked" : "not linked") + ".");
            _sdkReady = true;
        }
        else
        {
            SetStatus("Authentication failed. Check Console for details.");
        }
    }

    private void OnShowClicked()
    {
        if (!_sdkReady)
        {
            SetStatus("SDK not ready yet.");
            return;
        }

        string name = _inputField.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            SetStatus("Please enter a Commerce Opportunity name.");
            return;
        }

        _ = ShowCommerceOpportunity(name);
    }

    private async Task ShowCommerceOpportunity(string commerceOpportunityName)
    {
        SetStatus($"Loading '{commerceOpportunityName}'...");

        if (!WalmartSdk.Instance.TryGetCommerceOpportunity(
                commerceOpportunityName, out CommerceOpportunityInstance comOp))
        {
            SetStatus($"Commerce Opportunity '{commerceOpportunityName}' not found.");
            return;
        }

        if (comOp.ProductPackageId == Guid.Empty)
        {
            SetStatus($"'{commerceOpportunityName}' has no linked Product Package.");
            return;
        }

        BackendResponse response = await WalmartSdk.Instance
            .DownloadProductPackageDataAsync(comOp.ProductPackageId);

        if (!response.IsSuccess)
        {
            SetStatus($"Failed to download product data: {response.Status.Code}");
            return;
        }

        if (!WalmartSdk.Instance.TryShowCommerceOpportunity(commerceOpportunityName))
        {
            SetStatus($"Failed to show Commerce Opportunity.");
            return;
        }

        SetStatus("");
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
        if (!string.IsNullOrEmpty(message))
            Debug.Log($"[CommerceUI] {message}");
    }
}