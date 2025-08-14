using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class ButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover scale settings")]
    [SerializeField] private float hoverScale = 1.1f;      // ���콺 ���� �� ��ǥ ������(����)
    [SerializeField] private float duration = 0.12f;       // Ʈ�� �ð�
    [SerializeField] private AnimationCurve ease = null;   // ��¡ Ŀ��

    private Vector3 baseScale;
    private Coroutine tween;
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        if (ease == null) ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        baseScale = transform.localScale; // �ʱ� ������ ������
        videoPlayer = GetComponent<VideoPlayer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(Vector3.one * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(Vector3.one); // 1.0���� ����
    }

    private void ScaleTo(Vector3 target)
    {
        if (tween != null) StopCoroutine(tween);
        tween = StartCoroutine(ScaleRoutine(target));
    }

    private System.Collections.IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            float e = ease.Evaluate(u);
            transform.localScale = Vector3.LerpUnclamped(start, target, e);
            yield return null;
        }

        transform.localScale = target;
        tween = null;
    }

    private void OnEnable()
    {
        if (videoPlayer != null && !string.IsNullOrEmpty(videoPlayer.url))
        {               
            StartCoroutine(VideoManager.Instance.RestartFromStart(videoPlayer));
        }
    }

    private void OnDisable()
    {
        if (tween != null) StopCoroutine(tween);
        transform.localScale = baseScale; // ��Ȱ��ȭ �� ���� �����Ϸ� ����
    }
}
