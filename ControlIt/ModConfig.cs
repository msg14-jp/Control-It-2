using System.Collections.Generic;
using System.Xml.Serialization;

namespace ControlIt
{
    [ConfigurationPath("ControlItConfig.xml")]
    public class ModConfig
    {
        public bool ConfigUpdated { get; set; }

        // 新しいIDベースの言語指定（デフォルト: 自動）
        public string LanguageId { get; set; } = "auto";

        // 旧インデックス（読み込み時のみ使用し、移行完了後はXMLに出力しない）
        [XmlElement("LanguageIndex")]
        public int LegacyLanguageIndex { get; set; } = -1;

        // ★追加: LegacyLanguageIndex が -1 以外の時だけXMLにシリアライズ（保存）する
        public bool ShouldSerializeLegacyLanguageIndex()
        {
            return LegacyLanguageIndex != -1;
        }

        public bool HideGameLogo { get; set; } = false;
        public bool HideSubscriptionPanel { get; set; } = false;
        public bool HideDLCPanel { get; set; } = false;
        public bool HideAccountPanel { get; set; } = false;
        public bool HideWorkshopPanel { get; set; } = false;
        public bool HideMenuBackground { get; set; } = false;
        public bool HideContinueButton { get; set; } = false;
        public bool HideToolsButton { get; set; } = false;
        public bool HideCreditsButton { get; set; } = false;
        public bool HideChirper { get; set; } = false;
        public bool HideSocialMedia { get; set; } = false;
        public bool RestrictAdvertising { get; set; } = false;
        public bool RestrictUserGeneratedContentDetails { get; set; } = false;
        public bool RestrictTelemetry { get; set; } = false;
        public bool ShowStatistics { get; set; } = false;

        private static ModConfig instance;

        // 旧インデックスからIDへのマッピングテーブル
        private static readonly Dictionary<int, string> IndexToLanguageIdMap = new Dictionary<int, string>()
        {
            { 0, "auto" },
            { 1, "en-US" },
            { 2, "ja-JP" },
            { 3, "de-DE" },
            { 4, "es-ES" },
            { 5, "fr-FR" },
            { 6, "ko-KR" },
            { 7, "pl-PL" },
            { 8, "pt-BR" },
            { 9, "ru-RU" },
            { 10, "zh-CN" }
        };

        public static ModConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Configuration<ModConfig>.Load();
                    instance.MigrateLegacyConfig();
                }

                return instance;
            }
        }

        /// <summary>
        /// 旧バージョンのLanguageIndexが存在する場合、LanguageIdへマイグレーションする
        /// </summary>
        private void MigrateLegacyConfig()
        {
            // 旧インデックスが設定されている場合（デフォルト値 -1 以外）
            if (LegacyLanguageIndex != -1)
            {
                if (IndexToLanguageIdMap.TryGetValue(LegacyLanguageIndex, out string mappedId))
                {
                    LanguageId = mappedId;
                }

                // 移行完了後にリセットし、設定を新形式で保存
                LegacyLanguageIndex = -1;
                Save();
            }
        }

        public void Save()
        {
            Configuration<ModConfig>.Save();
            ConfigUpdated = true;
        }
    }
}