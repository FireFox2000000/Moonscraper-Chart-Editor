///////////////////////////////////////////////
// MK Glow	    							 //
//											 //
// Created by Michael Kremmel                //
// www.michaelkremmel.de                     //
// Copyright © 2015 All rights reserved.     //
///////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using MKGlowSystem;

namespace MKGlowSystemEditor
{
    [CustomEditor(typeof(MKGlow))]
    public class MKGlowEditor : Editor
    {
        private const string m_Style = "box";
        //private static Texture2D m_ComponentLabel;

        private SerializedProperty glowType;
        private SerializedProperty samples;
        private SerializedProperty blurSpread;
        private SerializedProperty blurIterations;
        private SerializedProperty glowIntensity;
        private SerializedProperty fullScreenGlowTint;
        private SerializedProperty showTransparent;
        private SerializedProperty showCutout;
        private SerializedProperty glowLayer;

		[MenuItem("Window/MKGlowSystem/Add MK Glow System To Selection")]
		private static void AddMKGlowToObject()
		{
			foreach (GameObject obj in Selection.gameObjects)
			{
				if (obj.GetComponent<MKGlow>() == null)
					obj.AddComponent<MKGlow>();
			}
		}

        private void OnEnable()
        {
            samples = serializedObject.FindProperty("samples");
            blurSpread = serializedObject.FindProperty("blurSpread");
            blurIterations = serializedObject.FindProperty("blurIterations");
            glowIntensity = serializedObject.FindProperty("glowIntensity");
            fullScreenGlowTint = serializedObject.FindProperty("fullScreenGlowTint");
            showTransparent = serializedObject.FindProperty("showTransparentGlow");
            showCutout = serializedObject.FindProperty("showCutoutGlow");
            glowType = serializedObject.FindProperty("glowType");
            glowLayer = serializedObject.FindProperty("glowLayer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(glowLayer);

            EditorGUILayout.PropertyField(glowType);

            EditorGUILayout.Slider(blurSpread, 0.05f, 0.6f, "Blur Spread");
            EditorGUILayout.IntSlider(blurIterations, 0, 10, "Blur Iterations");
            EditorGUILayout.IntSlider(samples, 2, 8, "Blur Samples");
            EditorGUILayout.Slider(glowIntensity, 0f, 1f, "Glow Intensity");
            fullScreenGlowTint.colorValue = EditorGUILayout.ColorField("Glow Tint", fullScreenGlowTint.colorValue);
            EditorGUILayout.PropertyField(showTransparent);
            EditorGUILayout.PropertyField(showCutout);

            serializedObject.ApplyModifiedProperties();

            //DrawDefaultInspector ();

        }
    }
}
#endif