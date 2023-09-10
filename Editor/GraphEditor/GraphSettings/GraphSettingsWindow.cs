using FrameworksXD.DialogueXD.Editor.Utilities;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor.GraphSettings
{
    public class GraphSettingsWindow : EditorWindow
    {
        private const string WindowName = "Dialogue Graph Settings";

        private DialogueGraphSettingsData SettingsData;

        private bool SpeakersFoldout;

        public static GraphSettingsWindow Open(DialogueGraphSettingsData settingsData)
        {
            var window = GetWindow<GraphSettingsWindow>(WindowName);
            window.SettingsData = settingsData;
            return window;
        }

        private void OnGUI()
        {
            if (SettingsData == null)
                return;

            DrawSpeakers();
        }

        private void DrawSpeakers()
        {
            SpeakersFoldout = EditorGUILayout.Foldout(SpeakersFoldout, "Speakers");
            if (SpeakersFoldout)
            {
                var speakers = SettingsData.DialogueSpeakerData;
                for (int i = 1; i < speakers.Count; i++)
                {
                    var speaker = speakers[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUI.enabled = false;
                    speaker.Id = EditorGUILayout.TextField("Id", speaker.Id);
                    GUI.enabled = true;
                    speaker.Name = EditorGUILayout.TextField("Name", speaker.Name);
                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("X"))
                    {
                        speakers.RemoveAt(i);
                        --i;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("New"))
                {
                    speakers.Add(new Data.DialogueSpeakerData());
                }
            }
        }
    }
}
