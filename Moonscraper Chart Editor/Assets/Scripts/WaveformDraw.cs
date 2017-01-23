using UnityEngine;
using System.Collections;

public class WaveformDraw : MonoBehaviour {
    public AudioClip testAudio;

    float[] data;

    void Start()
    {
        data = new float[testAudio.samples * testAudio.channels];
        testAudio.GetData(data, 0);
    }

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public void OnRenderObject()
    {
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        GL.Color(Color.green);

        float prevPos = -5;
        for (int i = 1000; i < data.Length; i += 1000)
        {
            // One vertex at transform position
            GL.Vertex3(data[i - 1000] * 500, prevPos, 0);
            prevPos += i / 5000;
            GL.Vertex3(data[i] * 500, prevPos, 0); 
        }
        GL.End();
        GL.PopMatrix();
    }
}
