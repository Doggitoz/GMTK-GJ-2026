using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// GEAR PUZZLE — single-file, code-built Unity UI minigame.
///
/// Designed to be embedded as a popup inside another game. Attach this
/// script to an empty GameObject anywhere in your scene (e.g. alongside
/// your spawner script). It builds its own Canvas, EventSystem and UI at
/// Awake — but stays HIDDEN until you call OpenMinigame().
///
/// INTEGRATION
///   GearPuzzleGame minigame = ...; // reference to this component
///   minigame.OpenMinigame();       // shows the popup (resumes prior progress,
///                                  // or starts a fresh puzzle the first time /
///                                  // after a previous win)
///   minigame.CloseMinigame();      // hides the popup; progress is preserved
///   minigame.onMinigameComplete.AddListener(success => { ... });
///                                  // fires with `true` the instant the puzzle
///                                  // is solved
///   minigame.IsOpen                // whether the popup is currently visible
///   minigame.IsCompleted           // whether the current puzzle has been solved
///
/// Clicking outside the panel (on the dimmed backdrop) closes the popup,
/// same as calling CloseMinigame() — progress is preserved either way.
///
/// RULES
/// - There is an invisible 1-column, 10-row table (rows 0..9). The rows and
///   the column itself are not drawn — only the "IN" marker (top) and "OUT"
///   marker (bottom) show where the table begins and ends.
/// - 3 gears spawn on random rows. The top gear lands closer to IN than to
///   the center gear's row, and the bottom gear lands closer to OUT than to
///   the center gear's row. Anchor distance is capped so every puzzle is
///   guaranteed solvable (see GenerateValidRows for the proof).
/// - Each gear has 4 sizes. Clicking a gear cycles size 1 -> 2 -> 3 -> 4 -> 1.
///   Growing a gear expands it symmetrically: one extra row above AND one
///   extra row below per size step. Visually, the gear graphic scales up
///   around its fixed center point.
/// - Gears may hang off the top/bottom edge of the table (no clamping on
///   position).
/// - A gear visually turns (the gear graphic itself spins) whenever it is
///   touching its neighbor exactly — no gap, no overlap.
/// - IN/OUT icons light up whenever their adjacent gear is in contact
///   (touching or overlapping) with the center gear.
/// - The player wins when both gear pairs are touching-but-not-overlapping
///   AND every one of the 10 rows is covered by some gear.
/// </summary>
public class GearPuzzleGame : MonoBehaviour
{
    [Header("Board (logical — not drawn)")]
    public int rowCount = 10;
    public float cellHeight = 42f;
    public float boardWidth = 220f;

    [Header("Popup Panel")]
    public float panelWidth = 300f;
    public float panelHeight = 700f;
    public Color panelColor = new Color(0.08f, 0.09f, 0.12f, 0.97f);
    public Color backdropColor = new Color(0f, 0f, 0f, 0.6f);
    // Raise this if the popup's clicks are being intercepted by other UI
    // (HUD, menus, etc.) in the host game — higher always wins raycasts.
    public int canvasSortingOrder = 999;
    // Releases and shows the cursor while the popup is open (restoring
    // whatever it was set to beforehand on close). Turn this off if your
    // game already handles cursor state itself. Strongly recommended for
    // mouse-look style controllers — otherwise moving the mouse to click a
    // gear also rotates the camera/player, which can visually look like
    // "the popup randomly closes" if anything nearby reacts to that turn.
    public bool manageCursorState = true;

    [Header("Gear Fill Colors")]
    public Color colorNormal = new Color(0.80f, 0.82f, 0.88f);
    public Color colorPartial = new Color(0.95f, 0.80f, 0.35f);
    public Color colorConnected = new Color(0.40f, 0.85f, 0.50f);
    public Color colorOverlap = new Color(0.90f, 0.30f, 0.30f);

    [Header("IN / OUT Marker Colors")]
    public Color colorIn = new Color(0.55f, 0.85f, 1f);
    public Color colorOut = new Color(1f, 0.65f, 0.35f);
    // Shown on the IN/OUT sprites when that end isn't in contact.
    public Color colorIconInactive = new Color(0.45f, 0.45f, 0.48f, 0.7f);

    [Header("Turning")]
    public float turnSpeedDegPerSec = 140f;

    [Header("Click / Tap")]
    // Small gears (size 1) are visually tiny — this guarantees every gear
    // always has at least this large an (invisible) tap/click target,
    // centered on the gear, regardless of its current visual size.
    public float minClickDiameter = 70f;

    [Header("Gear Graphics")]
    // Leave empty to use the built-in procedurally generated gear icon.
    // Assign your own Sprite here to fully replace it.
    public Sprite customGearSprite;
    public int gearTextureSize = 128;
    public int gearTeeth = 10;

    [Header("IN / OUT Icon Graphics")]
    // Leave empty to use the built-in procedurally generated arrow icon.
    public Sprite customInIconSprite;
    public Sprite customOutIconSprite;
    public int arrowTextureSize = 64;
    public float iconSize = 30f;

    [Header("Text Content")]
    public string titleText = "GEAR PUZZLE";
    public string inLabelText = "IN";
    public string outLabelText = "OUT";
    public string winMessageText = "YOU WIN!\nAll gears connected.";
    public bool showSizeLabel = true;
    public string sizeLabelFormat = "Size {0}";
    public string rowsFilledLabel = "Rows filled";

    [Header("Fonts & Sizes")]
    // Leave empty to use Unity's built-in default font.
    public Font customFont;
    public int titleFontSize = 26;
    public int markerLabelFontSize = 18;
    public int statusFontSize = 15;
    public int winFontSize = 40;
    public int gearSizeLabelFontSize = 13;

    [Header("Text Colors")]
    public Color titleColor = Color.white;
    public Color statusColor = new Color(0.85f, 0.85f, 0.9f);
    public Color gearLabelColor = Color.black;
    public Color winOverlayColor = new Color(0f, 0f, 0f, 0.65f);
    public Color winTextColor = Color.white;

    // How far apart (in rows) two adjacent gear anchors may spawn.
    // Capped at 4 (an initial gap of at most 3 rows) — see GenerateValidRows
    // for why this specific cap guarantees every puzzle is solvable.
    public int maxAnchorDistance = 4;

    [Header("Completion Callback")]
    // Fires with `true` the instant the puzzle is solved. Hook your other
    // script's method to this in the Inspector, or via
    // onMinigameComplete.AddListener(...) in code.
    public BoolUnityEvent onMinigameComplete = new BoolUnityEvent();
    // Fires whenever the popup closes, for ANY reason — clicking outside,
    // walking away, or auto-closing after a win. Useful for a spawner to
    // reliably know "the popup is no longer open" regardless of which path
    // caused it, without having to guess.
    public UnityEngine.Events.UnityEvent onMinigameClosed = new UnityEngine.Events.UnityEvent();

    [System.Serializable]
    public class BoolUnityEvent : UnityEvent<bool> { }

    float gearBaseDiameter; // visual diameter of a size-1 gear, set in BuildUI

    // ---- internal state ----
    class Gear
    {
        public int anchorRow;
        public int size = 1; // 1..4
        public RectTransform root;     // fixed position anchor (never moves)
        public RectTransform graphic;  // the gear image — this is what scales & spins
        public RectTransform hitArea;  // invisible click target — always big enough to tap
        public Image image;
        public Text label;
        public bool isTurning;

        public int RowSpan { get { return size * 2 - 1; } }
        public int TopRow { get { return anchorRow - (size - 1); } }
        public int BottomRow { get { return anchorRow + (size - 1); } }
    }

    Gear[] gears = new Gear[3]; // 0 = first/top, 1 = center, 2 = last/bottom
    bool hasWon = false;
    bool hasGeneratedOnce = false;
    bool isOpen = false;
    Font uiFont;
    Sprite gearSprite;
    Sprite arrowSprite;
    CursorLockMode _prevCursorLockState;
    bool _prevCursorVisible;

    GameObject minigameRoot;
    RectTransform boardRoot;
    Image inIcon;
    Image outIcon;
    Text statusText;
    GameObject winPanel;
    Text winText;

    public bool IsOpen { get { return isOpen; } }
    public bool IsCompleted { get { return hasWon; } }

    void Awake()
    {
        uiFont = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        gearSprite = customGearSprite != null ? customGearSprite : GenerateGearSprite(gearTextureSize, gearTeeth);
        arrowSprite = GenerateArrowSprite(arrowTextureSize);

        EnsureEventSystem();
        BuildUI();
        minigameRoot.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        // Spin any gear that is correctly connected to its neighbor.
        for (int i = 0; i < gears.Length; i++)
        {
            if (gears[i] != null && gears[i].isTurning && gears[i].graphic != null)
            {
                gears[i].graphic.Rotate(0f, 0f, turnSpeedDegPerSec * Time.deltaTime);
            }
        }
    }

    // ---------------------------------------------------------------
    // PUBLIC API — call these from your spawner/trigger script
    // ---------------------------------------------------------------

    /// <summary>
    /// Shows the popup. Resumes the in-progress puzzle if there is one;
    /// generates a brand new puzzle the very first time this is called, or
    /// whenever the previous puzzle had already been won.
    /// </summary>
    public void OpenMinigame()
    {
        if (!hasGeneratedOnce || hasWon)
        {
            NewPuzzle();
            hasGeneratedOnce = true;
        }
        minigameRoot.SetActive(true);
        isOpen = true;

        if (manageCursorState)
        {
            _prevCursorLockState = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Hides the popup without resetting anything — current gear sizes and
    /// positions are preserved and will be exactly as-is next time
    /// OpenMinigame() is called.
    /// </summary>
    public void CloseMinigame()
    {
        minigameRoot.SetActive(false);
        isOpen = false;

        if (manageCursorState)
        {
            Cursor.lockState = _prevCursorLockState;
            Cursor.visible = _prevCursorVisible;
        }
    }

    // ---------------------------------------------------------------
    // PUZZLE GENERATION
    // ---------------------------------------------------------------

    public void NewPuzzle()
    {
        hasWon = false;
        if (winPanel != null) winPanel.SetActive(false);

        int centerRow, firstRow, lastRow;
        GenerateValidRows(out firstRow, out centerRow, out lastRow);

        for (int i = 0; i < 3; i++)
        {
            if (gears[i] == null) gears[i] = new Gear();
            gears[i].size = 1;
            gears[i].graphic.rotation = Quaternion.identity;
        }
        gears[0].anchorRow = firstRow;
        gears[1].anchorRow = centerRow;
        gears[2].anchorRow = lastRow;

        for (int i = 0; i < 3; i++) UpdateGearVisual(i);
        RecomputeState();
    }

    /// <summary>
    /// Picks 3 rows (first &lt; center &lt; last) such that:
    ///   distance(first, IN=0)  &lt; distance(first, center)
    ///   distance(last, OUT=9)  &lt; distance(last, center)
    ///
    /// The anchor-to-anchor distance is also capped at maxAnchorDistance
    /// (default 4, i.e. an initial gap of at most 3 rows). This isn't just
    /// a difficulty tweak — it's what guarantees the puzzle is solvable.
    /// Each gear pair's size is independent EXCEPT the center gear, whose
    /// size is shared by both pairs, so a puzzle can look fine pair-by-pair
    /// and still have no size that satisfies both simultaneously. Capping
    /// both distances at 4 guarantees a solution always exists: setting the
    /// center gear to size 1 and the outer gears to size = their distance
    /// from the center (both then within the valid 1-4 range) always closes
    /// both gaps to exactly 0.
    /// </summary>
    void GenerateValidRows(out int firstRow, out int centerRow, out int lastRow)
    {
        int last = rowCount - 1;
        for (int attempt = 0; attempt < 500; attempt++)
        {
            int center = Random.Range(2, rowCount - 2); // leaves room on both sides

            int firstMax = Mathf.CeilToInt(center / 2f) - 1;
            int firstMin = Mathf.Max(0, center - maxAnchorDistance);
            int lastMin = Mathf.FloorToInt((last + center) / 2f) + 1;
            int lastMax = Mathf.Min(last, center + maxAnchorDistance);

            if (firstMin > firstMax || lastMin > lastMax) continue;

            int first = Random.Range(firstMin, firstMax + 1);
            int end = Random.Range(lastMin, lastMax + 1);

            firstRow = first;
            centerRow = center;
            lastRow = end;
            return;
        }

        // Fallback (should not normally trigger for rowCount = 10).
        centerRow = rowCount / 2;
        firstRow = 0;
        lastRow = last;
    }

    // ---------------------------------------------------------------
    // GAME LOGIC
    // ---------------------------------------------------------------

    public void OnGearClicked(int index)
    {
        if (hasWon) return;

        Gear g = gears[index];
        g.size = (g.size % 4) + 1; // 1->2->3->4->1, expands both up and down
        UpdateGearVisual(index);
        RecomputeState();
    }

    void RecomputeState()
    {
        int gap01 = gears[1].TopRow - gears[0].BottomRow - 1;
        int gap12 = gears[2].TopRow - gears[1].BottomRow - 1;

        bool overlap01 = gap01 < 0;
        bool overlap12 = gap12 < 0;
        bool touch01 = gap01 == 0;
        bool touch12 = gap12 == 0;

        gears[0].isTurning = touch01;
        gears[1].isTurning = touch01 || touch12;
        gears[2].isTurning = touch12;

        // IN lights up whenever the top gear is in contact with center —
        // touching exactly OR overlapping both count as "touching a gear".
        // OUT works the same way for the bottom gear. This is intentionally
        // more lenient than touch01/touch12 (which require an EXACT, non-
        // overlapping connection) — those still gate turning and the win
        // condition; this just answers "is contact happening at all?".
        bool inContact = gap01 <= 0;
        bool outContact = gap12 <= 0;
        if (inIcon != null) inIcon.color = inContact ? colorIn : colorIconInactive;
        if (outIcon != null) outIcon.color = outContact ? colorOut : colorIconInactive;

        SetGearColor(0, overlap01 ? colorOverlap : (touch01 ? colorConnected : colorNormal));
        SetGearColor(2, overlap12 ? colorOverlap : (touch12 ? colorConnected : colorNormal));

        Color midColor;
        if (overlap01 || overlap12) midColor = colorOverlap;
        else if (touch01 && touch12) midColor = colorConnected;
        else if (touch01 || touch12) midColor = colorPartial;
        else midColor = colorNormal;
        SetGearColor(1, midColor);

        bool[] covered = new bool[rowCount];
        for (int i = 0; i < 3; i++)
        {
            int top = Mathf.Max(0, gears[i].TopRow);
            int bottom = Mathf.Min(rowCount - 1, gears[i].BottomRow);
            for (int r = top; r <= bottom; r++) covered[r] = true;
        }
        int filledCount = 0;
        bool allFilled = true;
        for (int r = 0; r < rowCount; r++)
        {
            if (covered[r]) filledCount++; else allFilled = false;
        }

        if (statusText != null)
        {
            statusText.text = rowsFilledLabel + ": " + filledCount + " / " + rowCount;
        }

        bool win = touch01 && touch12 && allFilled;
        if (win && !hasWon)
        {
            hasWon = true;
            ShowWin();
        }
    }

    void SetGearColor(int index, Color c)
    {
        if (gears[index].image != null) gears[index].image.color = c;
    }

    void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (winText != null) winText.text = winMessageText;
        onMinigameComplete.Invoke(true);
    }

    // ---------------------------------------------------------------
    // VISUALS
    // ---------------------------------------------------------------

    // Y position (in boardRoot's top-center coordinate frame) of the center
    // of a given anchor row. This point never changes for a gear regardless
    // of its size, since growth is symmetric around the anchor.
    float CenterY(int anchorRow)
    {
        return -(anchorRow * cellHeight + cellHeight / 2f);
    }

    void UpdateGearVisual(int index)
    {
        Gear g = gears[index];

        // The gear's center never moves — only its scale changes — because
        // growth is symmetric (one row added above AND below per size step).
        g.root.anchoredPosition = new Vector2(0f, CenterY(g.anchorRow));

        float scale = g.RowSpan; // 1, 3, 5, or 7
        g.graphic.localScale = new Vector3(scale, scale, 1f);

        // The clickable area matches the visual gear once it's big enough,
        // but never shrinks below minClickDiameter — so a size-1 gear is
        // still easy to tap even though it's visually small.
        float visualDiameter = gearBaseDiameter * scale;
        float hitDiameter = Mathf.Max(visualDiameter, minClickDiameter);
        g.hitArea.sizeDelta = new Vector2(hitDiameter, hitDiameter);

        if (g.label != null)
        {
            g.label.gameObject.SetActive(showSizeLabel);
            if (showSizeLabel) g.label.text = string.Format(sizeLabelFormat, g.size);
        }
    }

    void BuildUI()
    {
        // --- Canvas & EventSystem ---
        GameObject canvasGO = new GameObject("GearPuzzleCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // For ScreenSpaceOverlay canvases, sortingOrder controls both draw
        // order AND raycast priority relative to any other overlay canvases
        // in the scene. Pinning this high ensures the popup — and, just as
        // importantly, its clicks — always wins over the host game's HUD.
        canvas.sortingOrder = canvasSortingOrder;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 1000f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Minigame root: everything below toggles with Open/CloseMinigame ---
        RectTransform minigameRootRect = CreateRect("MinigameRoot", canvasGO.transform);
        StretchFull(minigameRootRect);
        minigameRoot = minigameRootRect.gameObject;

        // --- Backdrop: full-screen, dims the game behind the popup, and
        // closes the popup when clicked (i.e. "click outside the panel"). ---
        RectTransform backdropRect = CreateRect("Backdrop", minigameRootRect);
        StretchFull(backdropRect);
        Image backdropImg = AddImage(backdropRect, backdropColor);
        Button backdropBtn = backdropRect.gameObject.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.targetGraphic = backdropImg;
        backdropBtn.onClick.AddListener(CloseMinigame);

        // --- Panel: fixed size, centered — this is "the game", everything
        // below is a child of it so clicks inside never reach the backdrop. ---
        RectTransform panel = CreateRect("Panel", minigameRootRect);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(panelWidth, panelHeight);
        AddImage(panel, panelColor);
        panel.gameObject.AddComponent<RectMask2D>(); // keep large gears/overflow inside the popup

        Transform root = panel;

        // --- Title ---
        RectTransform title = CreateRect("Title", root);
        SetTopCenterAnchor(title);
        title.anchoredPosition = new Vector2(0f, -24f);
        title.sizeDelta = new Vector2(panelWidth - 20f, 40f);
        AddText(title, titleText, titleFontSize, titleColor);

        // --- Board root (INVISIBLE — purely a logical/positioning frame) ---
        boardRoot = CreateRect("Board", root);
        SetTopCenterAnchor(boardRoot);
        boardRoot.anchoredPosition = new Vector2(0f, -110f);
        boardRoot.sizeDelta = new Vector2(boardWidth, rowCount * cellHeight);

        // --- IN marker (start of the table, above row 0) ---
        RectTransform inIconRect = CreateRect("InIcon", boardRoot);
        SetTopCenterAnchor(inIconRect);
        inIconRect.anchoredPosition = new Vector2(0f, 48f);
        inIconRect.sizeDelta = new Vector2(iconSize, iconSize);
        inIcon = AddImage(inIconRect, colorIconInactive);
        inIcon.sprite = customInIconSprite != null ? customInIconSprite : arrowSprite;

        RectTransform inTextRect = CreateRect("InText", boardRoot);
        SetTopCenterAnchor(inTextRect);
        inTextRect.anchoredPosition = new Vector2(0f, 14f);
        inTextRect.sizeDelta = new Vector2(boardWidth, 24f);
        AddText(inTextRect, inLabelText, markerLabelFontSize, colorIn);

        // --- OUT marker (end of the table, below the last row) ---
        RectTransform outTextRect = CreateRect("OutText", boardRoot);
        SetTopCenterAnchor(outTextRect);
        outTextRect.anchoredPosition = new Vector2(0f, -(rowCount * cellHeight) - 18f);
        outTextRect.sizeDelta = new Vector2(boardWidth, 24f);
        AddText(outTextRect, outLabelText, markerLabelFontSize, colorOut);

        RectTransform outIconRect = CreateRect("OutIcon", boardRoot);
        SetTopCenterAnchor(outIconRect);
        outIconRect.anchoredPosition = new Vector2(0f, -(rowCount * cellHeight) - 48f);
        outIconRect.sizeDelta = new Vector2(iconSize, iconSize);
        outIcon = AddImage(outIconRect, colorIconInactive);
        outIcon.sprite = customOutIconSprite != null ? customOutIconSprite : arrowSprite;

        // --- Gears ---
        for (int i = 0; i < 3; i++)
        {
            gears[i] = new Gear();
            CreateGearVisual(i);
        }

        // --- Status text ---
        RectTransform status = CreateRect("Status", root);
        SetTopCenterAnchor(status);
        status.anchoredPosition = new Vector2(0f, -110f - (rowCount * cellHeight) - 80f);
        status.sizeDelta = new Vector2(panelWidth - 20f, 40f);
        statusText = AddText(status, "", statusFontSize, statusColor);

        // --- Win overlay (contained within the panel) ---
        winPanel = CreateRect("WinPanel", root).gameObject;
        RectTransform winRect = winPanel.GetComponent<RectTransform>();
        StretchFull(winRect);
        AddImage(winRect, winOverlayColor);
        RectTransform winTextRect = CreateRect("WinText", winRect);
        StretchFull(winTextRect);
        winText = AddText(winTextRect, winMessageText, winFontSize, winTextColor);
        winPanel.SetActive(false);
    }

    void CreateGearVisual(int index)
    {
        Gear g = gears[index];

        // Root: fixed positioning anchor, never scaled or rotated. It has
        // zero size, so its top-center pivot collapses to a single point —
        // that point is always exactly the gear's anchor-row center.
        RectTransform root = CreateRect("Gear_" + index + "_Root", boardRoot);
        SetTopCenterAnchor(root);
        root.sizeDelta = Vector2.zero;
        g.root = root;

        // Graphic: the actual gear icon — this scales up with size and spins
        // when connected. Base size corresponds to a size-1 gear (1 row).
        RectTransform graphic = CreateRect("Graphic", root);
        graphic.anchorMin = graphic.anchorMax = graphic.pivot = new Vector2(0.5f, 0.5f);
        graphic.anchoredPosition = Vector2.zero;
        gearBaseDiameter = Mathf.Max(10f, cellHeight - 4f);
        graphic.sizeDelta = new Vector2(gearBaseDiameter, gearBaseDiameter);
        Image img = graphic.gameObject.AddComponent<Image>();
        img.sprite = gearSprite;
        img.color = colorNormal;
        img.raycastTarget = false; // clicks are handled by the hit area instead
        g.graphic = graphic;
        g.image = img;

        // Hit area: an invisible, always-at-least-minClickDiameter target,
        // centered on the same point as the graphic (center pivot, so its
        // size can change freely without shifting its position).
        RectTransform hitArea = CreateRect("HitArea", root);
        hitArea.anchorMin = hitArea.anchorMax = hitArea.pivot = new Vector2(0.5f, 0.5f);
        hitArea.anchoredPosition = Vector2.zero;
        hitArea.sizeDelta = new Vector2(minClickDiameter, minClickDiameter);
        Image hitImg = hitArea.gameObject.AddComponent<Image>();
        hitImg.color = new Color(0f, 0f, 0f, 0f); // fully transparent, still raycastable
        hitImg.raycastTarget = true;
        g.hitArea = hitArea;

        GearClickRelay relay = hitArea.gameObject.AddComponent<GearClickRelay>();
        relay.controller = this;
        relay.index = index;

        // Size label — stays a fixed, readable size regardless of gear scale
        // because it is NOT a child of the scaled graphic.
        RectTransform labelRect = CreateRect("Label", root);
        labelRect.anchorMin = labelRect.anchorMax = labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(70f, 24f);
        g.label = AddText(labelRect, "Size 1", gearSizeLabelFontSize, gearLabelColor);
        g.label.gameObject.SetActive(showSizeLabel);
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    // ---------------------------------------------------------------
    // Procedural graphics — used unless you assign your own Sprites above
    // ---------------------------------------------------------------

    /// <summary>Draws a toothed gear silhouette (with a center hole) into a texture.</summary>
    Sprite GenerateGearSprite(int px, int teethCount)
    {
        Texture2D tex = new Texture2D(px, px, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pix = new Color[px * px];
        float c = px / 2f;
        float outerR = px / 2f - 2f;
        float toothInner = outerR * 0.80f;
        float holeR = outerR * 0.22f;
        float twoPi = Mathf.PI * 2f;
        float toothAngle = twoPi / teethCount;
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < px; y++)
        {
            for (int x = 0; x < px; x++)
            {
                float dx = x - c;
                float dy = y - c;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float ang = Mathf.Atan2(dy, dx);
                if (ang < 0f) ang += twoPi;
                float angInTooth = ang % toothAngle;
                bool toothZone = angInTooth < toothAngle * 0.5f;
                float limit = toothZone ? outerR : toothInner;

                bool filled = dist <= limit && dist >= holeR;
                pix[y * px + x] = filled ? Color.white : clear;
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    /// <summary>Draws a solid downward-pointing arrow/triangle into a texture.</summary>
    Sprite GenerateArrowSprite(int px)
    {
        Texture2D tex = new Texture2D(px, px, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pix = new Color[px * px];
        Color clear = new Color(0f, 0f, 0f, 0f);
        float half = px / 2f;

        // Row 0 = bottom of the rendered sprite. Apex (width 0) at the
        // bottom, full width at the top => a triangle that points down.
        for (int y = 0; y < px; y++)
        {
            float frac = y / (float)(px - 1);
            float halfWidth = half * frac;
            for (int x = 0; x < px; x++)
            {
                bool filled = Mathf.Abs(x - half) <= halfWidth;
                pix[y * px + x] = filled ? Color.white : clear;
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    // ---------------------------------------------------------------
    // Small RectTransform helpers
    // ---------------------------------------------------------------

    RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    void SetTopCenterAnchor(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    Image AddImage(RectTransform rt, Color color)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Text AddText(RectTransform rt, string content, int fontSize, Color color)
    {
        Text txt = rt.gameObject.AddComponent<Text>();
        txt.text = content;
        txt.font = uiFont;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        return txt;
    }
}

/// <summary>
/// Tiny relay so a gear's Image can receive pointer clicks and forward
/// them to the controller. Kept in the same file for a single-file drop-in.
/// </summary>
public class GearClickRelay : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public GearPuzzleGame controller;
    [HideInInspector] public int index;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null) controller.OnGearClicked(index);
    }
}