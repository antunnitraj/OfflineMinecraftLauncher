using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;
using CmlLib.Utils;

namespace OfflineMinecraftLauncher;

public partial class LauncherForm : Form
{
    private readonly CMLauncher _launcher;

    private string _playerUuid = string.Empty;

    private ToolTip _characterPreviewTooltip = new ToolTip();

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

        // Adding tooltip to the character picture box and help icon
        new ToolTip().SetToolTip(characterHelpPictureBox, characterHelpPictureBox.Tag?.ToString());
        _characterPreviewTooltip.SetToolTip(characterPictureBox, characterPictureBox.Tag?.ToString());

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
            session.UUID = _playerUuid;

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

    private void usernameInput_TextChanged(object sender, EventArgs e)
    {
        // Disabling the start button if the username is empty
        if (string.IsNullOrEmpty(usernameInput.Text))
        {
            // If the username is empty, reset the UUID and character picture
            _playerUuid = string.Empty;
            _characterPreviewTooltip.SetToolTip(characterPictureBox, null);
            characterPictureBox.Image = null;
            characterPictureBox.Tag = null;
            btnStart.Enabled = false; // Disable the start button if username is empty
            return;
        }
        else
        {
            btnStart.Enabled = true;
        }

        // Update the UUID when the username changes
        _playerUuid = Character.GenerateUuidFromUsername(usernameInput.Text);

        // Update character picture box
        updateCharacterPreview();
    }

    private async void minecraftVersion_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Load all versions if "All Versions" is selected
        await listVersions(minecraftVersion.Text == "All Versions");
    }

    private void cbVersion_TextChanged(object sender, EventArgs e)
    {
        updateCharacterPreview();
    }

    private void updateCharacterPreview()
    {
        // Update the character preview based on the selected version
        string resourceName = Character.GetCharacterResourceNameFromUuidAndGameVersion(_playerUuid, cbVersion.Text);
        if (!string.IsNullOrEmpty(resourceName))
        {
            Bitmap? bitmapFromResources = (Bitmap?)Properties.Resources.ResourceManager.GetObject(resourceName);
            if (bitmapFromResources != null)
            {
                characterPictureBox.Image = bitmapFromResources;
                characterPictureBox.Tag = resourceName.Replace("_", " ");
                _characterPreviewTooltip.SetToolTip(characterPictureBox, characterPictureBox.Tag?.ToString());
            }
        }
    }

}
