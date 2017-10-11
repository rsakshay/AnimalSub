using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour {

    public float MAX_HP = 50;
    public Color endColor;

    SpriteRenderer sprite;
    Color startColor;
    float hp;


    // Use this for initialization
    void Start () {
        sprite = GetComponent<SpriteRenderer>();
        startColor = sprite.color;
        hp = MAX_HP;
	}
	
	// Update is called once per frame
	void Update () {
        Color.Lerp(endColor, startColor, hp / MAX_HP);
	}

    public float TrasferHPToAnimal()
    {
        float hpToTransfer = Random.Range(5f, 10f);

        hp -= hpToTransfer;

        if (hp < 0)
        {
            hpToTransfer -= Mathf.Abs(hp);
            hp = 0;
            Destroy(gameObject);
        }

        return hpToTransfer;
    }
}
