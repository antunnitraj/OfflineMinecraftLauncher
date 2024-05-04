using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;
using CmlLib.Utils;

namespace OfflineMinecraftLauncher;

public partial class LauncherForm : Form
{
    private readonly CMLauncher _launcher;

    public LauncherForm()
    {
        // Make CMLauncher with default minecraft path
        _launcher = new CMLauncher(new MinecraftPath());

        // Attach events with functions
        _launcher.FileChanged += _launcher_FileChanged;
        _launcher.ProgressChanged += _launcher_ProgressChanged;

        // Load UI
        InitializeComponent();
    }

    private async void LauncherForm_Load(object sender, EventArgs e)
    {
        // Load previous used values in the inputs
        usernameInput.Text = Properties.Settings.Default.Username;
        cbVersion.Text = Properties.Settings.Default.Version;

        // Set default username to Environment.UserName if empty
        if (string.IsNullOrEmpty(usernameInput.Text))
            usernameInput.Text = Environment.UserName;

        // When loaded, list all versions
        await listVersions();
    }

    private async Task listVersions(bool includeAll = false)
    {
        // Clear list
        cbVersion.Items.Clear();

        // List all versions
        var versions = await _launcher.GetAllVersionsAsync();
        foreach (var version in versions)
        {
            // Check if includeAll is enabled
            if (version.MType == MVersionType.Release || version.MType == MVersionType.Custom || includeAll)
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

        // Try to launch Minecraft with an Offline session
        try
        {
            var session = MSession.CreateOfflineSession(usernameInput.Text);
            var process = await _launcher.CreateProcessAsync(cbVersion.Text, new MLaunchOption
            {
                Session = session
            });
            new ProcessUtil(process).StartWithEvents();

            // Save values
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

    private async void minecraftVersion_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Load all versions if "All Versions" is selected
        await listVersions(minecraftVersion.Text == "All Versions");
    }
}
