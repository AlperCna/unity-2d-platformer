using System.IO;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Kayit dosyasiyla ilgili gelistirme araclari.
    ///
    /// Kayit sistemini test ederken surekli elle dosya silmek zorunda
    /// kalmamak icin. Menu: Tools > 2D Platformer > Kayit
    /// </summary>
    public static class SaveTools
    {
        private static string FilePath =>
            Path.Combine(Application.persistentDataPath, "save.json");

        [MenuItem("Tools/2D Platformer/Kayit/Kaydi Goster", false, 50)]
        public static void ShowSave()
        {
            if (!File.Exists(FilePath))
            {
                Debug.Log($"Kayit dosyasi yok.\n{FilePath}");
                return;
            }

            Debug.Log($"{FilePath}\n\n{File.ReadAllText(FilePath)}");
        }

        [MenuItem("Tools/2D Platformer/Kayit/Klasoru Ac", false, 51)]
        public static void OpenFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("Tools/2D Platformer/Kayit/Kaydi Sil", false, 52)]
        public static void DeleteSave()
        {
            if (!EditorGuards.RequireEditMode("Kaydi Sil",
                "Calisan SaveManager, Play'den cikarken dosyayi yeniden yazar.")) return;

            if (!File.Exists(FilePath))
            {
                Debug.Log("Silinecek kayit yok.");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "Kaydi Sil",
                $"Kayit dosyasi silinecek:\n{FilePath}\n\n" +
                "Tum bolum ilerlemesi ve ayarlar gidecek. Geri alinamaz.",
                "Sil", "Vazgec");

            if (!ok) return;

            File.Delete(FilePath);
            Debug.Log("Kayit silindi.");
        }

        /// <summary>
        /// Kayit dosyasini kasten bozar - hata dayanikliligini test etmek icin.
        /// Oyun cokmemeli, sifirdan baslamali ve bozuk dosyayi yedeklemeli.
        /// </summary>
        [MenuItem("Tools/2D Platformer/Kayit/Kaydi Boz (test)", false, 53)]
        public static void CorruptSave()
        {
            // EN ONEMLI KORUMA BU. Play modunda bozarsan calisan SaveManager
            // bellegindeki GECERLI veriyi cikis aninda diske yazar ve
            // bozuklugu siler. Test "gecti" gorunur ama hicbir sey test
            // edilmemis olur. Bu projede tam olarak bu yasandi.
            if (!EditorGuards.RequireEditMode("Kaydi Boz (test)",
                "Calisan SaveManager, Play'den cikarken bellegindeki gecerli veriyi\n" +
                "diske yazar ve bozuklugu siler. Test yaniltici sonuc verir.")) return;

            if (!File.Exists(FilePath))
            {
                Debug.Log("Bozulacak kayit yok. Once oyunu oynayip bir bolum bitir.");
                return;
            }

            File.WriteAllText(FilePath, "{bu gecerli bir JSON degil!!! ###");
            Debug.Log("Kayit kasten bozuldu. Simdi Play'e bas:\n" +
                      "  Beklenen: oyun ACILIR, sifirdan baslar, konsola uyari yazar,\n" +
                      "  bozuk dosya save.json.corrupt olarak yedeklenir.\n" +
                      "  COKMEMELI.");
        }
    }
}
