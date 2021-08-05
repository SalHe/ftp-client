using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using NLog;
using NLog.Targets;

namespace FTPClient.GUI.NLog
{
    public sealed class MyRichTextBoxTarget : TargetWithLayout
    {
        private readonly RichTextBox _richTextBox;

        public MyRichTextBoxTarget(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            base.Write(logEvent);
            _richTextBox.Dispatcher.Invoke(() =>
            {
                Color color;
                if (logEvent.Level == LogLevel.Trace)
                    color = MaterialDesignColors.Recommended.GreySwatch.Grey500;
                else if (logEvent.Level == LogLevel.Debug)
                    color = MaterialDesignColors.Recommended.BlueGreySwatch.BlueGrey500;
                else if (logEvent.Level == LogLevel.Debug)
                    color = MaterialDesignColors.Recommended.BlueGreySwatch.BlueGrey500;
                else if (logEvent.Level == LogLevel.Info)
                    color = Color.FromRgb(0, 0, 0);
                else if (logEvent.Level == LogLevel.Warn)
                    color = MaterialDesignColors.Recommended.YellowSwatch.Yellow800;
                else if (logEvent.Level == LogLevel.Error)
                    color = MaterialDesignColors.Recommended.RedSwatch.Red300;
                else if (logEvent.Level == LogLevel.Fatal)
                    color = MaterialDesignColors.Recommended.RedSwatch.Red900;
                var textRange = new TextRange(_richTextBox.Document.ContentEnd, _richTextBox.Document.ContentEnd);
                textRange.Text = Layout.Render(logEvent) + "\r\n";
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
                _richTextBox.ScrollToEnd();
            });
        }
    }
}
