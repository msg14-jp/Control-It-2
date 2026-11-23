using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Reflection.Emit;
using UnityEngine;

namespace ControlIt
{
    public class ModManager : MonoBehaviour
    {
        private bool _initialized;
        private float _timer;

        private UIPanel _menuContainer;
        private UISlicedSprite _centerPart;
        private UIButton _continueButton;
        private UIPanel _menu; // Toolsボタンの親要素
        private UIButton _toolsButton;
        private UIButton _creditsButton;
        private UIPanel _steamSubscriptionPanel;
        private UIPanel _subscriptionOrNewsPanel;
        private UIPanel _paradoxAccountPanel;
        private UIPanel _dlcPanel;
        private UIScrollablePanel _dlcPanelNewScrollablePanel;
        private UIPanel _workshopAdPanel;
        private UIScrollablePanel _workshopAdPanelScrollablePanel;
        private UILabel _workshopAdPanelDisabledLabel;
        private UISprite _chirper;
        private UIPanel _socialMediaButtons;
        private UITextureSprite _gameLogo;
        private UIPanel _networkTrafficRestricedPanel;
        private UILabel _networkTrafficRestricedLabel;
        private UILabel _networkTrafficRestricedNumber;

        public void Awake()
        {
            try
            {

            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:Awake -> Exception: " + e.Message);
            }
        }

        public void Start()
        {
            try
            {
                RefreshReferences();
                CreateUI();
            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:Start -> Exception: " + e.Message);
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (_networkTrafficRestricedNumber != null)
                {
                    Destroy(_networkTrafficRestricedNumber.gameObject);
                }

                if (_networkTrafficRestricedLabel != null)
                {
                    Destroy(_networkTrafficRestricedLabel.gameObject);
                }

                if (_networkTrafficRestricedPanel != null)
                {
                    Destroy(_networkTrafficRestricedPanel.gameObject);
                }

                if (_subscriptionOrNewsPanel != null)
                {
                    _subscriptionOrNewsPanel.isVisible = true;
                }

                if (_dlcPanelNewScrollablePanel != null)
                {
                    _dlcPanelNewScrollablePanel.isVisible = true;
                }

                if (_workshopAdPanelScrollablePanel != null)
                {
                    _workshopAdPanelScrollablePanel.isVisible = !PluginManager.noWorkshop;
                }

                if (_workshopAdPanelDisabledLabel != null)
                {
                    _workshopAdPanelDisabledLabel.isVisible = PluginManager.noWorkshop;
                }

                if (_centerPart != null)
                {
                    _centerPart.fillAmount = 1f;
                }

                if (_continueButton != null)
                {
                    _continueButton.isVisible = true;
                }

                if (_toolsButton != null)
                {
                    _toolsButton.isVisible = true;
                }

                if (_creditsButton != null)
                {
                    _creditsButton.isVisible = true;
                }

                if (_steamSubscriptionPanel != null)
                {
                    _steamSubscriptionPanel.isVisible = true;
                }

                if (_paradoxAccountPanel != null)
                {
                    _paradoxAccountPanel.isVisible = true;
                }

                if (_dlcPanel != null)
                {
                    _dlcPanel.isVisible = true;
                }

                if (_workshopAdPanel != null)
                {
                    _workshopAdPanel.isVisible = true;
                }

                if (_chirper != null)
                {
                    _chirper.isVisible = true;
                }

                if (_socialMediaButtons != null)
                {
                    _socialMediaButtons.isVisible = true;
                }

                if (_gameLogo != null)
                {
                    _gameLogo.isVisible = true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:OnDestroy -> Exception: " + e.Message);
            }
        }

        public void Update()
        {
            try
            {
                _timer += Time.deltaTime;

                // 参照のチェック（参照が切れていれば再取得するが、これ自体をUpdateUIのフラグにしない）
                CheckAndRefreshReferences();

                // 1秒周期、または設定変更時（ConfigUpdated）のみUI更新チェックを行う
                if (!_initialized || ModConfig.Instance.ConfigUpdated || _timer > 1.0f)
                {
                    _timer = 0f;

                    if (NeedUIUpdate())
                    {
                        UpdateUI();
                    }

                    _initialized = true;
                    ModConfig.Instance.ConfigUpdated = false;
                }

                // 統計表示の更新
                if (ModConfig.Instance.ShowStatistics && _networkTrafficRestricedPanel != null && _networkTrafficRestricedPanel.isVisible)
                {
                    UpdateStatistics();
                }
            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:Update -> Exception: " + e.Message);
            }
        }

        // 参照が切れているかを毎フレーム監視し、切れていれば即座にキャッチし直す
        private bool CheckAndRefreshReferences()
        {
            if (_menuContainer == null || !_menuContainer.gameObject.activeInHierarchy ||
                _paradoxAccountPanel == null || _workshopAdPanel == null || _dlcPanel == null ||
                _continueButton == null || _toolsButton == null || _creditsButton == null)
            {
                RefreshReferences();
                return true; // 参照の再取得が発生した
            }
            return false;
        }

        // 参照を一括取得するヘルパー
        private void RefreshReferences()
        {
            if (_menuContainer == null || !_menuContainer.gameObject.activeInHierarchy)
            {
                _menuContainer = GameObject.Find("MenuContainer")?.GetComponent<UIPanel>();
            }

            if (_menuContainer != null)
            {
                _centerPart = _menuContainer.Find("CenterPart")?.GetComponent<UISlicedSprite>();
                _steamSubscriptionPanel = _menuContainer.Find("SteamSubscriptionPanel")?.GetComponent<UIPanel>();
                _subscriptionOrNewsPanel = _menuContainer.Find("SubscriptionOrNewsPanel")?.GetComponent<UIPanel>();
                _paradoxAccountPanel = _menuContainer.Find("ParadoxAccountPanel")?.GetComponent<UIPanel>();
                _dlcPanel = _menuContainer.Find("DLCPanel")?.GetComponent<UIPanel>();

                if (_dlcPanel != null)
                {
                    _dlcPanelNewScrollablePanel = _dlcPanel.Find("ScrollablePanel")?.GetComponent<UIScrollablePanel>();
                }

                _workshopAdPanel = _menuContainer.Find("WorkshopAdPanel")?.GetComponent<UIPanel>();

                if (_workshopAdPanel != null)
                {
                    _workshopAdPanelScrollablePanel = _workshopAdPanel.Find("Container")?.GetComponent<UIScrollablePanel>();
                    _workshopAdPanelDisabledLabel = _workshopAdPanel.Find("DisabledLabel")?.GetComponent<UILabel>();
                }

                _chirper = _menuContainer.Find("Chirper")?.GetComponent<UISprite>();
                _socialMediaButtons = _menuContainer.Find("SocialMediaButtons")?.GetComponent<UIPanel>();
                _gameLogo = _menuContainer.Find("Logo")?.GetComponent<UITextureSprite>();

                // ★追加: Menuパネルとその配下のToolsボタンの参照を取得
                _menu = _menuContainer.Find("Menu")?.GetComponent<UIPanel>();
                if (_menu != null)
                {
                    _continueButton = _menu.Find("Continue")?.GetComponent<UIButton>();
                    _toolsButton = _menu.Find("Tools")?.GetComponent<UIButton>();
                    _creditsButton = _menu.Find("Credits")?.GetComponent<UIButton>();
                }
            }
        }

        // UIの状態が現在の設定値と喰い違っているかを双方向（表示・非表示の両方）で判定
        private bool NeedUIUpdate()
        {
            if (_menuContainer == null || !_menuContainer.gameObject.activeInHierarchy) return false;

            // メニュー背景（CenterPart）: 0.01f を閾値にして判定のチラつきを防止
            if (_centerPart != null)
            {
                bool hide = ModConfig.Instance.HideMenuBackground;
                if (hide && _centerPart.fillAmount > 0.01f) return true;
                if (!hide && _centerPart.fillAmount < 0.99f) return true;
            }

            // Continueボタン
            if (_continueButton != null)
            {
                bool hide = ModConfig.Instance.HideContinueButton;
                if (hide != !_continueButton.isVisible) return true;
            }

            // Toolsボタン
            if (_toolsButton != null)
            {
                bool hide = ModConfig.Instance.HideToolsButton;
                if (hide != !_toolsButton.isVisible) return true;
            }

            // Creditsボタン
            if (_creditsButton != null)
            {
                bool hide = ModConfig.Instance.HideCreditsButton;
                if (hide != !_creditsButton.isVisible) return true;
            }

            // パラドックスアカウントパネル
            if (_paradoxAccountPanel != null)
            {
                bool hide = ModConfig.Instance.HideAccountPanel;
                if (hide != !_paradoxAccountPanel.isVisible) return true;
            }

            // DLCパネル
            if (_dlcPanel != null)
            {
                bool hide = ModConfig.Instance.HideDLCPanel;
                if (hide != !_dlcPanel.isVisible) return true;
            }

            // ワークショップパネル
            if (_workshopAdPanel != null)
            {
                bool hide = ModConfig.Instance.HideWorkshopPanel;
                if (hide != !_workshopAdPanel.isVisible) return true;
            }

            // サブスク/ニュースパネル
            if (_steamSubscriptionPanel != null)
            {
                bool hide = ModConfig.Instance.HideSubscriptionPanel;
                if (hide != !_steamSubscriptionPanel.isVisible) return true;
            }

            // Chirper
            if (_chirper != null)
            {
                bool hide = ModConfig.Instance.HideChirper;
                if (hide != !_chirper.isVisible) return true;
            }

            // SNSボタン
            if (_socialMediaButtons != null)
            {
                bool hide = ModConfig.Instance.HideSocialMedia;
                if (hide != !_socialMediaButtons.isVisible) return true;
            }

            // ゲームロゴ
            if (_gameLogo != null)
            {
                bool hide = ModConfig.Instance.HideGameLogo;
                if (hide != !_gameLogo.isVisible) return true;
            }

            // 統計表示
            if (_networkTrafficRestricedPanel != null)
            {
                bool show = ModConfig.Instance.ShowStatistics;
                if (show != _networkTrafficRestricedPanel.isVisible) return true;
            }

            return false;
        }

        private void CreateUI()
        {
            try
            {
                _networkTrafficRestricedPanel = UIUtils.CreatePanel("ControlItNetworkTrafficRestricedPanel");
                _networkTrafficRestricedPanel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
                _networkTrafficRestricedPanel.width = 500f;
                _networkTrafficRestricedPanel.height = 20f;
                _networkTrafficRestricedPanel.absolutePosition = new Vector3(12f, UIView.GetAView().GetScreenResolution().y - 32f);

                _networkTrafficRestricedLabel = UIUtils.CreateLabel(_networkTrafficRestricedPanel, "NetworkTrafficRestricedLabel", "Restricted in current session: ");
                _networkTrafficRestricedLabel.font = UIUtils.GetUIFont("OpenSans-Semibold");
                _networkTrafficRestricedLabel.autoSize = true;
                _networkTrafficRestricedLabel.height = 18f;
                _networkTrafficRestricedLabel.anchor = UIAnchorStyle.Right | UIAnchorStyle.Top;
                _networkTrafficRestricedLabel.relativePosition = new Vector3(0f, 0f);

                _networkTrafficRestricedNumber = UIUtils.CreateLabel(_networkTrafficRestricedPanel, "NetworkTrafficRestricedNumber", "0 (UGC Details), 0 (Telemetry)");
                _networkTrafficRestricedNumber.font = UIUtils.GetUIFont("OpenSans-Semibold");
                _networkTrafficRestricedNumber.autoSize = true;
                _networkTrafficRestricedNumber.height = 18f;
                _networkTrafficRestricedNumber.anchor = UIAnchorStyle.Right | UIAnchorStyle.Top;
                _networkTrafficRestricedNumber.relativePosition = new Vector3(_networkTrafficRestricedLabel.width + 5f, 0f);

                UpdateUI();

            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:CreateUI -> Exception: " + e.Message);
            }
        }

        private void UpdateUI()
        {
            try
            {
                if (_subscriptionOrNewsPanel != null)
                {
                    _subscriptionOrNewsPanel.isVisible = !ModConfig.Instance.HideSubscriptionPanel;
                }
                if (_dlcPanelNewScrollablePanel != null)
                {
                    _dlcPanelNewScrollablePanel.isVisible = !ModConfig.Instance.RestrictAdvertising;
                }
                if (_workshopAdPanelScrollablePanel != null)
                {
                    _workshopAdPanelScrollablePanel.isVisible = !ModConfig.Instance.RestrictAdvertising;
                }
                if (_workshopAdPanelDisabledLabel != null)
                {
                    _workshopAdPanelDisabledLabel.isVisible = false;
                }
                if (_centerPart != null)
                {
                    _centerPart.fillAmount = ModConfig.Instance.HideMenuBackground ? 0f : 1f;
                }
                if (_continueButton != null)
                {
                    _continueButton.isVisible = !ModConfig.Instance.HideContinueButton;
                }
                if (_toolsButton != null)
                {
                    _toolsButton.isVisible = !ModConfig.Instance.HideToolsButton;
                }
                if (_creditsButton != null)
                {
                    _creditsButton.isVisible = !ModConfig.Instance.HideCreditsButton;
                }
                if (_steamSubscriptionPanel != null)
                {
                    _steamSubscriptionPanel.isVisible = !ModConfig.Instance.HideSubscriptionPanel;
                }
                if (_paradoxAccountPanel != null)
                {
                    _paradoxAccountPanel.isVisible = !ModConfig.Instance.HideAccountPanel;
                }
                if (_dlcPanel != null)
                {
                    _dlcPanel.isVisible = !ModConfig.Instance.HideDLCPanel;
                }
                if (_workshopAdPanel != null)
                {
                    _workshopAdPanel.isVisible = !ModConfig.Instance.HideWorkshopPanel;
                }
                if (_chirper != null)
                {
                    _chirper.isVisible = !ModConfig.Instance.HideChirper;
                }
                if (_socialMediaButtons != null)
                {
                    _socialMediaButtons.isVisible = !ModConfig.Instance.HideSocialMedia;
                }
                if (_gameLogo != null)
                {
                    _gameLogo.isVisible = !ModConfig.Instance.HideGameLogo;
                }
                if (ModConfig.Instance.ShowStatistics)
                {
                    _networkTrafficRestricedPanel.isVisible = true;
                }
                else
                {
                    _networkTrafficRestricedPanel.isVisible = false;
                }
            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:UpdateUI -> Exception: " + e.Message);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                _networkTrafficRestricedNumber.text = string.Format("~{0} (UGC Details), ~{1} (Telemetry)", Statistics.Instance.UserGeneratedContentDetailsRequestRestricted.ToString(), Statistics.Instance.TelemetryEntriesSendRestricted.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("[Control It!] ModManager:UpdateStatistics -> Exception: " + e.Message);
            }
        }
    }
}