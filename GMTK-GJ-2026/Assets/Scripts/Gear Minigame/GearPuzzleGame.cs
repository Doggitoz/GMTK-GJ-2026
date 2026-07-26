using System.Collections.Generic;
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
/// - There is an invisible 1-column table. The rows and the column itself
///   are not drawn.
/// - A fixed, non-interactive gear sits at each end — "IN" at the top and
///   "OUT" at the bottom. These never change size and can't be clicked.
/// - 3 gears in between are player-controlled and spawn on random rows.
///   Each has 4 sizes; clicking one cycles size 1 -> 2 -> 3 -> 4 -> 1.
///   Growing a gear expands it symmetrically: one extra row above AND one
///   extra row below per size step. Visually, the gear graphic scales up
///   around its fixed center point.
/// - Gears may hang off the top/bottom edge of the table (no clamping on
///   position).
/// - A gear visually turns (the gear graphic itself spins) whenever it is
///   touching a neighbor exactly — no gap, no overlap. This applies to the
///   fixed IN/OUT gears too, once the nearest player gear reaches them.
/// - The player wins when the whole chain — IN, all 3 player gears, and
///   OUT — is connected end to end: every adjacent pair touching but not
///   overlapping. (This automatically means every row between IN and OUT
///   is filled, so there's no separate "fill the rows" check anymore.)
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
    // Leave empty for a plain colored rectangle (tinted by panelColor).
    // Assign your own Sprite to use it as the panel's background instead —
    // panelColor still applies as a tint on top of it (white = no tint).
    public Sprite customPanelSprite;
    // If your sprite has 9-slice borders set up in its import settings,
    // enable this so the panel stretches without distorting the borders.
    public bool panelSpriteIsSliced = false;
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
    // Colors for the fixed IN/OUT gears — independent from the player
    // gears' colors above, so you can make them visually distinct.
    public Color colorFixedGearNormal = new Color(0.55f, 0.55f, 0.60f);
    public Color colorFixedGearConnected = new Color(0.40f, 0.85f, 0.50f);
    public Color colorFixedGearOverlap = new Color(0.90f, 0.30f, 0.30f);

    [Header("Turning")]
    public float turnSpeedDegPerSec = 140f;

    [Header("Click / Tap")]
    // Small gears (size 1) are visually tiny — this guarantees every
    // PLAYER gear always has at least this large an (invisible) tap/click
    // target, centered on the gear, regardless of its current visual size.
    // The fixed IN/OUT gears have no click target at all.
    public float minClickDiameter = 70f;

    [Header("Gear Graphics")]
    // Leave empty to use the built-in procedurally generated gear icon.
    // Assign your own Sprite here to fully replace it — used for the
    // player gears AND the fixed IN/OUT gears, so everything matches.
    public Sprite customGearSprite;
    public int gearTextureSize = 128;
    public int gearTeeth = 10;

    [Header("Text Content")]
    public string titleText = "GEAR PUZZLE";
    public string winMessageText = "YOU WIN!\nAll gears connected.";
    public bool showSizeLabel = true;
    public string sizeLabelFormat = "Size {0}";

    [Header("Fonts & Sizes")]
    // Leave empty to use Unity's built-in default font.
    public Font customFont;
    public int titleFontSize = 26;
    public int winFontSize = 40;
    public int gearSizeLabelFontSize = 13;

    [Header("Text Colors")]
    public Color titleColor = Color.white;
    public Color gearLabelColor = Color.black;
    public Color winOverlayColor = new Color(0f, 0f, 0f, 0.65f);
    public Color winTextColor = Color.white;

    [Header("Completion Callback")]
    // Fires with `true` the instant the puzzle is solved. Hook your other
    // script's method to this in the Inspector, or via
    // onMinigameComplete.AddListener(...) in code.
    public BoolUnityEvent onMinigameComplete = new BoolUnityEvent();
    // Fires whenever the popup closes, for ANY reason — clicking outside,
    // walking away, or auto-closing after a win. Useful for a spawner to
    // reliably know "the popup is no longer open" regardless of which path
    // caused it, without having to guess.
    public UnityEvent onMinigameClosed = new UnityEvent();

    [SerializeField] private FMODUnity.EventReference gearSoundEvent;
    private FMOD.Studio.EventInstance gearSoundInstance;
    [SerializeField] private FMODUnity.EventReference gearShiftSoundEvent;

    [System.Serializable]
    public class BoolUnityEvent : UnityEvent<bool> { }

    float gearBaseDiameter; // visual diameter of a size-1 gear, set in BuildUI

    // ---- internal state ----
    class Gear
    {
        public int anchorRow;
        public int size = 1; // 1..4 for player gears; always 1 for fixed gears
        public bool isFixed;           // true for the IN/OUT gears
        public RectTransform root;     // fixed position anchor (never moves)
        public RectTransform graphic;  // the gear image — this is what scales & spins
        public RectTransform hitArea;  // invisible click target (player gears only)
        public Image image;
        public Text label;
        public bool isTurning;

        public int RowSpan { get { return size * 2 - 1; } }
        public int TopRow { get { return anchorRow - (size - 1); } }
        public int BottomRow { get { return anchorRow + (size - 1); } }
    }

    Gear[] gears = new Gear[3]; // 0 = first/top, 1 = center, 2 = last/bottom (player-controlled)
    Gear inGear;                // fixed, above gears[0]
    Gear outGear;                // fixed, below gears[2]

    bool hasWon = false;
    bool hasGeneratedOnce = false;
    bool isOpen = false;
    Font uiFont;
    Sprite gearSprite;
    CursorLockMode _prevCursorLockState;
    bool _prevCursorVisible;

    GameObject minigameRoot;
    RectTransform boardRoot;
    GameObject winPanel;
    Text winText;

    public bool IsOpen { get { return isOpen; } }
    public bool IsCompleted { get { return hasWon; } }

    void Awake()
    {
        uiFont = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        gearSprite = customGearSprite != null ? customGearSprite : GenerateGearSprite(gearTextureSize, gearTeeth);

        EnsureEventSystem();
        BuildUI();
        minigameRoot.SetActive(false);

        gearSoundInstance = FMODUnity.RuntimeManager.CreateInstance(gearSoundEvent);
    }

    void Update()
    {
        if (!isOpen) return;

        // Spin any gear that is correctly connected to a neighbor.
        for (int i = 0; i < gears.Length; i++)
        {
            if (gears[i] != null && gears[i].isTurning && gears[i].graphic != null)
                gears[i].graphic.Rotate(0f, 0f, turnSpeedDegPerSec * Time.deltaTime);
        }
        if (inGear != null && inGear.isTurning) inGear.graphic.Rotate(0f, 0f, turnSpeedDegPerSec * Time.deltaTime);
        if (outGear != null && outGear.isTurning) outGear.graphic.Rotate(0f, 0f, turnSpeedDegPerSec * Time.deltaTime);
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
        gearSoundInstance.start();
    }

    /// <summary>
    /// Hides the popup without resetting anything — current gear sizes and
    /// positions are preserved and will be exactly as-is next time
    /// OpenMinigame() is called.
    /// </summary>
    public void CloseMinigame()
    {
        bool wasOpen = isOpen;

        minigameRoot.SetActive(false);
        isOpen = false;
        gearSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        if (manageCursorState)
        {
            Cursor.lockState = _prevCursorLockState;
            Cursor.visible = _prevCursorVisible;
        }

        if (wasOpen) onMinigameClosed.Invoke();
    }

    // ---------------------------------------------------------------
    // PUZZLE GENERATION
    // ---------------------------------------------------------------

    public void NewPuzzle()
    {
        hasWon = false;
        if (winPanel != null) winPanel.SetActive(false);

        int centerRow, firstRow, lastRow;
        GenerateSolvableLayout(out firstRow, out centerRow, out lastRow);

        for (int i = 0; i < 3; i++)
        {
            if (gears[i] == null) gears[i] = new Gear();
            gears[i].size = 1;
            gears[i].graphic.rotation = Quaternion.identity;
        }
        gears[0].anchorRow = firstRow;
        gears[1].anchorRow = centerRow;
        gears[2].anchorRow = lastRow;

        if (inGear != null) inGear.graphic.rotation = Quaternion.identity;
        if (outGear != null) outGear.graphic.rotation = Quaternion.identity;

        for (int i = 0; i < 3; i++) UpdateGearVisual(i);
        RecomputeState();
    }

    // The IN gear always sits exactly 1 row above where gears[0] could
    // start (row -1). The OUT gear's row is derived from rowCount, chosen
    // to have the SAME parity as InAnchorRow (see GenerateSolvableLayout
    // for why that parity match matters) — normally rowCount+1, or
    // rowCount+2 if rowCount is odd.
    const int InAnchorRow = -1;
    int OutAnchorRow { get { return (rowCount % 2 == 0) ? rowCount + 1 : rowCount + 2; } }

    /// <summary>
    /// Constructs a layout that is GUARANTEED solvable, by working backwards
    /// from a hidden target solution instead of placing gears randomly and
    /// hoping a solution exists.
    ///
    /// WHY THIS IS NECESSARY: a player gear's row-span is always ODD
    /// (1, 3, 5, or 7 — one row added above AND below per size step). Three
    /// such gears can never exactly bridge a fixed, EVEN-length gap with
    /// zero slack — so IN and OUT can't simply sit at "one row outside the
    /// board" on both ends (that gap would be even) or no chain of 3
    /// odd-length gears could ever exactly fill it. Placing IN and OUT so
    /// the gap between them has the SAME parity as a sum of three odd
    /// numbers (which is always odd) resolves this — hence OutAnchorRow
    /// being chosen to match InAnchorRow's parity above.
    ///
    /// With that fixed, we pick a random valid (s0, s1, s2) size combo —
    /// the sizes that WOULD win the puzzle — then derive the anchor rows
    /// that make exactly that combo the (hidden) solution. The center size
    /// s1 is kept >= 2, which as a side effect also guarantees gears[0] and
    /// gears[2] end up closer to IN/OUT than to the center gear, preserving
    /// the original spawn "flavor". The player still starts at size 1 for
    /// all three and has to discover the solution themselves.
    /// </summary>
    void GenerateSolvableLayout(out int firstRow, out int centerRow, out int lastRow)
    {
        int outAnchor = OutAnchorRow;

        // Total of the 3 player gears' sizes required to exactly bridge
        // from IN to OUT, derived from how far apart they are.
        int targetSizeSum = Mathf.Clamp((outAnchor - InAnchorRow) / 2 + 1, 4, 12);

        List<Vector3Int> combos = new List<Vector3Int>();
        for (int s0 = 1; s0 <= 4; s0++)
            for (int s1 = 2; s1 <= 4; s1++) // >=2 preserves "closer to IN/OUT than center"
                for (int s2 = 1; s2 <= 4; s2++)
                    if (s0 + s1 + s2 == targetSizeSum)
                        combos.Add(new Vector3Int(s0, s1, s2));

        Vector3Int combo = combos.Count > 0
            ? combos[Random.Range(0, combos.Count)]
            : new Vector3Int(1, 3, 3); // fallback for extreme custom rowCount values

        int s0f = combo.x, s1f = combo.y, s2f = combo.z;

        firstRow = InAnchorRow + s0f;
        centerRow = InAnchorRow + 2 * s0f + s1f - 1;
        lastRow = outAnchor - s2f;
    }

    // ---------------------------------------------------------------
    // GAME LOGIC
    // ---------------------------------------------------------------

    public void OnGearClicked(int index)
    {
        FMODUnity.RuntimeManager.PlayOneShot(gearShiftSoundEvent, transform.position);
        if (hasWon) return;

        Gear g = gears[index];
        g.size = (g.size % 4) + 1; // 1->2->3->4->1, expands both up and down
        UpdateGearVisual(index);
        RecomputeState();
    }

    void RecomputeState()
    {
        int gapIn = gears[0].TopRow - inGear.BottomRow - 1;
        int gap01 = gears[1].TopRow - gears[0].BottomRow - 1;
        int gap12 = gears[2].TopRow - gears[1].BottomRow - 1;
        int gapOut = outGear.TopRow - gears[2].BottomRow - 1;

        bool overlapIn = gapIn < 0;
        bool overlap01 = gap01 < 0;
        bool overlap12 = gap12 < 0;
        bool overlapOut = gapOut < 0;

        bool touchIn = gapIn == 0;
        bool touch01 = gap01 == 0;
        bool touch12 = gap12 == 0;
        bool touchOut = gapOut == 0;

        inGear.isTurning = touchIn;
        gears[0].isTurning = touchIn || touch01;
        gears[1].isTurning = touch01 || touch12;
        gears[2].isTurning = touch12 || touchOut;
        outGear.isTurning = touchOut;

        SetGearColor(inGear, overlapIn ? colorFixedGearOverlap : (touchIn ? colorFixedGearConnected : colorFixedGearNormal));
        SetGearColor(gears[0], (overlapIn || overlap01) ? colorOverlap : ((touchIn || touch01) ? colorConnected : colorNormal));
        SetGearColor(gears[2], (overlap12 || overlapOut) ? colorOverlap : ((touch12 || touchOut) ? colorConnected : colorNormal));
        SetGearColor(outGear, overlapOut ? colorFixedGearOverlap : (touchOut ? colorFixedGearConnected : colorFixedGearNormal));

        Color midColor;
        if (overlap01 || overlap12) midColor = colorOverlap;
        else if (touch01 && touch12) midColor = colorConnected;
        else if (touch01 || touch12) midColor = colorPartial;
        else midColor = colorNormal;
        SetGearColor(gears[1], midColor);

        bool win = touchIn && touch01 && touch12 && touchOut;
        if (win && !hasWon)
        {
            hasWon = true;
            ShowWin();
        }
    }

    void SetGearColor(Gear g, Color c)
    {
        if (g != null && g.image != null) g.image.color = c;
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
        gearBaseDiameter = Mathf.Max(10f, cellHeight - 4f);

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
        Image panelImg = AddImage(panel, panelColor);
        if (customPanelSprite != null)
        {
            panelImg.sprite = customPanelSprite;
            panelImg.type = panelSpriteIsSliced ? Image.Type.Sliced : Image.Type.Simple;
        }
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

        // --- IN gear (fixed, above the player gears) ---
        inGear = CreateFixedGear(InAnchorRow, "InGear");

        // --- OUT gear (fixed, below the player gears) ---
        outGear = CreateFixedGear(OutAnchorRow, "OutGear");

        // --- Player gears ---
        for (int i = 0; i < 3; i++)
        {
            gears[i] = new Gear();
            CreateGearVisual(i);
        }

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

    /// <summary>
    /// Creates one of the two fixed, non-interactive end gears (IN/OUT).
    /// Same visual system as player gears (sprite, color states, spin) but
    /// with no hit area, no size label, and a position/size that never
    /// changes after creation.
    /// </summary>
    Gear CreateFixedGear(int anchorRow, string name)
    {
        Gear g = new Gear();
        g.isFixed = true;
        g.anchorRow = anchorRow;
        g.size = 1;

        RectTransform root = CreateRect(name + "_Root", boardRoot);
        SetTopCenterAnchor(root);
        root.sizeDelta = Vector2.zero;
        root.anchoredPosition = new Vector2(0f, CenterY(anchorRow));
        g.root = root;

        RectTransform graphic = CreateRect("Graphic", root);
        graphic.anchorMin = graphic.anchorMax = graphic.pivot = new Vector2(0.5f, 0.5f);
        graphic.anchoredPosition = Vector2.zero;
        graphic.sizeDelta = new Vector2(gearBaseDiameter, gearBaseDiameter);
        Image img = graphic.gameObject.AddComponent<Image>();
        img.sprite = gearSprite;
        img.color = colorFixedGearNormal;
        img.raycastTarget = false; // not interactive — no hit area at all
        g.graphic = graphic;
        g.image = img;

        return g;
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
    // Procedural graphics — used unless you assign your own Sprite above
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