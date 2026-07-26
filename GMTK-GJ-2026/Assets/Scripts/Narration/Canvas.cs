using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Narration
{
    public class Canvas : MonoBehaviour, IPointerDownHandler
    {
        [Header("References")]
        [SerializeField] private TMP_Text narrationText;

        private Coroutine currentRoutine;

        private InputAction _interactAction;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private float fadeInDuration = 0.5f;

        [SerializeField]
        private float fadeOutDuration = 0.25f;

        private Coroutine fadeRoutine;

        private bool _advanceRequested;

        private void Awake()
        {
            _interactAction = InputSystem.actions.FindAction("Interact");
            _canvasGroup.alpha = 0f;
            narrationText.enabled = false;
        }

        /// <summary>
        /// Starts playing a narration script.
        /// </summary>
        public IEnumerator PlayScript(Script script)
        {
            if (script == null || script.ScriptList == null)
                yield break;

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(PlayScriptRoutine(script));
            yield return currentRoutine;
        }

        public void SetActive(bool active)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            narrationText.enabled = active;

            _canvasGroup.interactable = active;
            _canvasGroup.blocksRaycasts = active;

            float duration = active ? fadeInDuration : fadeOutDuration;
            float targetAlpha = active ? 1f : 0f;

            fadeRoutine = StartCoroutine(FadeBackground(targetAlpha, duration));
        }

        private IEnumerator FadeBackground(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;

            // Handle instant fades.
            if (duration <= 0f)
            {
                _canvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                _canvasGroup.alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    elapsed / duration);

                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        private IEnumerator PlayScriptRoutine(Narration.Script script)
        {
            narrationText.text = "";

            foreach (var line in script.ScriptList)
            {
                yield return StartCoroutine(TypeLine(line));

                // Wait for the player to click before continuing.
                yield return StartCoroutine(WaitForAdvanceInput());
            }

            currentRoutine = null;
        }

        private IEnumerator WaitForAdvanceInput()
        {
            // Prevent the same click/input that opened the dialogue from advancing.
            yield return null;

            _advanceRequested = false;

            while (!_advanceRequested && !_interactAction.WasPressedThisFrame())
            {
                yield return null;
            }

            _advanceRequested = false;
        }

        private IEnumerator TypeLine(TextData data)
        {
            narrationText.text = "";

            string text = data.ScriptLine;

            if (string.IsNullOrEmpty(text))
            {
                yield return null;
                yield break;
            }

            // Instantly display if no typing time is specified.
            if (data.TimeToType <= 0f)
            {
                narrationText.text = text;
                yield return null;
                yield break;
            }

            float elapsed = 0f;
            int previousCharacterCount = 0;

            while (elapsed < data.TimeToType)
            {
                elapsed += Time.deltaTime;

                float percent = Mathf.Clamp01(elapsed / data.TimeToType);
                int charactersToShow = Mathf.FloorToInt(percent * text.Length);

                if (charactersToShow != previousCharacterCount)
                {
                    narrationText.text = text.Substring(0, charactersToShow);
                    previousCharacterCount = charactersToShow;
                }

                yield return null;
            }

            // Ensure the full line is shown.
            narrationText.text = text;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _advanceRequested = true;
        }
    }
}