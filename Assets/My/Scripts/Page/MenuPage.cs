using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MenuSetting
{
    public ImageSetting backgroundImage;
    public ButtonSetting backToIdleButton;

    public ButtonSetting whatIsButton;       // �÷��� �̼����̶�
    public ButtonSetting childhoodButton;    // ����
    public ButtonSetting youthButton;        // û��
    public ButtonSetting primeOfLifeButton;  // ���
    public ButtonSetting lastButton;         // ����
    public ButtonSetting deathButton;        // ����
    public ButtonSetting afterDeathButton;   // ����

    public PageSetting whatIsPage;       // �÷��� �̼����̶� ������
    public PageSetting childhoodPage;    // ���� ������
    public PageSetting youthPage;        // û�� ������
    public PageSetting primeOfLifePage;  // ��� ������    
    public PageSetting lastPage;         // ���� ������
    public PageSetting deathPage;        // ���� ������
    public PageSetting afterDeathPage;   // ���� ������
}

public class MenuPage : BasePage<MenuSetting>
{
    // JSON ���
    protected override string JsonPath => "JSON/MenuSetting.json";

    // ������ �� ���� ������ ĳ��
    private GameObject whatIsPage;
    private GameObject childhoodPage;
    private GameObject youthPage;
    private GameObject primeOfLifePage;
    private GameObject lastPage;
    private GameObject deathPage;
    private GameObject afterDeathPage;

    protected override async Task BuildContentAsync()
    {
        // �� �׺���̼� ��ư ���� �� ���̾
        await WireNavButton(
            Setting.whatIsButton,
            () => whatIsPage,
            go => whatIsPage = go,
            Setting.whatIsPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<WhatIsPage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.childhoodButton,
            () => childhoodPage,
            go => childhoodPage = go,
            Setting.childhoodPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<ChildhoodPage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.youthButton,
            () => youthPage,
            go => youthPage = go,
            Setting.youthPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<YouthPage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.primeOfLifeButton,
            () => primeOfLifePage,
            go => primeOfLifePage = go,
            Setting.primeOfLifePage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<PrimeOfLifePage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.lastButton,
            () => lastPage,
            go => lastPage = go,
            Setting.lastPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<LastPage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.deathButton,
            () => deathPage,
            go => deathPage = go,
            Setting.deathPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<DeathPage>();
                comp.menuPageInstance = this;
            });

        await WireNavButton(
            Setting.afterDeathButton,
            () => afterDeathPage,
            go => afterDeathPage = go,
            Setting.afterDeathPage,
            pageGO =>
            {
                var comp = pageGO.AddComponent<AfterDeathPage>();
                comp.menuPageInstance = this;
            });
    }

    /// <summary>
    /// ���� �׺���̼� ��ư ���� �� �ڵ鷯 ����
    /// 1) ��ư ����
    /// 2) Ŭ�� �� ���̵� �ƿ� �� ���� ��Ȱ��ȭ �� ���� ������ ����/Ȱ��ȭ �� ���̵� ��
    /// </summary>
    private async Task WireNavButton(
        ButtonSetting buttonSetting,
        System.Func<GameObject> getCache,
        System.Action<GameObject> setCache,
        PageSetting targetPageSetting,
        System.Action<GameObject> onCreatedAttach)
    {
        if (buttonSetting == null || targetPageSetting == null) return;

        var createdBtn = await UIManager.Instance.CreateSingleButtonAsync(
            buttonSetting, gameObject, default(CancellationToken));

        var btnGO = createdBtn.button;
        if (btnGO != null && btnGO.TryGetComponent<Button>(out var btn))
        {
            btn.onClick.AddListener(async () =>
            {
                // 1) ���̵� �ƿ� �� �޴� ��Ȱ��ȭ
                await FadeManager.Instance.FadeOutAsync(JsonLoader.Instance.Settings.fadeTime, true);
                gameObject.SetActive(false);

                // 2) ������ ���� or ��Ȱ��ȭ
                var cached = getCache();
                if (cached == null)
                {
                    GameObject parent = UIManager.Instance.mainBackground;
                    var pageGO = await UIManager.Instance.CreatePageAsync(targetPageSetting, parent);
                    if (pageGO != null)
                    {
                        onCreatedAttach?.Invoke(pageGO);
                        setCache(pageGO);
                    }
                }
                else
                {
                    cached.SetActive(true);
                    await FadeManager.Instance.FadeInAsync(JsonLoader.Instance.Settings.fadeTime, true);
                }
            });
        }
    }
}
