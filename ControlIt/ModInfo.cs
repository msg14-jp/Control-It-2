using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ControlIt
{
    public class ModInfo : IUserMod
    {
        public string Name => "Control It! 2.0";
        public string Description => Translations.Translate("MOD_DESCRIPTION");

        private static UIComponent _currentOptionsContainer = null;

        public void OnEnabled()
        {
            Translations.Index = ModConfig.Instance.LanguageIndex;
            Translations.Init();

            Translations.eventLanguageChanged -= OnAutoLanguageChanged;
            Translations.eventLanguageChanged += OnAutoLanguageChanged;

            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());

            if (GameObject.Find("MenuContainer") != null)
            {
                EnsureModManager();
            }
        }

        public void OnDisabled()
        {
            Translations.eventLanguageChanged -= OnAutoLanguageChanged;

            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }

            GameObject modManagerGameObject = GameObject.Find("ControlItModManager");
            if (modManagerGameObject != null)
            {
                UnityEngine.Object.Destroy(modManagerGameObject);
            }
        }

        private static void EnsureModManager()
        {
            GameObject modManagerGameObject = GameObject.Find("ControlItModManager");
            if (modManagerGameObject == null)
            {
                modManagerGameObject = new GameObject("ControlItModManager");
                modManagerGameObject.AddComponent<ModManager>();
            }
        }

        private void OnAutoLanguageChanged()
        {
            // UIView上のコルーチンランナーを使って確実にUI再構築を実行
            UIView view = UIView.GetAView();
            if (view != null)
            {
                view.StartCoroutine(RebuildUIAsync(_currentOptionsContainer));
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // 【修正ポイント1】UI描画時に設定値を再読み込みし、Auto判定・翻訳ファイルの適用を強制的に走らせる
            Translations.Index = ModConfig.Instance.LanguageIndex;

            // ゲーム側の言語ドロップダウンのフック登録＆初回同期
            Translations.HookLanguageDropdown();

            UIHelperBase group;
            bool selected;

            UIHelper baseHelper = helper as UIHelper;
            _currentOptionsContainer = null;
            if (baseHelper != null && baseHelper.self is UIComponent baseComponent)
            {
                _currentOptionsContainer = baseComponent as UIScrollablePanel;
                if (_currentOptionsContainer == null)
                {
                    _currentOptionsContainer = baseComponent.GetComponentInChildren<UIScrollablePanel>();
                }
            }

            // --- 言語選択ドロップダウン ---
            group = helper.AddGroup(Name);

            group.AddDropdown(
                Translations.Translate("GROUP_LANGUAGE"),
                Translations.LanguageList,
                ModConfig.Instance.LanguageIndex,
                index =>
                {
                    if (ModConfig.Instance.LanguageIndex != index)
                    {
                        ModConfig.Instance.LanguageIndex = index;
                        ModConfig.Instance.Save();

                        Translations.Index = index;

                        if (_currentOptionsContainer != null)
                        {
                            UIView view = UIView.GetAView();
                            if (view != null)
                            {
                                view.StartCoroutine(RebuildUIAsync(_currentOptionsContainer));
                            }
                        }
                    }
                });

            // --- 見た目の設定 ---
            group = helper.AddGroup(Translations.Translate("GROUP_VISUAL_APPEARANCE"));

            selected = ModConfig.Instance.HideGameLogo;
            group.AddCheckbox(Translations.Translate("HIDE_GAME_LOGO"), selected, sel =>
            {
                ModConfig.Instance.HideGameLogo = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideSubscriptionPanel;
            group.AddCheckbox(Translations.Translate("HIDE_SUBSCRIPTION_PANEL"), selected, sel =>
            {
                ModConfig.Instance.HideSubscriptionPanel = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideDLCPanel;
            group.AddCheckbox(Translations.Translate("HIDE_DLC_PANEL"), selected, sel =>
            {
                ModConfig.Instance.HideDLCPanel = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideAccountPanel;
            group.AddCheckbox(Translations.Translate("HIDE_ACCOUNT_PANEL"), selected, sel =>
            {
                ModConfig.Instance.HideAccountPanel = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideWorkshopPanel;
            group.AddCheckbox(Translations.Translate("HIDE_WORKSHOP_PANEL"), selected, sel =>
            {
                ModConfig.Instance.HideWorkshopPanel = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideMenuBackground;
            group.AddCheckbox(Translations.Translate("HIDE_MENU_BACKGROUND"), selected, sel =>
            {
                ModConfig.Instance.HideMenuBackground = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideChirper;
            group.AddCheckbox(Translations.Translate("HIDE_CHIRPER"), selected, sel =>
            {
                ModConfig.Instance.HideChirper = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideSocialMedia;
            group.AddCheckbox(Translations.Translate("HIDE_SOCIAL_MEDIA"), selected, sel =>
            {
                ModConfig.Instance.HideSocialMedia = sel;
                ModConfig.Instance.Save();
            });

            // --- ネットワーク通信設定 ---
            group = helper.AddGroup(Translations.Translate("GROUP_NETWORK_TRAFFIC"));

            selected = ModConfig.Instance.RestrictAdvertising;
            group.AddCheckbox(Translations.Translate("RESTRICT_ADVERTISING"), selected, sel =>
            {
                ModConfig.Instance.RestrictAdvertising = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.RestrictUserGeneratedContentDetails;
            group.AddCheckbox(Translations.Translate("RESTRICT_UGC_DETAILS"), selected, sel =>
            {
                ModConfig.Instance.RestrictUserGeneratedContentDetails = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.RestrictTelemetry;
            group.AddCheckbox(Translations.Translate("RESTRICT_TELEMETRY"), selected, sel =>
            {
                ModConfig.Instance.RestrictTelemetry = sel;
                ModConfig.Instance.Save();
            });

            // --- 詳細設定 ---
            group = helper.AddGroup(Translations.Translate("GROUP_ADVANCED"));

            selected = ModConfig.Instance.ShowStatistics;
            group.AddCheckbox(Translations.Translate("SHOW_STATISTICS"), selected, sel =>
            {
                ModConfig.Instance.ShowStatistics = sel;
                ModConfig.Instance.Save();
            });
        }

        private IEnumerator RebuildUIAsync(UIComponent container)
        {
            yield return new WaitForSecondsRealtime(0.2f);

            if (container != null && container.gameObject != null && container.isVisible)
            {
                try
                {
                    var children = container.components.ToArray();
                    foreach (var child in children)
                    {
                        if (child != null)
                        {
                            UnityEngine.Object.DestroyImmediate(child.gameObject);
                        }
                    }

                    OnSettingsUI(new UIHelper(container));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ControlIt] UI再構築エラー: {ex.Message}");
                }
            }

            UpdateContentManagerText();
        }

        private void UpdateContentManagerText()
        {
            try
            {
                UIComponent contentManagerUI = UIView.Find<UIComponent>("ContentManagerPanel");

                if (contentManagerUI != null && contentManagerUI.isVisible)
                {
                    var entryPanels = contentManagerUI.GetComponentsInChildren<UIComponent>();
                    foreach (var entry in entryPanels)
                    {
                        UILabel nameLabel = entry.Find<UILabel>("Name");
                        if (nameLabel != null && nameLabel.text.Contains("Control It!"))
                        {
                            UILabel descLabel = entry.Find<UILabel>("Description");
                            if (descLabel != null)
                            {
                                descLabel.text = Translations.Translate("MOD_DESCRIPTION");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log($"[ControlIt] コンテンツマネージャー更新エラー: {ex.Message}");
            }
        }
    }
}