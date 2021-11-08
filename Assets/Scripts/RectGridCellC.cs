using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectGridCellC : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer InnerSprite;

    [SerializeField]
    SpriteRenderer OuterSprite;

    public RectGridCellA gridCellA;

    public void SetInnerColor(Color color)
    {
        InnerSprite.color = color;
    }

    public void SetOuterColor(Color color)
    {
        OuterSprite.color = color;
    }
}
