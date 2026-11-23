using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ControlIt
{
    public class ModInfo : IUserMod
    {
        public string Name
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"Control It! {version.Major}.{version.Minor}.{version.Build}";
            }
        }
        public string Description => Translations.Translate("MOD_DESCRIPTION");

        private static UIComponent _currentOptionsContainer = null;
        private bool _isRebuildingUI = false; // 再構築の無限ループを防ぐフラグ

        public void OnEnabled()
        {
            Translations.Language = ModConfig.Instance.LanguageId;
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
            // UI再構築中なら重複実行しない
            if (_isRebuildingUI) return;

            UIView view = UIView.GetAView();
            if (view != null && _currentOptionsContainer != null)
            {
                view.StartCoroutine(RebuildUIAsync(_currentOptionsContainer));
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // ★ポイント1: OnSettingsUI冒頭での Translations.Language への再代入を削除してループを防止

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

            int currentDropdownIndex = GetCurrentLanguageDropdownIndex();

            group.AddDropdown(
                Translations.Translate("GROUP_LANGUAGE"),
                Translations.LanguageList,
                currentDropdownIndex,
                index =>
                {
                    string selectedLanguageId = GetLanguageIdFromIndex(index);

                    if (ModConfig.Instance.LanguageId != selectedLanguageId)
                    {
                        ModConfig.Instance.LanguageId = selectedLanguageId;
                        ModConfig.Instance.Save();

                        // 言語を変更（これが eventLanguageChanged を発火させる）
                        Translations.Language = selectedLanguageId;
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

            selected = ModConfig.Instance.HideContinueButton;
            group.AddCheckbox(Translations.Translate("HIDE_CONTINUE_BUTTON"), selected, sel =>
            {
                ModConfig.Instance.HideContinueButton = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideToolsButton;
            group.AddCheckbox(Translations.Translate("HIDE_TOOLS_BUTTON"), selected, sel =>
            {
                ModConfig.Instance.HideToolsButton = sel;
                ModConfig.Instance.Save();
            });

            selected = ModConfig.Instance.HideCreditsButton;
            group.AddCheckbox(Translations.Translate("HIDE_CREDITS_BUTTON"), selected, sel =>
            {
                ModConfig.Instance.HideCreditsButton = sel;
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

        private int GetCurrentLanguageDropdownIndex()
        {
            if (Translations.LanguageIDs != null)
            {
                int index = Array.IndexOf(Translations.LanguageIDs, ModConfig.Instance.LanguageId);
                if (index >= 0)
                {
                    return index;
                }
            }
            return 0;
        }

        private string GetLanguageIdFromIndex(int index)
        {
            if (Translations.LanguageIDs != null && index >= 0 && index < Translations.LanguageIDs.Length)
            {
                return Translations.LanguageIDs[index];
            }
            return "en-US";
        }

        private IEnumerator RebuildUIAsync(UIComponent container)
        {
            // ★ポイント2: 二重実行を防ぐガードフラグをオンにする
            if (_isRebuildingUI) yield break;
            _isRebuildingUI = true;

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

            // ★ポイント3: 処理が終わったらガードフラグを解除
            _isRebuildingUI = false;
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