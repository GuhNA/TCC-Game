using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class AlertPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject popupRoot;

    [SerializeField]
    private Text messageText;

    [SerializeField, Min(0f)]
    private float visibleDurationSeconds = 1f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        popupRoot?.SetActive(false);
    }

    private void OnDisable()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = null;
    }

    public void Show(string message)
    {
        if (popupRoot == null || messageText == null)
        {
            return;
        }

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        messageText.text = message ?? string.Empty;
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
