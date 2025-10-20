using System.Collections;
using System.IO;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShareIQController : MonoBehaviour
{
    private const string ScorePrefKey = "ShareIQScore";
    private const string AppStoreUrl = "https://apps.apple.com/app/id0000000000"; // TODO: replace with your real App Store URL.
    private const int ShareImageWidth = 1200;
    private const int ShareImageHeight = 630;
    private const string iOSGameId = "5968456";
    private const string androidGameId = ""; // TODO: replace with your Android Game ID when available.
    private const string bannerPlacementId = "Banner_iOS"; // TODO: update to match the placement name set up in the Unity Dashboard.

    [SerializeField]
    private bool testMode = true;

    private bool adsInitialized;
    private bool bannerLoaded;
    private bool bannerShowing;
    private Text scoreValueText;
    private Button playAgainButton;
    private Button shareButton;
    private GameObject bannerPlaceholder;
    private bool isSharing;

    void Awake()
    {
#if UNITY_ADS
        StartCoroutine(InitializeAdsRoutine());
#else
        Debug.LogWarning("Unity Ads not enabled. Skipping ad initialization.");
#endif
    }

    void Start()
    {
        CacheUI();
        DisplayScore();
        SetBannerPlaceholderVisible(true);
    }

#if UNITY_ADS
    private IEnumerator InitializeAdsRoutine()
    {
        if (adsInitialized)
        {
            yield break;
        }

#if UNITY_IOS
        string gameId = iOSGameId;
#elif UNITY_ANDROID
        string gameId = string.IsNullOrEmpty(androidGameId) ? iOSGameId : androidGameId;
#else
        string gameId = iOSGameId;
#endif

        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogWarning("Unity Ads initialization skipped: no valid game ID configured for this platform.");
            yield break;
        }

        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(gameId, testMode);
            float timeout = 15f;
            while (!Advertisement.isInitialized && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!Advertisement.isInitialized)
        {
            Debug.LogError("Unity Ads failed to initialize.");
            yield break;
        }

        adsInitialized = true;
        Debug.Log("Unity Ads initialization complete.");

        LoadBanner();
    }

    private void LoadBanner()
    {
        if (!adsInitialized)
        {
            return;
        }

        bannerLoaded = false;
        bannerShowing = false;

        SetBannerPlaceholderVisible(true);

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

        BannerLoadOptions loadOptions = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(bannerPlacementId, loadOptions);
    }

    private void OnBannerLoaded()
    {
        Debug.Log("Unity Ads banner loaded.");
        bannerLoaded = true;
        ShowBanner();
    }

    private void OnBannerError(string message)
    {
        Debug.LogWarning($"Unity Ads banner failed to load: {message}");
        bannerLoaded = false;
        bannerShowing = false;
        SetBannerPlaceholderVisible(true);
    }

    private void ShowBanner()
    {
        if (!adsInitialized || !bannerLoaded)
        {
            return;
        }

        if (bannerShowing)
        {
            return;
        }

        BannerOptions showOptions = new BannerOptions
        {
            clickCallback = () => Debug.Log("Unity Ads banner clicked."),
            hideCallback = () => Debug.Log("Unity Ads banner hidden."),
            showCallback = () =>
            {
                Debug.Log("Unity Ads banner shown.");
                SetBannerPlaceholderVisible(false);
            }
        };

        Advertisement.Banner.Show(bannerPlacementId, showOptions);
        bannerShowing = true;
    }
#endif // UNITY_ADS

    private void HideBanner(bool destroy = true, bool showPlaceholder = true)
    {
#if UNITY_ADS
        if (!adsInitialized)
        {
            return;
        }

        if (!bannerLoaded && !bannerShowing)
        {
            return;
        }

        Advertisement.Banner.Hide(destroy);
        bannerShowing = false;
#endif
        SetBannerPlaceholderVisible(showPlaceholder);
    }

    private void CacheUI()
    {
        GameObject scoreValueObject = GameObject.Find("ScoreValue");
        if (scoreValueObject != null)
        {
            scoreValueText = scoreValueObject.GetComponent<Text>();
        }

        GameObject playAgainObject = GameObject.Find("PlayAgainButton");
        if (playAgainObject != null)
        {
            playAgainButton = playAgainObject.GetComponent<Button>();
            playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
        }

        GameObject shareButtonObject = GameObject.Find("ShareButton");
        if (shareButtonObject != null)
        {
            shareButton = shareButtonObject.GetComponent<Button>();
            shareButton.onClick.AddListener(() => StartCoroutine(ShareScoreRoutine()));
        }

        bannerPlaceholder = GameObject.Find("BannerPlaceholder");
    }

    private void DisplayScore()
    {
        if (scoreValueText == null)
        {
            return;
        }

        int score = PlayerPrefs.GetInt(ScorePrefKey, 0);
        scoreValueText.text = score.ToString("000");
    }

    private void HandlePlayAgainClicked()
    {
        HideBanner();
        SceneManager.LoadScene("GameScene");
    }

    private IEnumerator ShareScoreRoutine()
    {
        if (isSharing)
        {
            yield break;
        }

        isSharing = true;

        bool bannerWasShowing = bannerShowing;
        HideBanner(false, false);
        bool placeholderWasActive = bannerPlaceholder != null && bannerPlaceholder.activeSelf;
        SetBannerPlaceholderVisible(false);

        if (shareButton != null)
        {
            shareButton.interactable = false;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = null;
        Texture2D shareTexture = null;

        try
        {
            screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenshot == null)
            {
                Debug.LogWarning("[ShareIQ] Failed to capture screenshot for sharing.");
                yield break;
            }

            shareTexture = CreateShareImage(screenshot, ShareImageWidth, ShareImageHeight, new Color32(12, 18, 24, 255));

            byte[] pngData = shareTexture.EncodeToPNG();
            string sharePath = Path.Combine(Application.temporaryCachePath, "coloriq-share.png");
            File.WriteAllBytes(sharePath, pngData);
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.SetNoBackupFlag(sharePath);
#endif

            int score = PlayerPrefs.GetInt(ScorePrefKey, 0);
            string message = ComposeShareMessage(score);

            IOSShare.ShareImage(sharePath, message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ShareIQ] Failed to share score: {ex}");
        }
        finally
        {
            if (screenshot != null)
            {
                UnityEngine.Object.Destroy(screenshot);
            }

            if (shareTexture != null)
            {
                UnityEngine.Object.Destroy(shareTexture);
            }

            if (shareButton != null)
            {
                shareButton.interactable = true;
            }

            isSharing = false;

#if UNITY_ADS
            if (bannerWasShowing && adsInitialized)
            {
                LoadBanner();
            }
            else
#endif
            {
                SetBannerPlaceholderVisible(placeholderWasActive);
            }
        }
    }

    private void OnDestroy()
    {
        HideBanner();
    }

    private void SetBannerPlaceholderVisible(bool visible)
    {
        if (bannerPlaceholder != null)
        {
            bannerPlaceholder.SetActive(visible && !isSharing);
        }
    }

    private Texture2D CreateShareImage(Texture2D source, int targetWidth, int targetHeight, Color background)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

        source.filterMode = FilterMode.Bilinear;

        Color[] fill = new Color[targetWidth * targetHeight];
        for (int i = 0; i < fill.Length; i++)
        {
            fill[i] = background;
        }
        result.SetPixels(fill);

        float scale = Mathf.Min(targetWidth / (float)source.width, targetHeight / (float)source.height);
        int scaledWidth = Mathf.RoundToInt(source.width * scale);
        int scaledHeight = Mathf.RoundToInt(source.height * scale);
        int offsetX = (targetWidth - scaledWidth) / 2;
        int offsetY = (targetHeight - scaledHeight) / 2;

        for (int y = 0; y < scaledHeight; y++)
        {
            float v = (y + 0.5f) / scaledHeight;
            for (int x = 0; x < scaledWidth; x++)
            {
                float u = (x + 0.5f) / scaledWidth;
                Color pixel = source.GetPixelBilinear(u, v);
                result.SetPixel(offsetX + x, offsetY + y, pixel);
            }
        }

        result.Apply();
        return result;
    }

    private string ComposeShareMessage(int score)
    {
        return $"I hit {score} IQ in Color IQ! Beat me => {AppStoreUrl}";
    }
}
