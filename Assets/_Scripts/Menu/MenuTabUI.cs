using UnityEngine;
using UnityEngine.UI;

public class MenuTabUI : MonoBehaviour
{
    public GameObject panel1;
    public GameObject panel2;

    public GameObject button1Object;
    public GameObject button2Object;

    public void SelectTab1()
    {
        panel1.SetActive(true);
        panel2.SetActive(false);
    }

    public void SelectTab2()
    {
        panel1.SetActive(false);
        panel2.SetActive(true);
    }
}