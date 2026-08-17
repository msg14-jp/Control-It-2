using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ControlIt
{
    public static class Translations
    {
        public static event Action eventLanguageChanged;

        public static readonly string[] LanguageList = new string[]
        {
            "Auto",
            "English",
            "日本語",
            "Deutsch",
            "Español",
            "Français",
            "한국어",
            "Polski",
            "Português (Brasil)",
            "Русский",
            "简体中文"
        };

        private static readonly string[] LanguageCodes = new string[]
        {
            "auto",
            "en-US",
            "ja-JP",
            "de-DE",
            "es-ES",
            "fr-FR",
            "ko-KR",
            "pl-PL",
            "pt-BR",
            "ru-RU",
            "zh-CN"
        };

        private static int _currentIndex = 0;
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static bool _isHooked = false;

        public static int Index
        {
            get => _currentIndex;
            set
            {
                if (value >= 0 && value < LanguageList.Length)
                {
                    _currentIndex = value;
                    Debug.Log($"[ControlIt] Index変更: {value} ({LanguageList[value]})");
                    LoadLanguage(GetTargetLanguageCode(_currentIndex));
                }
            }
        }

        public static void Init()
        {
            try
            {
                if (LocaleManager.exists)
                {
                    LocaleManager.eventLocaleChanged -= OnGameLocaleChanged;
                    LocaleManager.eventLocaleChanged += OnGameLocaleChanged;
                    Debug.Log("[ControlIt] LocaleManager.eventLocaleChanged の登録成功");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlIt] LocaleChangedイベントの登録に失敗: {ex.Message}");
            }

            HookLanguageDropdown();
            LoadLanguage(GetTargetLanguageCode(_currentIndex));
        }

        public static void HookLanguageDropdown()
        {
            try
            {
                UIDropDown langDropdown = UIView.Find<UIDropDown>("Language");
                if (langDropdown != null)
                {
                    langDropdown.eventSelectedIndexChanged -= OnLanguageDropdownChanged;
                    langDropdown.eventSelectedIndexChanged += OnLanguageDropdownChanged;
                    _isHooked = true;
                    Debug.Log("[ControlIt] ゲームの言語ドロップダウンの監視・フックに成功しました！");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlIt] ドロップダウンフック失敗: {ex.Message}");
            }
        }

        private static void OnLanguageDropdownChanged(UIComponent component, int value)
        {
            Debug.Log($"[ControlIt] 言語ドロップダウンの変更を直接検知しました！ (Index: {value})");
            OnGameLocaleChanged();
        }

        private static void OnGameLocaleChanged()
        {
            if (_currentIndex == 0)
            {
                Debug.Log("[ControlIt] ゲーム本体の言語変更を検知したため、Auto設定を更新します");
                LoadLanguage(GetTargetLanguageCode(0));
                eventLanguageChanged?.Invoke();
            }
        }

        public static string Translate(string key)
        {
            if (_translations != null && _translations.TryGetValue(key, out string translated))
            {
                return translated;
            }
            return key;
        }

        private static string GetTargetLanguageCode(int index)
        {
            if (index == 0)
            {
                return GetGameLanguageCode();
            }

            if (index > 0 && index < LanguageCodes.Length)
            {
                return LanguageCodes[index];
            }

            return "en-US";
        }

        private static string GetGameLanguageCode()
        {
            try
            {
                if (LocaleManager.exists && LocaleManager.instance != null)
                {
                    // ゲーム本体の言語コードを取得（"en", "ja", "de" などが入る）
                    string gameLang = LocaleManager.instance.language;

                    Debug.Log($"[ControlIt] 取得したゲーム言語識別子: '{gameLang}'");

                    if (!string.IsNullOrEmpty(gameLang))
                    {
                        gameLang = gameLang.ToLower();
                        if (gameLang.StartsWith("ja") || gameLang.Contains("japanese")) return "ja-JP";
                        if (gameLang.StartsWith("de") || gameLang.Contains("german")) return "de-DE";
                        if (gameLang.StartsWith("es") || gameLang.Contains("spanish")) return "es-ES";
                        if (gameLang.StartsWith("fr") || gameLang.Contains("french")) return "fr-FR";
                        if (gameLang.StartsWith("ko") || gameLang.Contains("korean")) return "ko-KR";
                        if (gameLang.StartsWith("pl") || gameLang.Contains("polish")) return "pl-PL";
                        if (gameLang.StartsWith("pt") || gameLang.Contains("portuguese")) return "pt-BR";
                        if (gameLang.StartsWith("ru") || gameLang.Contains("russian")) return "ru-RU";
                        if (gameLang.StartsWith("zh") || gameLang.Contains("chinese")) return "zh-CN";
                        if (gameLang.StartsWith("en") || gameLang.Contains("english")) return "en-US";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlIt] ゲーム言語の取得エラー: {ex.Message}");
            }

            return "en-US";
        }

        private static void LoadLanguage(string langCode)
        {
            _translations.Clear();
            try
            {
                string modPath = PluginManager.instance.GetPluginsInfo()
                    .FirstOrDefault(p => p.userModInstance != null && p.userModInstance.GetType().Assembly == Assembly.GetExecutingAssembly())?.modPath;

                if (string.IsNullOrEmpty(modPath))
                {
                    Debug.LogError("[ControlIt] Modのディレクトリパスが取得できませんでした。");
                    return;
                }

                string translationsFolder = Path.Combine(modPath, "Translations");

                // 探す候補パスのリスト（優先度順）
                List<string> candidatePaths = new List<string>();

                // 1. 完全一致 (例: ja-JP.json)
                candidatePaths.Add(Path.Combine(translationsFolder, $"{langCode}.json"));

                // 2. ハイフン前のみ (例: ja.json)
                if (langCode.Contains("-"))
                {
                    string shortLang = langCode.Split('-')[0];
                    candidatePaths.Add(Path.Combine(translationsFolder, $"{shortLang}.json"));
                }

                // 3. デフォルトフォールバック (en-US.json, en.json)
                candidatePaths.Add(Path.Combine(translationsFolder, "en-US.json"));
                candidatePaths.Add(Path.Combine(translationsFolder, "en.json"));

                string filePath = candidatePaths.FirstOrDefault(p => File.Exists(p));

                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.LogError($"[ControlIt] 翻訳ファイルが一つも見つかりませんでした。 (探索コード: {langCode}, フォルダ: {translationsFolder})");
                    return;
                }

                Debug.Log($"[ControlIt] 翻訳ファイルを読み込み完了: {filePath} (要求コード: {langCode})");

                string jsonText = File.ReadAllText(filePath);
                string pattern = @"\""(.*?)\""\s*:\s*\""((?:[^""\\]|\\.)*)\""";
                MatchCollection matches = Regex.Matches(jsonText, pattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 3)
                    {
                        string key = match.Groups[1].Value;
                        string value = Regex.Unescape(match.Groups[2].Value);
                        _translations[key] = value;
                    }
                }

                Debug.Log($"[ControlIt] ロード成功: {_translations.Count} 件のキーを辞書に登録しました");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ControlIt] 読み込みエラー: {ex.Message}");
            }
        }
    }
}