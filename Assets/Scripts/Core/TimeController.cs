using System.Collections;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Kisa sureli zaman donmasi (hit stop).
    ///
    /// Carpisma aninda oyunu 3-6 kare dondurmak darbeyi "hissettirir".
    /// Az is, cok etki - ama uc tuzagi var, ucu de burada kapatildi:
    ///
    /// 1) timeScale = 0 iken WaitForSeconds SONSUZA KADAR bekler.
    ///    WaitForSecondsRealtime kullanilmali.
    ///
    /// 2) Coroutine yarida kesilirse (sahne degisimi, nesne yok olmasi)
    ///    oyun donmus kalir. Bu yuzden DontDestroyOnLoad + OnDisable guvenligi.
    ///
    /// 3) Efekti baslatan nesne yok olabilir (ezilen dusman kendi hit stop'unu
    ///    baslatip sonra Destroy olursa coroutine olur). O yuzden hit stop
    ///    KALICI bir yoneticide calisir, cagiran nesnede degil.
    /// </summary>
    public class TimeController : MonoBehaviour
    {
        public static TimeController Instance { get; private set; }

        /// <summary>Su an hit stop icinde miyiz?</summary>
        public bool IsFrozen { get; private set; }

        private Coroutine current;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Oyunu kisa sure tamamen dondurur.
        /// Onerilen sureler: dusman ezme 0.05, olum 0.08, sir bulma 0.06.
        /// Normal para icin KULLANMA - 25 kez donan oyun sinir bozucudur.
        /// </summary>
        public void HitStop(float duration)
        {
            if (duration <= 0f) return;

            // Devam eden bir donma varsa uzat, ust uste bindirme
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            IsFrozen = true;
            Time.timeScale = 0f;

            // ZORUNLU: WaitForSeconds timeScale 0'da sonsuza kadar bekler
            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            IsFrozen = false;
            current = null;
        }

        /// <summary>Sahne degisimi gibi durumlarda guvenlik agi.</summary>
        public void ResetTimeScale()
        {
            if (current != null) StopCoroutine(current);
            current = null;
            IsFrozen = false;
            Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            // Nesne yok olursa oyun donmus kalmasin
            if (IsFrozen) Time.timeScale = 1f;
            IsFrozen = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
