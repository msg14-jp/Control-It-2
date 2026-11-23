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
            "简体中文",
            "繁體中文",
            "Italiano",
            "Українська",
            "Português (Portugal)",
            "Монгол хэл",
            "Ελληνικά",
            "Nederlands",
            "Dansk",
            "Suomi",
            "Kalaallisut",
            "Íslenska",
            "Svenska",
            "Norsk (Bokmål)",
            "Norsk (Nynorsk)",
            "ไทย",
            "Tiếng Việt",
            "हिन्दी",
            "العربية"
        };

        // 各言語のID（LanguageId）として使用
        public static readonly string[] LanguageIDs = new string[]
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
            "zh-CN",
            "zh-TW",
            "it-IT",
            "uk-UA",
            "pt-PT",
            "mn-MN",
            "el-GR",
            "nl-NL",
            "da-DK",
            "fi-FI",
            "kl-GL",
            "is-IS",
            "sv-SE",
            "nb-NO",
            "nn-NO",
            "th-TH",
            "vi-VN",
            "hi-IN",
            "ar-SA"
        };

        private static string _currentLanguageId = "auto";
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static bool _isHooked = false;
        private static string _loadedLanguageCode = "";

        /// <summary>
        /// IDで言語を設定・取得するプロパティ
        /// </summary>
        public static string Language
        {
            get => _currentLanguageId;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "auto";
                }

                // リスト内に存在するIDか検証（存在しなければ auto にフォールバック）
                if (LanguageIDs.Contains(value))
                {
                    _currentLanguageId = value;
                }
                else
                {
                    _currentLanguageId = "auto";
                }

                Debug.Log($"[ControlIt] Language ID変更: {_currentLanguageId}");
                LoadLanguage(GetTargetLanguageCode(_currentLanguageId));

                eventLanguageChanged?.Invoke();
            }
        }

        /// <summary>
        /// ゲーム本体に「言語設定が変わったよ」と安全に通知してUIテキストを再取得させる
        /// </summary>
        private static void NotifyLocaleChanged()
        {
            try
            {
                if (LocaleManager.exists)
                {
                    // ゲーム本体のLocaleManagerに言語更新イベントを直接発火させる
                    // (ForceReloadと違ってファイルを再読み込みせず、安全にイベントだけを飛ばす)
                    typeof(LocaleManager)
                        .GetMethod("OnLocaleChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        ?.Invoke(LocaleManager.instance, null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlIt] Locale変更通知で例外が発生: {ex.Message}");
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
            LoadLanguage(GetTargetLanguageCode(_currentLanguageId));
        }

        public static void HookLanguageDropdown()
        {
            try
            {
                UIDropDown langDropdown = UIView.Find<UIDropDown>("Language");
                if (langDropdown != null)
                {
                    // 重複登録を防止するために解除してから登録
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
            if (_currentLanguageId == "auto")
            {
                Debug.Log("[ControlIt] ゲーム本体の言語変更を検知したため、Auto設定を更新します");
                LoadLanguage(GetTargetLanguageCode("auto"));
                eventLanguageChanged?.Invoke();
            }
        }

        public static string Translate(string key)
        {
            // イベント未登録なら登録を試みる
            if (!_isHooked)
            {
                EnsureInitialized();
            }

            // ★MODが有効かどうかで読み込む言語コードを決定する
            string targetLangCode;
            if (IsModEnabled())
            {
                // MOD有効時：ユーザーが設定した言語（auto または 個別指定）を使用
                targetLangCode = GetTargetLanguageCode(_currentLanguageId);
            }
            else
            {
                // MOD無効時：ユーザーの設定を無視して、強制的にゲーム本体の言語（auto）に追従
                targetLangCode = GetTargetLanguageCode("auto");
            }

            // 辞書が空、または読み込み済み言語とズレている場合に再ロード
            if (_translations == null || _translations.Count == 0 || _loadedLanguageCode != targetLangCode)
            {
                LoadLanguage(targetLangCode);
            }

            if (_translations != null && _translations.TryGetValue(key, out string translated))
            {
                return translated;
            }
            return key;
        }

        /// <summary>
        /// このMODが現在有効化されているかチェックするヘルパー
        /// </summary>
        private static bool IsModEnabled()
        {
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                PluginManager.PluginInfo plugin = PluginManager.instance.GetPluginsInfo()
                    .FirstOrDefault(p => p.GetAssemblies().Any(a => a == executingAssembly));

                return plugin != null && plugin.isEnabled;
            }
            catch
            {
                return false;
            }
        }

        // MOD無効時でもLocaleManagerのイベントを安全にフックするためのヘルパー
        private static void EnsureInitialized()
        {
            try
            {
                if (LocaleManager.exists)
                {
                    LocaleManager.eventLocaleChanged -= OnGameLocaleChanged;
                    LocaleManager.eventLocaleChanged += OnGameLocaleChanged;
                }
            }
            catch
            {
                // 初期化タイミングによる例外を予防
            }
        }

        /// <summary>
        /// 指定したLanguageIDから、実際に読み込むべき言語コード（ja-JP, en-US等）を決定する
        /// </summary>
        private static string GetTargetLanguageCode(string langId)
        {
            if (langId == "auto")
            {
                return GetGameLanguageCode();
            }

            if (!string.IsNullOrEmpty(langId))
            {
                return langId;
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
            _loadedLanguageCode = langCode;

            try
            {
                // 現在実行中のアセンブリ（DLL）を取得
                Assembly executingAssembly = Assembly.GetExecutingAssembly();

                // userModInstanceを見ずに、アセンブリ一覧に自分が含まれているかでMODのパスを特定する
                string modPath = PluginManager.instance.GetPluginsInfo()
                    .FirstOrDefault(p => p.GetAssemblies().Any(a => a == executingAssembly))?.modPath;

                if (string.IsNullOrEmpty(modPath))
                {
                    Debug.LogError("[ControlIt] Modのディレクトリパスが取得できませんでした。");
                    return;
                }

                string translationsFolder = Path.Combine(modPath, "Translations");

                // 指定言語のJSONを取得
                string filePath = Path.Combine(translationsFolder, $"{langCode}.json");

                // 指定言語のJSONが存在しない場合は英語にフォールバック
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(translationsFolder, "en-US.json");
                }

                if (!File.Exists(filePath))
                {
                    Debug.LogError($"[ControlIt] 翻訳ファイルが見つかりませんでした。 (探索コード: {langCode}, フォルダ: {translationsFolder})");
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