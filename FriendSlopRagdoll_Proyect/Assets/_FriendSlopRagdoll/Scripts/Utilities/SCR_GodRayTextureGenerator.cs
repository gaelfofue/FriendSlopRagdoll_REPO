using UnityEngine;
using UnityEditor;
using System.IO;

public class GodRayTextureGenerator
{
    private const string ArtPath =
        "Assets/_FriendSlopRagdoll/Art/Textures";

    [MenuItem("Tools/God Rays/Generate Texture")]
    static void GenerateTexture()
    {
        int width = 256;
        int height = 512;

        Texture2D texture = new Texture2D(
            width, height, TextureFormat.RGBA32, false
        );

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float horizontal = (float)x / (width - 1);
                float vertical = (float)y / (height - 1);

                // Desvanecimiento suave en los extremos laterales.
                float sideFade =
                    Mathf.SmoothStep(0f, 0.18f, horizontal) *
                    Mathf.SmoothStep(0f, 0.18f, 1f - horizontal);

                // La luz nace intensa y se desvanece progresivamente.
                float beamFade = Mathf.Pow(1f - vertical, 1.35f);

                float alpha = sideFade * beamFade;

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        texture.Apply();

        Directory.CreateDirectory(ArtPath + "Textures");

        byte[] bytes = texture.EncodeToPNG();

        File.WriteAllBytes(
            ArtPath + "Textures/TEX_GodRay.png",
            bytes
        );

        AssetDatabase.Refresh();

        Debug.Log("¡Textura God Ray generada!");
    }

    [MenuItem("Tools/God Rays/Generate Cone Mesh")]
    static void GenerateConeMesh()
    {
        int segments = 48;

        float length = 8f;
        float startRadius = 0.15f;
        float endRadius = 1.5f;

        Mesh mesh = new Mesh();
        mesh.name = "GodRayConeMesh";

        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];
        Vector2[] uvs = new Vector2[segments * 2];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            float u = (float)i / segments;

            uvs[i] = new Vector2(u, 0f);
            uvs[i + segments] = new Vector2(u, 1f);

            vertices[i] = new Vector3(
                x * startRadius,
                0f,
                z * startRadius
            );

            vertices[i + segments] = new Vector3(
                x * endRadius,
                -length,
                z * endRadius
            );
        }

        int triangleIndex = 0;

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            int bottomA = i;
            int bottomB = next;
            int topA = i + segments;
            int topB = next + segments;

            triangles[triangleIndex++] = bottomA;
            triangles[triangleIndex++] = bottomB;
            triangles[triangleIndex++] = topA;

            triangles[triangleIndex++] = bottomB;
            triangles[triangleIndex++] = topB;
            triangles[triangleIndex++] = topA;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        string folder = ArtPath + "Meshes/";

        Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();

        string path = AssetDatabase.GenerateUniqueAssetPath(
            folder + "GodRayConeMesh.asset"
        );

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("¡Malla cónica generada en: " + path);
    }
}
