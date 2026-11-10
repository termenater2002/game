using System.Globalization;
using TMPro;
using UnityEngine;

public class DemoHud : MonoBehaviour
{
    public SessionWallet wallet;
    public FirstPersonCamera look;
    public ObjectGrabber grabber;
    private Canvas canvas;
    private TextMeshProUGUI balance;
    private TextMeshProUGUI status;
    private TextMeshProUGUI gain;
    private decimal previousBalance;
    private float gainUntil;
    public string DisplayedBalance => balance != null ? balance.text : string.Empty;

    private void Awake()
    {
        if (wallet == null) wallet = GetComponent<SessionWallet>();
        var go = new GameObject("Demo HUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var panel = new GameObject("Session Panel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(go.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        Anchor(rect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(370, 112));
        var image = panel.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.025f, 0.035f, 0.055f, 0.88f);
        image.raycastTarget = false;
        Label(panel.transform, "Heading", "FOUNDATION v1  /  SESSION EARNINGS", new Vector2(0, 1), new Vector2(14, -10), new Vector2(346, 25), 17);
        balance = Label(panel.transform, "Balance", "$ 0.00", new Vector2(0, 1), new Vector2(14, -36), new Vector2(346, 40), 30);
        gain = Label(panel.transform, "Damage Reward", "", new Vector2(0, 1), new Vector2(14, -80), new Vector2(346, 24), 17);
        gain.color = new Color(0.5f, 1f, 0.7f);
        Label(go.transform, "Controls", "WASD / arrows: move    SPACE: jump\nMOUSE: look    LMB: grab / release    ESC: unlock cursor", Vector2.zero, new Vector2(24, 22), new Vector2(920, 65), 20);
        var crosshair = Label(go.transform, "Crosshair", "+", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(32, 32), 28);
        crosshair.alignment = TextAlignmentOptions.Center;
        status = Label(go.transform, "Interaction", "", new Vector2(0.5f, 0.5f), new Vector2(0, -44), new Vector2(640, 30), 20);
        status.alignment = TextAlignmentOptions.Center;
    }

    private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
    private static TextMeshProUGUI Label(Transform parent, string name, string text, Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        Vector2 pivot = anchor.x == 0.5f ? new Vector2(0.5f, 0.5f) : anchor;
        Anchor(label.rectTransform, anchor, pivot, position, size);
        return label;
    }
    private void OnEnable()
    {
        if (canvas != null) canvas.enabled = true;
        if (wallet == null) return;
        previousBalance = wallet.Earnings.Balance;
        wallet.Earnings.Changed += RefreshBalance;
        RefreshBalance(previousBalance);
    }
    private void RefreshBalance(decimal value)
    {
        balance.text = "$ " + value.ToString("N2", CultureInfo.InvariantCulture);
        if (value > previousBalance)
        {
            gain.text = "+ $ " + (value - previousBalance).ToString("N2", CultureInfo.InvariantCulture);
            gainUntil = Time.unscaledTime + 1.5f;
        }
        previousBalance = value;
    }
    private void Update()
    {
        if (Time.unscaledTime > gainUntil) gain.text = "";
        if (Cursor.lockState != CursorLockMode.Locked) status.text = "Click to resume";
        else if (grabber != null && grabber.IsHolding) status.text = "LMB: release";
        else if (grabber != null && grabber.TryGetTarget(out var target, out _)) status.text = "LMB: grab " + target.name;
        else status.text = "";
    }
    private void OnDisable()
    {
        if (wallet != null) wallet.Earnings.Changed -= RefreshBalance;
        if (canvas != null) canvas.enabled = false;
    }
}
