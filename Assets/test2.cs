using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class test2 : MonoBehaviour
{
    public Button btn;
    public SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            StartCoroutine(FadOut());
        });
    }

    IEnumerator FadOut()
    {
        for (int i =0; i<=225; i++)
        {
            var newAlpha = 1 - (i / 225f);
            sr.color = new Color(1, 1, 1, newAlpha);
            i++;
            yield return null;
        }
    }
}
