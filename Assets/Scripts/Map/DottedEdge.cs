using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DottedEdge : MonoBehaviour
{
    public Transform a;
    public Transform b;

    [Header("Look")]
    public float width = 0.06f;
    [Tooltip("Dots per world-unit (higher = more dots).")]
    public float dotsPerUnit = 2.5f;
    public Material dottedMaterial; // optional; if null we create one

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;
        lr.textureMode = LineTextureMode.Tile;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.widthMultiplier = width;

        if (!dottedMaterial)
            dottedMaterial = BuildRuntimeDottedMaterial();

        lr.material = dottedMaterial;
    }

    void LateUpdate()
    {
        if (!a || !b) return;

        lr.positionCount = 2;
        lr.SetPosition(0, a.position);
        lr.SetPosition(1, b.position);

        // Tile the texture based on distance
        float dist = Vector2.Distance(a.position, b.position);
        var mainTex = lr.material.mainTexture;
        float tile = Mathf.Max(1f, dist * dotsPerUnit);
        lr.material.mainTextureScale = new Vector2(tile, 1f);
    }

    // Make a small dotted texture if none is provided
    Material BuildRuntimeDottedMaterial()
    {
        var tex = new Texture2D(8, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        // pattern: [dot, gap, gap, gap, dot, gap, gap, gap]
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 2; y++)
        {
            bool isDot = (x == 0) || (x == 4);
            Color c = isDot ? Color.white : new Color(1,1,1,0);
            tex.SetPixel(x, y, c);
        }
        tex.Apply();

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = tex;
        mat.SetFloat("_EnableExternalAlpha", 1);
        return mat;
    }
}
