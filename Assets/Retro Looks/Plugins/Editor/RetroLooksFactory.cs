using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;
using System.IO;

namespace RetroLooks
{
    public class RetroLooksFactory
    {
        [MenuItem("Assets/Create/RetroLooks Profile", priority = 201)]
        static void MenuCreateRetroLooksProfile()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateRetroLooksProfile>(), "New RetroLooks Profile.asset", icon, null);
        }

        internal static RetroLooksProfile CreateRetroLooksProfileAtPath(string path)
        {
            var profile = ScriptableObject.CreateInstance<RetroLooksProfile>();
            profile.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }
    }

    class DoCreateRetroLooksProfile : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            RetroLooksProfile profile = RetroLooksFactory.CreateRetroLooksProfileAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(profile);
        }
    }
}
