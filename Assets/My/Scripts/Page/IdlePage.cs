using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class IdleSetting
{
    public ImageSetting backgroundImage;
    public TextSetting titleText;
    public ButtonSetting startButton;
    public PageSetting menuPage;
}

public class IdlePage : BasePage<IdleSetting>
{
    // JSON ��θ� ����
    protected override string JsonPath => "JSON/IdleSetting.json";   

    // ������ ���� ������(Ÿ��Ʋ �ؽ�Ʈ, ���� ��ư)�� ����
    protected override async Task BuildContentAsync()
    {
        // Ÿ��Ʋ �ؽ�Ʈ
        await UIManager.Instance.CreateSingleTextAsync(
            Setting.titleText, gameObject, default(CancellationToken));

        // ���� ��ư
        var created = await UIManager.Instance.CreateSingleButtonAsync(
            Setting.startButton, gameObject, default(CancellationToken));

        var startGO = created.button;
        if (startGO != null && startGO.TryGetComponent<Button>(out var startBtn))
        {
            startBtn.onClick.AddListener(async () =>
            {
                // ���̵� �ƿ� �� IdlePage ��Ȱ��ȭ
                await FadeManager.Instance.FadeOutAsync(JsonLoader.Instance.Settings.fadeTime, true);
                gameObject.SetActive(false);

                // �޴� ������ ���� �� ǥ��
                GameObject parent = UIManager.Instance.mainBackground;
                GameObject menuPage = await UIManager.Instance.CreatePageAsync(Setting.menuPage, parent);
                if (menuPage != null)
                {
                    menuPage.AddComponent<MenuPage>();
                }
            });
        }
    }
}