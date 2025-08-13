using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    /// <summary>
    /// StreamingAssets ��� ��θ� file:/// ���� URL�� ��ȯ
    /// ��: "Video/Test/TestVideo.webm" -> "file:///D:/.../StreamingAssets/Video/Test/TestVideo.webm"
    /// </summary>
    public string BuildStreamingUrl(string relative)
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, relative).Replace("\\", "/");
        return new Uri(fullPath).AbsoluteUri;
    }

    /// <summary>
    /// ���� ��Ÿ�ӿ��� webm ��� ���ɼ��� ������ ���������� �Ǵ�
    /// - Windows(Editor/Player): false (Media Foundation ��ο��� webm ���� ����)
    /// - Android, Linux: true (��ü�� ����. ��, ����̽�/�ڵ��� ���� ����)
    /// - �� ��: ���������� false
    /// </summary>
    public bool IsWebmLikelySupported()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return false;
#elif UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// ��� ���(�밳 .webm)�� �޾�, ��Ÿ���� webm�� �� ���� �� ������ .mp4�� ��ü �õ�
    /// </summary>
    public string ResolvePlayableUrl(string relativePathPossiblyWebm)
    {
        // webm �ƴϸ� �״��
        if (!relativePathPossiblyWebm.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
            return BuildStreamingUrl(relativePathPossiblyWebm);

        // webm ���� OK �� �״��
        if (IsWebmLikelySupported())
            return BuildStreamingUrl(relativePathPossiblyWebm);

        // webm ���� X �� ���� �̸��� mp4�� ������ �װɷ� ��ü
        var mp4Relative = Path.ChangeExtension(relativePathPossiblyWebm, ".mp4");

        // ����: Android���� File.Exists�� StreamingAssets Ȯ���� �Ұ�.
        // ���� ������/�����쿡���� ���� ���� üũ. (������/�����쿡���� ������ �ʿ��ϹǷ� ���)
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        var mp4Full = Path.Combine(Application.streamingAssetsPath, mp4Relative).Replace("\\", "/");
        if (File.Exists(mp4Full))
            return BuildStreamingUrl(mp4Relative);
#endif

        // ��ü ���� �� ���� webm URL �״��(������ ������ �α����� Ȯ��)
        return BuildStreamingUrl(relativePathPossiblyWebm);
    }
    void OnError(VideoPlayer src, string msg) => Debug.LogError($"[VideoPlayer] error: {msg}");
    void OnPrepared(VideoPlayer src) { }

    /// <summary>
    /// VideoPlayer �غ� �� ���. ���� �� false ��ȯ. (Ÿ�Ӿƿ�/��� ��ū/���� �̺�Ʈ ó�� ����)
    /// </summary>
    public async Task<bool> PrepareAndPlayAsync(
        VideoPlayer vp,
        string url,
        AudioSource audio,
        float volume,
        CancellationToken token,
        double timeoutSeconds = 10.0)
    {
        vp.errorReceived += OnError;
        vp.prepareCompleted += OnPrepared;

        try
        {
            vp.source = VideoSource.Url;
            vp.url = url;
            vp.isLooping = true;
            vp.playOnAwake = false;

            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.EnableAudioTrack(0, true);
            vp.SetTargetAudioSource(0, audio);
            audio.playOnAwake = false;
            audio.loop = true;
            audio.volume = Mathf.Clamp01(volume);

            vp.Prepare();

            var start = Time.realtimeSinceStartupAsDouble;
            while (!vp.isPrepared)
            {
                if (token.IsCancellationRequested) return false;
                if (Time.realtimeSinceStartupAsDouble - start > timeoutSeconds)
                {
                    Debug.LogError("[VideoPlayer] prepare timeout");
                    return false;
                }
                await Task.Yield();
            }

            vp.Play();
            if (audio.volume > 0f) audio.Play();
            return true;
        }
        finally
        {
            // �̺�Ʈ �ߺ� ��� ����
            vp.errorReceived -= OnError;
            vp.prepareCompleted -= OnPrepared;
        }
    }

    /// <summary>
    /// RawImage + VideoPlayer ���տ��� ��ư ũ�⿡ �´� RenderTexture�� ����/����
    /// ��ȯ��: ������ RenderTexture (�ʿ�� ���� å���� ȣ���� Ȥ�� �ı� �������� ó��)
    /// </summary>
    public RenderTexture WireRawImageAndRenderTexture(VideoPlayer vp, UnityEngine.UI.RawImage raw, Vector2Int size)
    {
        int rtW = Mathf.Max(2, size.x);
        int rtH = Mathf.Max(2, size.y);
        var rtex = new RenderTexture(rtW, rtH, 0);
        rtex.Create();

        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rtex;
        if (raw) raw.texture = rtex;

        return rtex;
    }

    public IEnumerator RestartFromStart(VideoPlayer vp)
    {
        // �ҽ��� ������ ���� ����
        if (vp.clip == null && string.IsNullOrEmpty(vp.url))
        {
            Debug.LogWarning("[VideoPlayerManager] No clip/url set on VideoPlayer.");
            yield break;
        }
        vp.Stop();
        vp.Prepare();
        while (!vp.isPrepared)
            yield return null;

        vp.time = 0.0;

        try { vp.frame = 0; } catch { /* �÷���/�ҽ��� ���� ���� ���� */ }

        vp.Play();

        // (����) ù �������� ������ ǥ�õ� ������ ��� ����ϰ� �ʹٸ�:
        // while (!vp.isPlaying) yield return null;
        // yield return new WaitForEndOfFrame();
    }
}
