using System.Collections;
using HarmonyLib;
using Sonder.Managers;
using UnityEngine;

namespace Loading
{
    public class Loader : MonoBehaviour
    {
        private static bool _loaded;
        private static bool _loading;
        private static Loader _retryHost;
        private static bool _patched;

        private static void EnsurePatched()
        {
            if (_patched) return;
            try
            {
                new Harmony("com.sonder.menu").PatchAll();
            }
            catch (System.Exception ex)
            {
                Debug.Log("[Sonder Loader] PatchAll failed: " + ex);
            }
            _patched = true;
        }

        public static void Init()
        {
            Load();
        }

        public static void StartRetrySystem()
        {
            StartRetry();
        }

        // compat alias (typo used by some inject flows)
        public static void StarRetrySystem()
        {
            StartRetry();
        }

        public static void Load()
        {
            if (_loaded || _loading) return;
            _loading = true;

            try
            {
                EnsurePatched();
                MenuManager.Initialize();
                BoardRuntime.Initialize();
                _loaded = true;
                _loading = false;
                Debug.Log("[Sonder Loader] Initialized");
            }
            catch (System.Exception ex)
            {
                _loading = false;
                _loaded = false;
                Debug.Log("[Sonder Loader] Load failed: " + ex);
                StartRetry();
            }
        }

        public static void StartRetry()
        {
            if (_loaded || _retryHost != null) return;

            GameObject host = new GameObject("SonderLoader");
            Object.DontDestroyOnLoad(host);
            _retryHost = host.AddComponent<Loader>();
            _retryHost.StartCoroutine(_retryHost.RetryRoutine());
        }

        private IEnumerator RetryRoutine()
        {
            int attempts = 0;
            while (!_loaded && attempts < 30)
            {
                attempts++;
                yield return new WaitForSeconds(1f);
                if (_loaded) break;

                _loading = false;
                try
                {
                    EnsurePatched();
                    MenuManager.Initialize();
                    BoardRuntime.Initialize();
                    _loaded = true;
                    _loading = false;
                    Debug.Log("[Sonder Loader] Initialized on retry " + attempts);
                }
                catch (System.Exception ex)
                {
                    _loading = false;
                    _loaded = false;
                    Debug.Log("[Sonder Loader] Retry " + attempts + " failed: " + ex);
                }
            }

            if (_retryHost != null)
            {
                GameObject obj = _retryHost.gameObject;
                _retryHost = null;
                Object.Destroy(obj);
            }
        }
    }
}
