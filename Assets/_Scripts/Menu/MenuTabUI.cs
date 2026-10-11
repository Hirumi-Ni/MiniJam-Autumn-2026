using UnityEngine;

public class MenuTabUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;

    [Header("Tabs")]
    [SerializeField] private TabButton tab1;
    [SerializeField] private TabButton tab2;

    private void Start()
    {
        Select(1, playSound: false, instant: true);
    }

    public void SelectTab1() => Select(1, playSound: true);
    public void SelectTab2() => Select(2, playSound: true);

    private void Select(int index, bool playSound, bool instant = false)
    {
        bool isFirst = index == 1;

        panel1.SetActive(isFirst);
        panel2.SetActive(!isFirst);

        tab1.SetSelected(isFirst, instant);
        tab2.SetSelected(!isFirst, instant);

        if (playSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_UIPopOpen);
        }
    }
}
