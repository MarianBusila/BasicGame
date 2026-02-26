\# Step-by-Step Guide: Walmart Commerce SDK in BasicGame



---

\## Step 0: Create Unity Project in the Editor and import packages from the Asset Store

https://assetstore.unity.com/packages/sdk/unity-commerce-sdk-7265475

https://assetstore.unity.com/packages/sdk/walmart-immersive-commerce-sdk-2185813



\## Step 1: Add the Authentication Package



Open `Packages/manifest.json` in a text editor and add this line inside `"dependencies"`:



```json

"com.unity.services.authentication": "3.3.3",

```



Add it right after the `"com.unity.commerce-sdk"` line. Save the file. Unity will auto-resolve it when you switch back to the Editor.



---



\## Step 2: Link Project to Unity Cloud



1\. In Unity Editor, go to \*\*Edit > Project Settings > Services\*\*

2\. Click \*\*Link to Unity Cloud\*\* (or select your organization if already linked)

3\. Choose your Unity Organization

4\. This sets the `cloudProjectId` and `organizationId` that the SDK needs at runtime



---



\## Step 3: Create the Data Store Assets



1\. Go to \*\*Window > Commerce Opportunity\*\*

2\. The first time you open it, an error may appear — that's normal. Close the window.

3\. Check in the \*\*Project\*\* window: `Assets/Resources/Commerce Opportunity Editor/` should now contain `CommerceOpportunityDatastore.asset`



---



\## Step 4: Configure CommerceOpportunityDatastore



1\. In the \*\*Project\*\* window, navigate to `Assets/Resources/Commerce Opportunity Editor/`

2\. Select `CommerceOpportunityDatastore`

3\. In the \*\*Inspector\*\*, set these fields:

&nbsp;  - \*\*Organization Id\*\* — your Unity Organization ID (find it in Unity Dashboard > organization menu > Manage Organization)

&nbsp;  - \*\*Environment Id\*\* — your environment ID (Unity Dashboard > your project > Environments tab)

&nbsp;  - \*\*Runtime Environment\*\* — drag `RuntimeEnvDetails` from `Packages/Walmart Immersive Commerce SDK/AddressableAssets/RuntimeEnvironments/`

&nbsp;  - \*\*Editor Environment\*\* — drag `CommerceOpportunityEditorEnv` from `Packages/Walmart Immersive Commerce SDK/AddressableAssets/RuntimeEnvironments/`



---



\## Step 5: Create Remaining Config Assets



1\. Go to \*\*Window > Commerce Opportunity\*\* again

2\. This time it should open without errors and auto-create two additional assets in `Assets/Resources/Commerce Opportunity Editor/`:

&nbsp;  - `ConfigDatastore.asset`

&nbsp;  - `ProductPackagesDatastore.asset`



---



\## Step 6: Create a Commerce Opportunity



1\. In the \*\*Commerce Opportunity\*\* window, create a new Commerce Opportunity (give it a name you'll remember, e.g. `"My ComOp 01"`)

2\. Click \*\*Sync\*\* to pull Product Packages from the Unity Dashboard

3\. Link a Product Package to your Commerce Opportunity



> You must have already created a Product Package in the \[Unity Dashboard Immersive Commerce setup guide](https://cloud.unity.com/home/organizations/default/projects/default/immersive-commerce-sdk/setup-guide) before this step.



---



\## Step 7: Configure ARSConfiguration



This is required for Walmart account linking and checkout, but \*\*not\*\* for viewing products.



1\. Select `Assets/Resources/Unity Immersive Commerce/ARSConfiguration` in the Project window

2\. In the Inspector, set:

&nbsp;  - \*\*Base Url\*\* — `https://cloud-code.services.api.unity.com/v1/projects/`



---



\## Step 8: Set Up Addressables



The SDK uses Addressables to load its UI screens, themes, fonts, and runtime environments. Your project might not have Addressables initialized yet.



1\. Go to \*\*Window > Asset Management > Addressables > Groups\*\*

2\. If prompted, click \*\*Create Addressables Settings\*\* — this creates `Assets/AddressableAssetsData/`

3\. You need to manually add entries pointing to the SDK assets. Do this:

&nbsp;   - In the Project window, navigate to Packages/Walmart Immersive Commerce SDK/AddressableAssets/AddressableAssetsData/

&nbsp;   - You should see Walmart Group.asset there — drag it into the Addressables Groups window

&nbsp;   - Then navigate to Packages/Commerce SDK/Runtime/UI/AddressableAssets/AddressableAssetsData/AssetGroup/

&nbsp;   - You should see CommerceOpportunity Group.asset there — drag it into the Addressables Groups window

4\. Select \*\*all entries\*\* inside `CommerceOpportunity Group`, right-click, and choose \*\*Move to > Default Local Group\*\*

5\. Select \*\*all entries\*\* inside `Walmart Group`, right-click, and choose \*\*Move to > Default Local Group\*\*

6\. Right-click the now-empty `CommerceOpportunity Group` and delete it

7\. Right-click the now-empty `Walmart Group` and delete it

8\. In the top menu bar of the window, go to Tools > Labels (or click the Labels button)
  3. Add these labels one by one:
    - runtimeEnv
    - image
    - theme
    - screen
    - font
    - toast
    - misc
    - panelsetting

9\. In the Addressables Groups window toolbar, go to \*\*Build > New Build > Default Build Script\*\*



This bakes all SDK assets into the local addressables build.



---



\## Step 9: Create the AccountManager Script



Create a new file at `Assets/Scripts/AccountManager.cs` with this content:



```csharp

using System;

using System.Threading.Tasks;

using Unity.Services.Authentication;

using Unity.Services.Core;

using UnityEngine;



public class AccountManager : MonoBehaviour

{

&nbsp;   public struct LoginResult

&nbsp;   {

&nbsp;       public bool Success;

&nbsp;       public string Id;

&nbsp;       public string AccessToken;

&nbsp;   }



&nbsp;   public async Task InitializeAsync()

&nbsp;   {

&nbsp;       try

&nbsp;       {

&nbsp;           await UnityServices.InitializeAsync();

&nbsp;       }

&nbsp;       catch (Exception e)

&nbsp;       {

&nbsp;           Debug.LogException(e);

&nbsp;       }

&nbsp;   }



&nbsp;   public async Task<LoginResult> SignInAnonymously()

&nbsp;   {

&nbsp;       try

&nbsp;       {

&nbsp;           await AuthenticationService.Instance.SignInAnonymouslyAsync();

&nbsp;           return new LoginResult

&nbsp;           {

&nbsp;               Success = true,

&nbsp;               Id = AuthenticationService.Instance.PlayerId,

&nbsp;               AccessToken = AuthenticationService.Instance.AccessToken

&nbsp;           };

&nbsp;       }

&nbsp;       catch (RequestFailedException e)

&nbsp;       {

&nbsp;           Debug.LogError($"Sign in anonymously failed with error code: {e.ErrorCode}");

&nbsp;       }



&nbsp;       return new LoginResult

&nbsp;       {

&nbsp;           Success = false,

&nbsp;           Id = default,

&nbsp;           AccessToken = default

&nbsp;       };

&nbsp;   }

}

```



---



\## Step 10: Create the CommerceOpportunityUI Script



Create a new file at `Assets/Scripts/CommerceOpportunityUI.cs` with this content:



```csharp

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

&nbsp;   private AccountManager \_accountManager;

&nbsp;   private InputField \_inputField;

&nbsp;   private Button \_showButton;

&nbsp;   private Text \_statusText;

&nbsp;   private bool \_sdkReady;



&nbsp;   private async void Awake()

&nbsp;   {

&nbsp;       \_accountManager = GetComponent<AccountManager>();

&nbsp;       await InitializeSdk();

&nbsp;   }



&nbsp;   private void Start()

&nbsp;   {

&nbsp;       CreateUI();

&nbsp;   }



&nbsp;   private void CreateUI()

&nbsp;   {

&nbsp;       // Canvas

&nbsp;       var canvasGo = new GameObject("Canvas");

&nbsp;       canvasGo.transform.SetParent(transform);

&nbsp;       var canvas = canvasGo.AddComponent<Canvas>();

&nbsp;       canvas.renderMode = RenderMode.ScreenSpaceOverlay;

&nbsp;       canvas.sortingOrder = -1;

&nbsp;       canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

&nbsp;       canvasGo.AddComponent<GraphicRaycaster>();



&nbsp;       // Panel background

&nbsp;       var panelGo = new GameObject("Panel");

&nbsp;       panelGo.transform.SetParent(canvasGo.transform, false);

&nbsp;       var panelRect = panelGo.AddComponent<RectTransform>();

&nbsp;       panelRect.anchorMin = new Vector2(0.1f, 0.85f);

&nbsp;       panelRect.anchorMax = new Vector2(0.9f, 0.95f);

&nbsp;       panelRect.offsetMin = Vector2.zero;

&nbsp;       panelRect.offsetMax = Vector2.zero;

&nbsp;       var layout = panelGo.AddComponent<HorizontalLayoutGroup>();

&nbsp;       layout.spacing = 10;

&nbsp;       layout.childForceExpandWidth = false;

&nbsp;       layout.childForceExpandHeight = true;

&nbsp;       layout.padding = new RectOffset(10, 10, 5, 5);



&nbsp;       // InputField

&nbsp;       var inputGo = new GameObject("InputField");

&nbsp;       inputGo.transform.SetParent(panelGo.transform, false);

&nbsp;       var inputImage = inputGo.AddComponent<Image>();

&nbsp;       inputImage.color = Color.white;

&nbsp;       var inputLayout = inputGo.AddComponent<LayoutElement>();

&nbsp;       inputLayout.flexibleWidth = 1;



&nbsp;       var textGo = new GameObject("Text");

&nbsp;       textGo.transform.SetParent(inputGo.transform, false);

&nbsp;       var text = textGo.AddComponent<Text>();

&nbsp;       text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

&nbsp;       text.color = Color.black;

&nbsp;       text.supportRichText = false;

&nbsp;       var textRect = textGo.GetComponent<RectTransform>();

&nbsp;       textRect.anchorMin = Vector2.zero;

&nbsp;       textRect.anchorMax = Vector2.one;

&nbsp;       textRect.offsetMin = new Vector2(5, 2);

&nbsp;       textRect.offsetMax = new Vector2(-5, -2);



&nbsp;       var placeholderGo = new GameObject("Placeholder");

&nbsp;       placeholderGo.transform.SetParent(inputGo.transform, false);

&nbsp;       var placeholder = placeholderGo.AddComponent<Text>();

&nbsp;       placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

&nbsp;       placeholder.fontStyle = FontStyle.Italic;

&nbsp;       placeholder.color = new Color(0.5f, 0.5f, 0.5f);

&nbsp;       placeholder.text = "Enter Commerce Opportunity name...";

&nbsp;       var phRect = placeholderGo.GetComponent<RectTransform>();

&nbsp;       phRect.anchorMin = Vector2.zero;

&nbsp;       phRect.anchorMax = Vector2.one;

&nbsp;       phRect.offsetMin = new Vector2(5, 2);

&nbsp;       phRect.offsetMax = new Vector2(-5, -2);



&nbsp;       \_inputField = inputGo.AddComponent<InputField>();

&nbsp;       \_inputField.textComponent = text;

&nbsp;       \_inputField.placeholder = placeholder;



&nbsp;       // Show Button

&nbsp;       var buttonGo = new GameObject("ShowButton");

&nbsp;       buttonGo.transform.SetParent(panelGo.transform, false);

&nbsp;       var btnImage = buttonGo.AddComponent<Image>();

&nbsp;       btnImage.color = new Color(0.2f, 0.6f, 1f);

&nbsp;       var btnLayout = buttonGo.AddComponent<LayoutElement>();

&nbsp;       btnLayout.minWidth = 100;



&nbsp;       var btnTextGo = new GameObject("Text");

&nbsp;       btnTextGo.transform.SetParent(buttonGo.transform, false);

&nbsp;       var btnText = btnTextGo.AddComponent<Text>();

&nbsp;       btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

&nbsp;       btnText.text = "Show";

&nbsp;       btnText.color = Color.white;

&nbsp;       btnText.alignment = TextAnchor.MiddleCenter;

&nbsp;       var btnTextRect = btnTextGo.GetComponent<RectTransform>();

&nbsp;       btnTextRect.anchorMin = Vector2.zero;

&nbsp;       btnTextRect.anchorMax = Vector2.one;

&nbsp;       btnTextRect.offsetMin = Vector2.zero;

&nbsp;       btnTextRect.offsetMax = Vector2.zero;



&nbsp;       \_showButton = buttonGo.AddComponent<Button>();

&nbsp;       \_showButton.onClick.AddListener(OnShowClicked);



&nbsp;       // Status Text

&nbsp;       var statusGo = new GameObject("StatusText");

&nbsp;       statusGo.transform.SetParent(canvasGo.transform, false);

&nbsp;       \_statusText = statusGo.AddComponent<Text>();

&nbsp;       \_statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

&nbsp;       \_statusText.color = Color.white;

&nbsp;       \_statusText.fontSize = 14;

&nbsp;       \_statusText.alignment = TextAnchor.UpperLeft;

&nbsp;       var statusRect = statusGo.GetComponent<RectTransform>();

&nbsp;       statusRect.anchorMin = new Vector2(0.1f, 0.75f);

&nbsp;       statusRect.anchorMax = new Vector2(0.9f, 0.85f);

&nbsp;       statusRect.offsetMin = Vector2.zero;

&nbsp;       statusRect.offsetMax = Vector2.zero;

&nbsp;   }



&nbsp;   private async Task InitializeSdk()

&nbsp;   {

&nbsp;       SetStatus("Initializing WalmartSdk...");



&nbsp;       if (!WalmartSdk.Instance.IsInitialized)

&nbsp;       {

&nbsp;           await WalmartSdk.Instance.InitializeAsync();

&nbsp;       }



&nbsp;       SetStatus("WalmartSdk initialized. Signing in...");



&nbsp;       await \_accountManager.InitializeAsync();

&nbsp;       var result = await \_accountManager.SignInAnonymously();



&nbsp;       if (result.Success)

&nbsp;       {

&nbsp;           bool linked = await WalmartSdk.Instance

&nbsp;               .SetupAuthorizationHeaderAndCheckAccountLinkStatus(

&nbsp;                   "Bearer", AuthenticationService.Instance.AccessToken);

&nbsp;           SetStatus("Ready. Account " + (linked ? "linked" : "not linked") + ".");

&nbsp;           \_sdkReady = true;

&nbsp;       }

&nbsp;       else

&nbsp;       {

&nbsp;           SetStatus("Authentication failed. Check Console for details.");

&nbsp;       }

&nbsp;   }



&nbsp;   private void OnShowClicked()

&nbsp;   {

&nbsp;       if (!\_sdkReady)

&nbsp;       {

&nbsp;           SetStatus("SDK not ready yet.");

&nbsp;           return;

&nbsp;       }



&nbsp;       string name = \_inputField.text.Trim();

&nbsp;       if (string.IsNullOrEmpty(name))

&nbsp;       {

&nbsp;           SetStatus("Please enter a Commerce Opportunity name.");

&nbsp;           return;

&nbsp;       }



&nbsp;       \_ = ShowCommerceOpportunity(name);

&nbsp;   }



&nbsp;   private async Task ShowCommerceOpportunity(string commerceOpportunityName)

&nbsp;   {

&nbsp;       SetStatus($"Loading '{commerceOpportunityName}'...");



&nbsp;       if (!WalmartSdk.Instance.TryGetCommerceOpportunity(

&nbsp;               commerceOpportunityName, out CommerceOpportunityInstance comOp))

&nbsp;       {

&nbsp;           SetStatus($"Commerce Opportunity '{commerceOpportunityName}' not found.");

&nbsp;           return;

&nbsp;       }



&nbsp;       if (comOp.ProductPackageId == Guid.Empty)

&nbsp;       {

&nbsp;           SetStatus($"'{commerceOpportunityName}' has no linked Product Package.");

&nbsp;           return;

&nbsp;       }



&nbsp;       BackendResponse response = await WalmartSdk.Instance

&nbsp;           .DownloadProductPackageDataAsync(comOp.ProductPackageId);



&nbsp;       if (!response.IsSuccess)

&nbsp;       {

&nbsp;           SetStatus($"Failed to download product data: {response.Status.Code}");

&nbsp;           return;

&nbsp;       }



&nbsp;       if (!WalmartSdk.Instance.TryShowCommerceOpportunity(commerceOpportunityName))

&nbsp;       {

&nbsp;           SetStatus($"Failed to show Commerce Opportunity.");

&nbsp;           return;

&nbsp;       }



&nbsp;       SetStatus("");

&nbsp;   }



&nbsp;   private void SetStatus(string message)

&nbsp;   {

&nbsp;       if (\_statusText != null)

&nbsp;           \_statusText.text = message;

&nbsp;       if (!string.IsNullOrEmpty(message))

&nbsp;           Debug.Log($"\[CommerceUI] {message}");

&nbsp;   }

}

```



---



\## Step 11: Set Up the Scene



1\. Open `Assets/Scenes/SampleScene.unity`

2\. In the \*\*Hierarchy\*\*, right-click and choose \*\*Create Empty\*\*

3\. Rename it to `CommerceManager`

4\. With `CommerceManager` selected, click \*\*Add Component\*\* in the Inspector and add `AccountManager`

5\. Click \*\*Add Component\*\* again and add `CommerceOpportunityUI`



---



\## Step 12: Add Scene to Build Settings



1\. Go to \*\*File > Build Settings\*\*

2\. Click \*\*Add Open Scenes\*\* to add `SampleScene` to the build list



---



\## Verification



1\. Enter \*\*Play Mode\*\* (for best results, open \*\*Window > General > Device Simulator\*\* first)

2\. Wait for the status text to show "Ready"

3\. Type the name of the Commerce Opportunity you created in Step 6 into the text field

4\. Click \*\*Show\*\*

5\. The Walmart product overlay should appear

6\. Check the \*\*Console\*\* window for any errors if something doesn't work



