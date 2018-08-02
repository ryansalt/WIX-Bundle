namespace InstallerUI.Bootstrapper
{
    public enum InstallationStatus
    {
        Initializing,
        DetectedAbsent,
        DetectedPresent,
        DetectedNewer,
        Applying,
        Applied,
        Failed,
    }
}
