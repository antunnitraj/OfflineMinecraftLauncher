using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using CmlLib.Core.VersionMetadata;

namespace OfflineMinecraftLauncher;

public partial class LauncherForm : Form
{
    private readonly MinecraftLauncher _launcher;
    private string _playerUuid = string.Empty;

    private readonly ToolTip _characterPreviewTooltip = new ToolTip();
    private readonly ToolTip _helpTooltip = new ToolTip();

    public LauncherForm()
    {
        // Make CMLauncher with default minecraft path
        _launcher = new MinecraftLauncher(new MinecraftPath());

        // Attach events with functions
        _launcher.FileProgressChanged += _launcher_FileProgressChanged;
        _launcher.ByteProgressChanged += _launcher_ByteProgressChanged;

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
        _helpTooltip.SetToolTip(characterHelpPictureBox, characterHelpPictureBox.Tag?.ToString());
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
            if (version.GetVersionType() == MVersionType.Release ||
                version.GetVersionType() == MVersionType.Custom ||
                includeAll)
            {
                cbVersion.Items.Add(version.Name);
            }
        }

        // Default latest if not already set
        if (string.IsNullOrEmpty(cbVersion.Text))
            cbVersion.Text = versions.LatestReleaseName;
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        // Disable UI while launching
        if (string.IsNullOrEmpty(cbVersion.Text))
        {
            MessageBox.Show("Please select a Minecraft version.");
            return;
        }

        if (string.IsNullOrEmpty(usernameInput.Text))
        {
            MessageBox.Show("Please enter a username.");
            return;
        }

        this.Enabled = false;
        btnStart.Text = "Launching";

        // Try to launch Minecraft with an Offline session
        try
        {
            var session = MSession.CreateOfflineSession(usernameInput.Text);
            session.UUID = _playerUuid;

            await _launcher.InstallAsync(cbVersion.Text);
            var process = await _launcher.BuildProcessAsync(cbVersion.Text, new MLaunchOption
            {
                Session = session
            });
            process.Start();

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
            MessageBox.Show($"Failed to launch Minecraft:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Reset UI
            pbProgress.Value = 0;
            pbFiles.Value = 0;
            lbProgress.Text = "";

            this.Enabled = true;
            btnStart.Text = "Launch";
        }
    }

    private void _launcher_FileProgressChanged(object? sender, InstallerProgressChangedEventArgs args)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => _launcher_FileProgressChanged(sender, args)));
            return;
        }

        pbFiles.Maximum = args.TotalTasks;
        pbFiles.Value = Math.Min(args.ProgressedTasks, args.TotalTasks);
        lbProgress.Text = $"{args.Name} - {args.ProgressedTasks} / {args.TotalTasks}";
    }

    private void _launcher_ByteProgressChanged(object? sender, ByteProgress args)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => _launcher_ByteProgressChanged(sender, args)));
            return;
        }

        pbProgress.Maximum = 100;
        pbProgress.Value = (int)(args.ProgressedBytes * 100 / args.TotalBytes);
    }

    private void usernameInput_TextChanged(object sender, EventArgs e)
    {
        // Disabling the start button if the username is empty
        if (string.IsNullOrWhiteSpace(usernameInput.Text))
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
