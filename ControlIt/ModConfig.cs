namespace ControlIt
{
    [ConfigurationPath("ControlItConfig.xml")]
    public class ModConfig
    {
        public bool ConfigUpdated { get; set; }
        public int LanguageIndex { get; set; } = 0;
        public bool HideGameLogo { get; set; } = false;
        public bool HideSubscriptionPanel { get; set; } = false;
        public bool HideDLCPanel { get; set; } = false;
        public bool HideAccountPanel { get; set; } = false;
        public bool HideWorkshopPanel { get; set; } = false;
        public bool HideMenuBackground { get; set; } = false;
        public bool HideChirper { get; set; } = false;
        public bool HideSocialMedia { get; set; } = false;
        public bool RestrictAdvertising { get; set; } = false;
        public bool RestrictUserGeneratedContentDetails { get; set; } = false;
        public bool RestrictTelemetry { get; set; } = false;
        public bool ShowStatistics { get; set; } = false;

        private static ModConfig instance;

        public static ModConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Configuration<ModConfig>.Load();
                }

                return instance;
            }
        }

        public void Save()
        {
            Configuration<ModConfig>.Save();
            ConfigUpdated = true;
        }
    }
}