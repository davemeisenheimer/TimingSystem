using System;
using System.Globalization;
using System.Windows;
using TrailMeisterDb;

namespace TrailMeisterViewer.Windows.Settings
{
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
            PopulateMonths();
            LoadCurrentSettings();
        }

        private void PopulateMonths()
        {
            for (int m = 1; m <= 12; m++)
                cboSeasonMonth.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m));
        }

        private void LoadCurrentSettings()
        {
            var s = AppSettings.Current;
            tbOutputDir.Text           = s.ExportOutputDirectory;
            tbDbServer.Text            = s.DbServer;
            tbDbName.Text              = s.DbName;
            tbDbUserId.Text            = s.DbUserId;
            pbDbPassword.Password      = s.DbPassword;
            tbHandicapMinLap.Text      = s.HandicapMinLapLengthM.ToString();
            tbHandicapPenalty.Text     = (s.HandicapPenaltyPerHundredM * 100).ToString("G");
            tbHandicapMaxPenalty.Text  = (s.HandicapMaxPenalty * 100).ToString("G");
            cboSeasonMonth.SelectedIndex = s.SeasonStartMonth - 1;
            tbArduinoIp.Text           = s.ArduinoIpAddress;
            tbArduinoPort.Text         = s.ArduinoPort.ToString();
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select export output folder",
                SelectedPath = tbOutputDir.Text,
                ShowNewFolderButton = true
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                tbOutputDir.Text = dlg.SelectedPath;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (!TryParseFields(out var parsed)) return;

            var s = AppSettings.Current;
            s.ExportOutputDirectory       = tbOutputDir.Text.Trim();
            s.DbServer                    = tbDbServer.Text.Trim();
            s.DbName                      = tbDbName.Text.Trim();
            s.DbUserId                    = tbDbUserId.Text.Trim();
            s.DbPassword                  = pbDbPassword.Password;
            s.HandicapMinLapLengthM       = parsed.MinLap;
            s.HandicapPenaltyPerHundredM  = parsed.Penalty;
            s.HandicapMaxPenalty          = parsed.MaxPenalty;
            s.SeasonStartMonth            = cboSeasonMonth.SelectedIndex + 1;
            s.ArduinoIpAddress            = tbArduinoIp.Text.Trim();
            s.ArduinoPort                 = parsed.Port;

            AppSettingsService.Save();
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;

        private bool TryParseFields(out (int MinLap, double Penalty, double MaxPenalty, int Port) parsed)
        {
            parsed = default;

            if (!int.TryParse(tbHandicapMinLap.Text, out int minLap) || minLap < 0)
            {
                Error("Min lap length must be a non-negative whole number.");
                return false;
            }
            if (!double.TryParse(tbHandicapPenalty.Text, out double penaltyPct) || penaltyPct < 0)
            {
                Error("Penalty per 100 m must be a non-negative number.");
                return false;
            }
            if (!double.TryParse(tbHandicapMaxPenalty.Text, out double maxPenaltyPct) || maxPenaltyPct < 0)
            {
                Error("Maximum penalty must be a non-negative number.");
                return false;
            }
            if (cboSeasonMonth.SelectedIndex < 0)
            {
                Error("Please select a season start month.");
                return false;
            }
            if (!int.TryParse(tbArduinoPort.Text, out int port) || port < 1 || port > 65535)
            {
                Error("Arduino port must be a number between 1 and 65535.");
                return false;
            }

            parsed = (minLap, penaltyPct / 100.0, maxPenaltyPct / 100.0, port);
            return true;
        }

        private void Error(string message) =>
            MessageBox.Show(message, "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
