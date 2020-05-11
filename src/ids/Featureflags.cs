namespace gateway
{
    public class KeyStoreMigrationPlan
    {
        public bool AnnounceKey1 { get; set; }
        public bool RetireLegacyKey { get; set; }
        public bool FullyMigratedKeyStore { get; set; }

    }
}