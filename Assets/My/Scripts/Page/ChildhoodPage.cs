using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChildhoodSetting
{
    public ImageSetting backgroundImage;
    public ButtonSetting backToIdleButton;
    public ButtonSetting backButton;

    public ImageSetting childhoodImage;
    public ImageSetting childhoodExplainImage;

    public ButtonSetting childhood_1;
    public ButtonSetting childhood_2;
    public ButtonSetting childhood_3;
    public ButtonSetting childhood_4;
}

public class ChildhoodPage : BasePage<ChildhoodSetting>
{
    // JSON ���
    protected override string JsonPath => "JSON/ChildhoodSetting.json";

    // ������ �� ������ ����
    protected override async Task BuildContentAsync()
    {
        // ������ ���� �̹���
        if (Setting.childhoodImage != null)
            await UIManager.Instance.CreateImageAsync(Setting.childhoodImage, gameObject, default(CancellationToken));
        if (Setting.childhoodExplainImage != null)
            await UIManager.Instance.CreateImageAsync(Setting.childhoodExplainImage, gameObject, default(CancellationToken));

        // ������ ���� ��ư��
        await WireButton(Setting.childhood_1, "[ChildhoodPage] childhood_1 clicked.");
        await WireButton(Setting.childhood_2, "[ChildhoodPage] childhood_2 clicked.");
        await WireButton(Setting.childhood_3, "[ChildhoodPage] childhood_3 clicked.");
        await WireButton(Setting.childhood_4, "[ChildhoodPage] childhood_4 clicked.");
    }

    // ��ư ���� �� �̺�Ʈ ����
    // TODO: ���� �α� �޽��� ��� ���� ������ ��ȯ �������� ���� �ʿ�
    private async Task WireButton(ButtonSetting bs, string logMessage)
    {
        if (bs == null) return;

        var created = await UIManager.Instance.CreateSingleButtonAsync(bs, gameObject, default(CancellationToken));
        var go = created.button;
        if (go != null && go.TryGetComponent<Button>(out var btn))
        {
            btn.onClick.AddListener(() => Debug.Log(logMessage));
        }
    }
}
