using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source: https://unitycodemonkey.com/video.php?v=waEsGu--9P8
public class UtilsClass
{
    /// <summary>
    /// Creates a text object with the given text and position.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="localPosition">The local position.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="color">The color.</param>
    /// <param name="textAnchor">The text anchor.</param>
    /// <param name="textAlignment">The text alignment.</param>
    /// <param name="sortingOrder">The sorting order.</param>
    /// <returns>A TextMesh.</returns>
    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color color = default(Color), TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000)
    {
        if (color == default(Color))
        {
            color = Color.white;
        }

        return CreateWorldText(parent, text, localPosition, fontSize, color, textAnchor, textAlignment, sortingOrder);
    }

    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
        renderer.sortingOrder = sortingOrder;
        renderer.sortingLayerName = "UI";
        return textMesh;
    }

    /// <summary>
    /// Gets the position of the mouse in world space.
    /// </summary>
    /// <returns>the position as A Vector3.</returns>
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosScreen = new Vector3();
        mousePosScreen.x = Input.mousePosition.x;
        mousePosScreen.y = Input.mousePosition.y;
        mousePosScreen.z = 10f;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(mousePosScreen);
        Debug.Log("Mouse position: " + mousePosition);
        return mousePosition;
    }
}
