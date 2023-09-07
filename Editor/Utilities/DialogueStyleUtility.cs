using UnityEditor;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.Utilities
{
    public static class DialogueStyleUtility
    {

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            for (int i = 0; i < styleSheetNames.Length; i++)
            {
                string name = styleSheetNames[i];

                StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(name);
                element.styleSheets.Add(styleSheet);
            }
            return element;
        }


        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            for (int i = 0; i < classNames.Length; i++)
            {
                string name = classNames[i];
                element.AddToClassList(name);
            }
            return element;
        }
    }
}
