using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;
using CmlLib.Utils;

namespace OfflineMinecraftLauncher;

public partial class LauncherForm : Form
{
    private readonly MSession _session;
    private readonly CMLauncher _launcher;

    public LauncherForm()
    {
        // Make Offline MSession
        _session = new MSession
        {
            AccessToken = "0",
            UUID = "00000000000000000000000000000000"
        };

        // Make CMLauncher with default minecraft path
        _launcher = new CMLauncher(new MinecraftPath());
        _launcher.FileChanged += _launcher_FileChanged;
        _launcher.ProgressChanged += _launcher_ProgressChanged;

        // Load UI
        InitializeComponent();
    }

    private async void LauncherForm_Load(object sender, EventArgs e)
    {
        // Load settings
        usernameInput.Text = Properties.Settings.Default.Username;
        cbVersion.Text = Properties.Settings.Default.Version;

        // Set default Username to Environment Username
        if (string.IsNullOrEmpty(usernameInput.Text))
            usernameInput.Text = Environment.UserName;

        // When loaded, list all versions
        await listVersions();
    }

    private async Task listVersions()
    {
        // Clear list
        cbVersion.Items.Clear();

        // List all release versions
        var versions = await _launcher.GetAllVersionsAsync();
        foreach (var version in versions)
        {
            if (version.MType == MVersionType.Release)
                cbVersion.Items.Add(version.Name);
        }

        // Default latest if not already set
        if (string.IsNullOrEmpty(cbVersion.Text))
            cbVersion.Text = versions.LatestReleaseVersion?.Name;
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        // Disable UI while launching
        this.Enabled = false;
        btnStart.Text = "Launching";

        // Try to launch Minecraft
        _session.Username = usernameInput.Text;
        try
        {
            var process = await _launcher.CreateProcessAsync(cbVersion.Text, new MLaunchOption
            {
                Session = _session
            });
            new ProcessUtil(process).StartWithEvents();

            // Save Setting
            Properties.Settings.Default.Username = usernameInput.Text;
            Properties.Settings.Default.Version = cbVersion.Text;
            Properties.Settings.Default.Save();

            // Exit if successful
            Application.Exit();
        }
        catch (Exception ex)
        {
            // Show error
            MessageBox.Show(ex.ToString());
        }

        // Reset UI
        pbProgress.Value = 0;
        pbFiles.Value = 0;
        lbProgress.Text = "";

        this.Enabled = true;
        btnStart.Text = "Launch";
    }

    private void _launcher_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
    {
        pbProgress.Maximum = 100;
        pbProgress.Value = e.ProgressPercentage;
    }

    private void _launcher_FileChanged(CmlLib.Core.Downloader.DownloadFileChangedEventArgs e)
    {
        pbFiles.Maximum = e.TotalFileCount;
        pbFiles.Value = e.ProgressedFileCount;

        lbProgress.Text = $"[{e.FileKind}] {e.FileName} - {e.ProgressedFileCount} / {e.TotalFileCount}";
    }
}
