# Step-by-Step Guide: Walmart Commerce SDK in BasicGame

---

## Step 0: Create Unity Project in the Editor and import packages from the Asset Store

https://assetstore.unity.com/packages/sdk/unity-commerce-sdk-7265475

https://assetstore.unity.com/packages/sdk/walmart-immersive-commerce-sdk-2185813



## Step 1: Add the Authentication Package


Open `Packages/manifest.json` in a text editor and add this line inside `"dependencies"`:

```json

"com.unity.services.authentication": "3.3.3",

```

Add it right after the `"com.unity.commerce-sdk"` line. Save the file. Unity will auto-resolve it when you switch back to the Editor.

---

## Step 2: Link Project to Unity Cloud

1. In Unity Editor, go to **Edit > Project Settings > Services**

2. Click **Link to Unity Cloud** (or select your organization if already linked)

3. Choose your Unity Organization

4. This sets the `cloudProjectId` and `organizationId` that the SDK needs at runtime

---

## Step 3: Create the Data Store Assets

1. Go to **Window > Commerce Opportunity**

2. The first time you open it, an error may appear — that's normal. Close the window.

3. Check in the **Project** window: `Assets/Resources/Commerce Opportunity Editor/` should now contain `CommerceOpportunityDatastore.asset`

---

## Step 4: Configure CommerceOpportunityDatastore

1. In the **Project** window, navigate to `Assets/Resources/Commerce Opportunity Editor/`

2. Select `CommerceOpportunityDatastore`

3. In the **Inspector**, set these fields:

    - **Organization Id** — your Unity Organization ID (find it in Unity Dashboard > organization menu > Manage Organization)

    - **Environment Id** — your environment ID (Unity Dashboard > your project > Environments tab)

    - **Runtime Environment** — drag `RuntimeEnvDetails` from `Packages/Walmart Immersive Commerce SDK/AddressableAssets/RuntimeEnvironments/`

    - **Editor Environment** — drag `CommerceOpportunityEditorEnv` from `Packages/Walmart Immersive Commerce SDK/AddressableAssets/RuntimeEnvironments/`

---

## Step 5: Create Remaining Config Assets

1. Go to **Window > Commerce Opportunity** again

2. This time it should open without errors and auto-create two additional assets in `Assets/Resources/Commerce Opportunity Editor/`:

    - `ConfigDatastore.asset`

    - `ProductPackagesDatastore.asset`

---

## Step 6: Create a Commerce Opportunity

1. In the **Commerce Opportunity** window, create a new Commerce Opportunity (give it a name you'll remember, e.g. `"My ComOp 01"`)

2. Click **Sync** to pull Product Packages from the Unity Dashboard

3. Link a Product Package to your Commerce Opportunity

> You must have already created a Product Package in the \[Unity Dashboard Immersive Commerce setup guide](https://cloud.unity.com/home/organizations/default/projects/default/immersive-commerce-sdk/setup-guide) before this step.

---

## Step 7: Configure ARSConfiguration

This is required for Walmart account linking and checkout, but **not** for viewing products.

1. Select `Assets/Resources/Unity Immersive Commerce/ARSConfiguration` in the Project window

2. In the Inspector, set: 
    - **Base Url** : `https://cloud-code.services.api.unity.com/v1/projects/`
    - **Relay Service Data** : add headers **Content-Type** and **Accept**, both with value : **application/json**

---

## Step 8: Set Up Addressables

The SDK uses Addressables to load its UI screens, themes, fonts, and runtime environments. Your project might not have Addressables initialized yet.

1. Go to **Window > Asset Management > Addressables > Groups**

2. If prompted, click **Create Addressables Settings** — this creates `Assets/AddressableAssetsData/`

3. You need to manually add entries pointing to the SDK assets. Do this:

    - In the Project window, navigate to Packages/Walmart Immersive Commerce SDK/AddressableAssets/AddressableAssetsData/

    - You should see Walmart Group.asset there — drag it into the Addressables Groups window

    - Then navigate to Packages/Commerce SDK/Runtime/UI/AddressableAssets/AddressableAssetsData/AssetGroup/

    - You should see CommerceOpportunity Group.asset there — drag it into the Addressables Groups window

4. Select **all entries** inside `CommerceOpportunity Group`, right-click, and choose **Move to > Default Local Group**

5. Select **all entries** inside `Walmart Group`, right-click, and choose **Move to > Default Local Group**

6. Right-click the now-empty `CommerceOpportunity Group` and delete it

7. Right-click the now-empty `Walmart Group` and delete it

8. In the top menu bar of the window, go to Tools > Labels (or click the Labels button), add these labels one by one:
    - runtimeEnv
    - image
    - theme
    - screen
    - font
    - toast
    - misc
    - panelsetting

9. In the Addressables Groups window toolbar, go to **Build > New Build > Default Build Script**

This bakes all SDK assets into the local addressables build.

---

## Step 9: Create the AccountManager Script

Create a new file at `Assets/Scripts/AccountManager.cs` with this content:

```csharp
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public struct LoginResult
    {
        public bool Success;
        public string Id;
        public string AccessToken;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task<LoginResult> SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            return new LoginResult
            {
                Success = true,
                Id = AuthenticationService.Instance.PlayerId,
                AccessToken = AuthenticationService.Instance.AccessToken
            };
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Sign in anonymously failed with error code: {e.ErrorCode}");
        }

        return new LoginResult
        {
            Success = false,
            Id = default,
            AccessToken = default
        };
    }
}
```

---

## Step 10: Create the CommerceOpportunityUI Script


Create a new file at `Assets/Scripts/CommerceOpportunityUI.cs` with this content:

```csharp
using System;
using System.Threading.Tasks;
using Unity.Commerce.Backend;
using Unity.Commerce.CommerceOpportunity;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.EventSystems;
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
        // EventSystem (required for UI input)
        var existingEventSystem = FindObjectOfType<EventSystem>();
        if (existingEventSystem == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }
        else if (existingEventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            existingEventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

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
```

---

## Step 11: Set Up the Scene

1. Open `Assets/Scenes/SampleScene.unity`

2. In the **Hierarchy**, right-click and choose **Create Empty**

3. Rename it to `CommerceManager`

4. With `CommerceManager` selected, click **Add Component** in the Inspector and add `AccountManager`

5. Click **Add Component** again and add `CommerceOpportunityUI`

---

## Step 12: Add Scene to Build Settings

1. Go to **File > Build Settings**

2. Click **Add Open Scenes** to add `SampleScene` to the build list

---

## Step 13: Set DeepLink (Android)

1. Create AndroidManifest.xml file in Assets/Pugins/Android with the following content. Note the settings for the deep link that were obtained from the Assets/Resources/Commerce Opportunity Editor/Config DataStore -> Config/Organization Configuration/Subaffiliate Project Configurations/ID/Callback
```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest
    xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.unity3d.player"
    xmlns:tools="http://schemas.android.com/tools">
    <application>
        <activity android:name="com.unity3d.player.UnityPlayerActivity"
                  android:theme="@style/UnityThemeSelector">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
			<intent-filter>
			  <action android:name="android.intent.action.VIEW" />
			  <category android:name="android.intent.category.DEFAULT" />
			  <category android:name="android.intent.category.BROWSABLE" />
			  <data android:scheme="ob4ace9422c18458897697c7a358f2f48" android:host="immersive-commerce" />
			</intent-filter>			
            <meta-data android:name="unityplayer.UnityActivity" android:value="true" />
        </activity>
    </application>
</manifest>
```

## Verification

1. Enter **Play Mode** (for best results, open **Window > General > Device Simulator** first)

2. Wait for the status text to show "Ready"

3. Type the name of the Commerce Opportunity you created in Step 6 into the text field

4. Click **Show**

5. The Walmart product overlay should appear

6. Check the **Console** window for any errors if something doesn't work
