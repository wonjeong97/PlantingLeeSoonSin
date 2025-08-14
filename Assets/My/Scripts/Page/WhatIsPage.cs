using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class WhatIsSetting
{
    public ImageSetting backgroundImage;
    public ButtonSetting backToIdleButton;
    public ButtonSetting backButton;

    public VideoSetting video;
}

public class WhatIsPage : BasePage<WhatIsSetting>
{
    protected override string JsonPath => "JSON/WhatIsSetting.json";

    private GameObject videoPlayer;

    protected override async Task BuildContentAsync()
    {
        // ������ ����: ���� �÷��̾� ����
        videoPlayer = await UIManager.Instance.CreateVideoPlayerAsync(
            Setting.video, gameObject, default(CancellationToken));
    }

    private void OnEnable()
    {
        // �ٽ� Ȱ��ȭ�� �� �׻� ù �����Ӻ��� ���
        if (videoPlayer != null && videoPlayer.TryGetComponent<VideoPlayer>(out var vp))
        {
            StartCoroutine(VideoManager.Instance.RestartFromStart(vp));
        }
    }
}
