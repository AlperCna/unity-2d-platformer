namespace Platformer.Core
{
    /// <summary>
    /// Respawn'da baslangic durumuna donmesi gereken nesneler.
    ///
    /// NEDEN GEREKLI: oyuncu olup checkpoint'te dogdugunda sadece karakter
    /// degil, bolum de sifirlanmali. Hareketli platform yanlis yerdeyken
    /// dogarsan gecemezsin ve sonsuz olum dongusune girersin.
    ///
    /// KIM UYGULAMAZ, ONEMLI:
    ///   Coin        - toplanmis kalsin, ayni parayi 20 kez toplamak iskence
    ///   Checkpoint  - aktif kalsin, yoksa geriye dogru kaydedersin
    ///   SecretArea  - bulunmus kalsin
    ///
    /// GameManager.ResetLevelState() sahnedeki tumunu bulup cagirir.
    /// </summary>
    public interface IResettable
    {
        void ResetToInitialState();
    }
}
