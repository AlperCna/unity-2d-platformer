using UnityEditor;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Editor araclari icin ortak on kontroller.
    ///
    /// NEDEN VAR: bu proje ayni hatayi uc kez yasadi. Play modunda
    /// calistirilan araclar ya sessizce hicbir sey yapmadi, ya
    /// InvalidOperationException atti, ya da daha kotusu — YANILTICI
    /// SONUC uretti:
    ///
    ///   - CameraBoundsTool: MarkSceneDirty exception atti
    ///   - LevelBuilder: SaveCurrentModifiedScenesIfUserWantsTo exception atti
    ///   - SaveTools "Kaydi Boz": dosyayi bozdu ama calisan SaveManager
    ///     Play'den cikarken uzerine gecerli veri yazdi. Test "gecti"
    ///     gorundu, oysa hicbir sey test edilmemisti.
    ///
    /// Sonuncusu en tehlikelisi: hata vermeyen ama yanlis sonuc veren test.
    /// </summary>
    internal static class EditorGuards
    {
        /// <summary>
        /// Arac Play modunda calistirilamiyorsa false doner ve kullaniciya
        /// NEDEN oldugunu soyler.
        /// </summary>
        internal static bool RequireEditMode(string toolName, string reason = null)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;

            string message = $"\"{toolName}\" Play modunda calismaz.\n\n";
            message += reason ?? "Play modunda yapilan degisiklikler kalici degildir.";
            message += "\n\nOnce Play'i durdur, sonra tekrar dene.";

            EditorUtility.DisplayDialog("Once Play'i durdur", message, "Tamam");
            return false;
        }
    }
}
