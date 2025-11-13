namespace AdUserStatus.Services
{
    public sealed class ProcessingDialog : Form
    {
        private readonly Label _label;
        private readonly Label _rightLabel;
        private readonly ProgressBar _progress;
        private readonly Button _btnCancel;

        private string _baseMessage;
        private int? _lastCurrent;
        private int? _lastTotal;

        public event EventHandler? CancelRequested;

        public ProcessingDialog(string message = "Processing…")
        {
            Text = "Processing";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(14);

            _baseMessage = message;

            // --- Layout setup ---
            var layout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 3,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // left: message
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // right: counts

            // --- Left label (main message) ---
            _label = new Label
            {
                AutoSize = true,
                Text = message,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(0, 0, 0, 8)
            };

            // --- Right label (progress count) ---
            _rightLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                //ForeColor = SystemColors.GrayText,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 0, 8)
            };

            // --- Progress bar ---
            _progress = new ProgressBar
            {
                Dock = DockStyle.Top,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Width = 320
            };

            // --- Cancel button ---
            _btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            _btnCancel.Click += (_, __) => CancelRequested?.Invoke(this, EventArgs.Empty);

            // Add to layout
            layout.Controls.Add(_label, 0, 0);
            layout.Controls.Add(_rightLabel, 1, 0);
            layout.SetColumnSpan(_progress, 2);
            layout.Controls.Add(_progress, 0, 1);
            layout.SetColumnSpan(_btnCancel, 2);
            layout.Controls.Add(_btnCancel, 0, 2);

            Controls.Add(layout);
        }

        // === Update methods ===

        public void SetMessage(string msg)
        {
            if (InvokeRequired) BeginInvoke(new Action(() => _label.Text = msg));
            else _label.Text = msg;
            _baseMessage = msg;
        }

        public void SetProgress(int percent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetProgress(percent)));
                return;
            }

            if (percent <= 0)
                _progress.Style = ProgressBarStyle.Marquee;
            else
            {
                _progress.Style = ProgressBarStyle.Continuous;
                _progress.Value = Math.Min(100, Math.Max(0, percent));
            }

            if (percent >= 100)
            {
                Task.Delay(500).ContinueWith(_ =>
                {
                    if (!IsDisposed)
                        BeginInvoke(new Action(Close));
                });
            }
        }

        // Called when reporting (current, total)
        public void SetProgress(int current, int total)
        {
            _lastCurrent = current;
            _lastTotal = total;

            int percent = total > 0 ? (int)Math.Round((double)current / total * 100) : 0;
            SetProgress(percent);
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            string suffix = (_lastCurrent.HasValue && _lastTotal.HasValue)
                ? $"{_lastCurrent.Value} / {_lastTotal.Value}"
                : string.Empty;

            if (InvokeRequired)
                BeginInvoke(new Action(() => _rightLabel.Text = suffix));
            else
                _rightLabel.Text = suffix;
        }
    }
}
