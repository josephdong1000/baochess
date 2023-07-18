using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CircleHighlightScript : MonoBehaviour {
    public (int, int) Position { get; set; }

    public Color circleColor;
    public Sprite circleSprite;

    public Color shieldColor;
    public Sprite shieldSprite;

    public Color selectColor;
    public Sprite selectSprite;

    public Color attackColor;
    public Sprite attackSprite;

    public Color rangedColor;
    public Sprite rangedSprite;

    public float fadeSpeed;
    public float fadeDeltaTime;


    private BoardScript _boardScript;
    private List<(int, int)> _multiplePairPositions;

    SpriteRenderer _spriteRenderer;

    // private Color _defaultColor;
    public SpriteType thisSpriteType { get; set; }


    public enum SpriteType {
        None,
        Cirle,
        Shield,
        Select,
        Attack,
        Ranged,
    }

    private void Awake() {
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        _multiplePairPositions = _boardScript.MultiplePairPositions;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Start() {
        
        // StartCoroutine(SetSpriteType(thisSpriteType));

        SetSpriteType(thisSpriteType);
    }


    public void SetSpriteType(SpriteType spriteType) {
        // yield return new WaitUntil(() => thisSpriteType != SpriteType.None);

        if (spriteType == SpriteType.Cirle) {
            _spriteRenderer.sprite = circleSprite;
            _spriteRenderer.color = circleColor;
            transform.rotation = RandomCardinalDirection();
        } else if (spriteType == SpriteType.Shield) {
            _spriteRenderer.sprite = shieldSprite;
            _spriteRenderer.color = shieldColor;
        } else if (spriteType == SpriteType.Select) {
            _spriteRenderer.sprite = selectSprite;
            _spriteRenderer.color = selectColor;
            transform.rotation = RandomCardinalDirection();
        } else if (spriteType == SpriteType.Attack) {
            _spriteRenderer.sprite = attackSprite;
            _spriteRenderer.color = attackColor;
            transform.rotation = RandomCardinalDirection();
        } else if (spriteType == SpriteType.Ranged) {
            _spriteRenderer.sprite = rangedSprite;
            _spriteRenderer.color = rangedColor;
            transform.rotation = RandomFlippedDirection();
        } else {
            throw new NotImplementedException("None sprite type not implemented");
        }
    }

    private Quaternion RandomCardinalDirection() {
        return quaternion.Euler(0,
                                0,
                                90 * Random.Range(0, 4) * Mathf.Deg2Rad);
    }
    
    private Quaternion RandomFlippedDirection() {
        return quaternion.Euler(0,
                                0,
                                180 * Random.Range(0, 2) * Mathf.Deg2Rad);
    }


    // public void StartFadeOutSequence() {
    //     StartCoroutine(FadeOutSequence());
    // }

    public IEnumerator FadeOutSequence() {
        Color defaultColor = _spriteRenderer.color;
        float alpha = 1;

        while (alpha > 0 && _spriteRenderer != null) {
            alpha -= fadeSpeed * fadeDeltaTime;
            _spriteRenderer.color = new Color(defaultColor.r,
                                              defaultColor.g,
                                              defaultColor.b,
                                              alpha);
            yield return new WaitForSeconds(fadeDeltaTime);
        }

        // if (gameObject != null) {
        //     Destroy(gameObject);
        // }
    }

    public IEnumerator Destroy() {
        Destroy(gameObject);
        yield break;
    }
}