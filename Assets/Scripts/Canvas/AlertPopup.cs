using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class AlertPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject popupRoot;

    [SerializeField]
    private Text messageText;

    [SerializeField] private GameObject themedFeedbackPrefab;
    private TMP_Text themedMessage;

    [SerializeField, Min(0f)]
    private float visibleDurationSeconds = 1f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        popupRoot?.SetActive(false);
        if (themedFeedbackPrefab != null)
        {
            GameObject instance = Instantiate(themedFeedbackPrefab);
            popupRoot = instance;
            themedMessage = instance.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(text => text.name == "Feedback Message");
            Button dismiss = instance.GetComponentInChildren<Button>(true);
            if (dismiss != null) dismiss.onClick.AddListener(Hide);
            instance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (themedMessage != null && popupRoot != null) Destroy(popupRoot);
    }

    private void OnDisable()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = null;
        popupRoot?.SetActive(false);
    }

    public void Show(string message)
    {
        if (popupRoot == null || (messageText == null && themedMessage == null))
        {
            return;
        }

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        if (themedMessage != null) themedMessage.text = message ?? string.Empty;
        else messageText.text = message ?? string.Empty;
        popupRoot.SetActive(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        popupRoot?.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDurationSeconds);
        hideRoutine = null;
        popupRoot.SetActive(false);
    }
}
