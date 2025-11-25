// Original code found at- https://github.com/robertcupisz/fire. Edits have occured

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Fire : MonoBehaviour
{
	public static float m_Brightness = 8.0f;
	public static float m_Speed = 1.0f;
	static MaterialPropertyBlock m_MatProps;
	public static float m_StartTime = 0.0f;
	public static float m_TimeElapsed;
	public static float m_LastFrameTime;

    public static Camera cam;
    public static Vector3 camPosition;
    public static Matrix4x4 camlocalToWorldMatrix;
    Renderer ren;

    [SerializeField]
    NoteController nCon = null;

    Transform t;

	public void Start()
	{
        ren = GetComponent<Renderer>();

        if (m_MatProps == null)
            m_MatProps = new MaterialPropertyBlock();

        m_MatProps.Clear();
        m_MatProps.SetVector("_Scale", transform.localScale);
        m_MatProps.SetFloat("_Brightness", m_Brightness);
        m_MatProps.SetVector("_CameraLocalPos", new Vector3(0.0f, 3.9f, -19.7f));

        t = transform;
    }

	public void OnWillRenderObject()
	{
        if (Globals.viewMode == Globals.ViewMode.Song)
        {
            m_MatProps.SetVector("_CameraLocalPos", t.InverseTransformPoint(camPosition));
            m_MatProps.SetMatrix("_CameraToLocal", t.worldToLocalMatrix * camlocalToWorldMatrix);

            ren.SetPropertyBlock(m_MatProps);

            if (Application.isPlaying && nCon.note != null)
            {
                int noteNumber = (int)nCon.note.rawNote;
                ren.sharedMaterial = FireSyncronizer.flameMaterials[noteNumber]; //MaterialByPosition();
            }
        }
	}

    Material MaterialByPosition()
    {
        float xPos = transform.position.x;

        if (xPos < 0)   // Green or red
        {
            if (xPos < -1)
                return FireSyncronizer.flameMaterials[0];
            else
                return FireSyncronizer.flameMaterials[1];
        }
        // Yellow, blue or orange
        else
        {
            if (xPos >= 1)
            {
                if (xPos > 1)
                    return FireSyncronizer.flameMaterials[4];
                else
                    return FireSyncronizer.flameMaterials[3];
            }
            else
                return FireSyncronizer.flameMaterials[2];
        }
    }
}
