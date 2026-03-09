using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrailMeisterDb;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public partial class LapDetailControl : UserControl
    {
        public LapDetailControl()
        {
            InitializeComponent();
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Column != colLapLength) return;
            if (e.Row.Item is not DbLap lap) return;

            // e.EditingElement is a ContentPresenter for DataGridTemplateColumn,
            // so navigate the visual tree to find the TextBox inside it.
            var textBox = FindVisualChild<TextBox>(e.EditingElement);
            if (textBox == null) return;

            int? newLength = int.TryParse(textBox.Text.Trim(), out int parsed) ? parsed : null;
            lap.LapLength = newLength;
            new DbLapsTable().updateLapLength(lap.LapId, newLength);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
