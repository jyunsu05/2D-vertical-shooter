using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    private SpriteRenderer sr;

    public Sprite[] Boom;
    public Sprite[] Coin;
    public Sprite[] Power;

    public float animationSpeed = 0.1f;
    private Coroutine currentAnim;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        PlayAnimation("coin");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string[] types = {"boom",  "coin", "power"};
            PlayAnimation(types[Random.Range(0, 3)]);
        }
    }

    public void PlayAnimation(string type)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);

        Sprite[] targetSprites = null;
        if(type == "boom") targetSprites = Boom;
        else if(type == "coin") targetSprites = Coin;
        else if(type == "power") targetSprites = Power;

        if (targetSprites != null || targetSprites.Length > 0)
        {
            currentAnim = StartCoroutine(AnimateSprite(targetSprites));
        }
    }

    IEnumerator AnimateSprite(Sprite[] sprites)
    {
        int index = 0;
        while (true)
        {
            sr.sprite = sprites[index];
            index = (index + 1) % sprites.Length; 
            yield return new WaitForSeconds(animationSpeed);
        }
    }
}
