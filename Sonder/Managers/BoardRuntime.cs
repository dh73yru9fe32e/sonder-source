using TMPro;
using UnityEngine;

namespace Sonder.Managers
{
    public class BoardRuntime : MonoBehaviour
    {
        private static BoardRuntime _instance;
        private bool _textApplied;
        private float _nextTry;
        private int _tries;

        public static void Initialize()
        {
            if ((Object)(object)_instance != (Object)null)
                return;
            GameObject go = new GameObject("Sonder_BoardRuntime");
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<BoardRuntime>();
        }

        private void Update()
        {
            if (_textApplied)
                return;
            if (_tries > 600)
                return;
            if (Time.time < _nextTry)
                return;
            _nextTry = Time.time + 1f;
            _tries++;
            TryApplyText();
        }

        private void TryApplyText()
        {
            if (_textApplied) return;
            try
            {
                GameObject tree = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom");
                if ((Object)(object)tree == (Object)null)
                    return;

                TextMeshPro motdHeading = FindTMP(tree, "motdHeadingText");
                TextMeshPro motdBody = FindTMP(tree, "motdBodyText");
                TextMeshPro welcomeHeading = FindTMP(tree, "WelcomeToGorilllaTagHeadingText");
                TextMeshPro headOutside = FindTMP(tree, "HeadOutside");
                Transform cocHeading = tree.transform.Find("CodeOfConductHeadingText");
                Transform cocBody = tree.transform.Find("COCBodyText_TitleData");

                if ((Object)(object)motdHeading == (Object)null || (Object)(object)motdBody == (Object)null)
                    return;

                if ((Object)(object)cocHeading != (Object)null)
                    Object.Destroy(cocHeading.gameObject);
                if ((Object)(object)cocBody != (Object)null)
                    Object.Destroy(cocBody.gameObject);

                string letterMeanings =
                    "<color=white>M</color> <size=60>(MASTER - FIRST ON LEADERBOARD)</size>\n" +
                    "<color=white>Priv M</color> <size=60>(PRIVATE ROOM + MASTER)</size>\n" +
                    "<color=white>GR</color> <size=60>(GHOST REACTOR)</size>\n" +
                    "<color=white>A / B / X</color> <size=60>(CONTROLLER BUTTON HOLD)</size>\n" +
                    "<color=white>RJ / LJ</color> <size=60>(GRIP / JOYSTICK CLICK)</size>\n" +
                    "<color=white>SS / CS</color> <size=60>(SERVER / CLIENT SIDED)</size>";

                if ((Object)(object)motdHeading != (Object)null)
                    motdHeading.text = "<color=white>SONDER 1.0.3</color>";

                if ((Object)(object)welcomeHeading != (Object)null)
                    welcomeHeading.text = "<color=white>LETTER MEANINGS</color>";

                if ((Object)(object)headOutside != (Object)null)
                {
                    headOutside.richText = true;
                    headOutside.text =
                        letterMeanings +
                        "\n----------------------------------------------\n" +
                        "<color=white><size=70>SONDER MENU ACTIVE</size></color>\n" +
                        "<color=grey><size=58>BUILT BY OLIVIA - https://discord.gg/ojailbreaks</size></color>";
                }

                string motd =
                    "<color=white>THANKS FOR USING SONDER</color>\n" +
                    "<color=yellow>BUILT BY OLIVIA</color>  <color=white>|</color>  <color=yellow>https://discord.gg/ojailbreaks</color>\n\n" +
                    "<color=white>CHECK TOOLTIP TEXT ON MODS BEFORE USE</color>\n" +
                    "<color=white>[M] / [Priv M] / [GR] TAGS MATTER FOR REQUIREMENTS</color>\n" +
                    "<color=grey>SONDER 1.0.3 - CUSTOM BUILD</color>\n\n" +
                    "<color=#00FFFF>OP / GR FEATURES ARE HIGH IMPACT</color>\n" +
                    "<color=red>USE AT YOUR OWN RISK</color>";

                string account = "\n\n<color=white>MENU: </color><color=green>SONDER LOADED</color>";
                motdBody.richText = true;
                motdBody.text = motd + account;

                _textApplied = true;
            }
            catch
            {
                // retry
            }
        }

        private static TextMeshPro FindTMP(GameObject root, string childName)
        {
            Transform t = root.transform.Find(childName);
            if ((Object)(object)t == (Object)null)
                return null;
            return t.GetComponent<TextMeshPro>();
        }
    }
}
