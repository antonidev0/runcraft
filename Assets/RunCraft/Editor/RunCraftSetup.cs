using System.IO;
using UnityEditor;
using UnityEngine;

namespace RunCraft.EditorTools
{
    /// <summary>
    /// Se ejecuta solo al abrir el proyecto en Unity:
    ///  1. Crea el material base (URP Lit) en Resources para que el juego funcione también en el móvil.
    ///  2. Deja el juego en vertical y con nombre/identificador propios para Android.
    /// También aparece en el menú superior: RunCraft > Preparar proyecto.
    /// </summary>
    [InitializeOnLoad]
    public static class RunCraftSetup
    {
        const string MatPath = "Assets/RunCraft/Resources/RunCraftBaseMat.mat";

        static RunCraftSetup()
        {
            EditorApplication.delayCall += () => Run(false);
        }

        [MenuItem("RunCraft/Preparar proyecto")]
        static void RunFromMenu() => Run(true);

        static void Run(bool verbose)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // 1) Material base
            if (AssetDatabase.LoadAssetAtPath<Material>(MatPath) == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit");
                if (s == null)
                {
                    Debug.LogWarning("RunCraft: no encuentro el shader URP Lit. ¿Está instalado Universal RP?");
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(MatPath));
                    var m = new Material(s) { name = "RunCraftBaseMat", enableInstancing = true };
                    if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
                    AssetDatabase.CreateAsset(m, MatPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("RunCraft: material base creado en " + MatPath);
                }
            }

            // 2) Ajustes de Android (solo la primera vez)
            if (!EditorPrefs.GetBool("RunCraft_SettingsDone_" + Application.dataPath, false) || verbose)
            {
                PlayerSettings.productName = "RunCraft";
                PlayerSettings.companyName = "Antoni";
                PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.antoni.runcraft");
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                PlayerSettings.allowedAutorotateToLandscapeLeft = false;
                PlayerSettings.allowedAutorotateToLandscapeRight = false;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                EditorPrefs.SetBool("RunCraft_SettingsDone_" + Application.dataPath, true);
                AssetDatabase.SaveAssets();
                Debug.Log("RunCraft: ajustes de Android listos (vertical, com.antoni.runcraft).");
            }

            if (verbose) Debug.Log("RunCraft: proyecto preparado. Abre Assets/Scenes/SampleScene y pulsa Play.");
        }
    }
}
