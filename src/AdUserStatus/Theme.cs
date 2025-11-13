namespace AdUserStatus
{
    public static class Theme
    {
        public static readonly Color Primary = ColorTranslator.FromHtml("#005a9e");
        public static readonly Color Link = ColorTranslator.FromHtml("#0078d7");
        public static readonly Color Bg = ColorTranslator.FromHtml("#f7f9fb");
        public static readonly Color CardBg = Color.White;
        public static readonly Color Border = ColorTranslator.FromHtml("#d0d7de");
        public static readonly Color Text = ColorTranslator.FromHtml("#333333");

        public static void Apply(Form form)
        {
            form.BackColor = Bg;
            form.Font = new Font("Segoe UI", 10f);

            foreach (Control c in form.Controls)
                StyleControl(c);
        }

        private static void StyleControl(Control c)
        {
            switch (c)
            {
                case Button b:
                    StylePrimary(b);
                    break;
                case TabControl tc:
                    tc.BackColor = CardBg;
                    break;
                case DataGridView gv:
                    gv.BackgroundColor = CardBg;
                    gv.GridColor = Border;
                    break;
            }

            foreach (Control child in c.Controls)
                StyleControl(child);
        }

        public static void StylePrimary(Button b)
        {
            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Border;

            // Hover/pressed colors for enabled state
            b.FlatAppearance.MouseOverBackColor = Shade(Link, -0.10);
            b.FlatAppearance.MouseDownBackColor = Shade(Link, -0.20);

            b.BackColor = Link;
            b.ForeColor = Color.White;
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.Font = new Font("Segoe UI Semibold", 10f);
            b.Padding = new Padding(14, 8, 14, 8);

            // 🔹 Make disabled state readable (gray text on light bg)
            var disabledBg = ColorTranslator.FromHtml("#e6f2fb");  // very light blue
            var disabledBorder = ColorTranslator.FromHtml("#b9d6ef");

            void applyState()
            {
                if (b.Enabled)
                {
                    b.BackColor = Link;
                    b.FlatAppearance.BorderColor = Border;
                }
                else
                {
                    b.BackColor = disabledBg;                  // light so gray text is visible
                    b.FlatAppearance.BorderColor = disabledBorder;
                }
            }

            b.EnabledChanged += (_, __) => applyState();
            applyState(); // set initial state
        }

        public static void StyleSecondary(Button b)
        {
            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Border;

            b.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#f3f6fa");
            b.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml("#e8eef7");

            b.BackColor = Color.White;
            b.ForeColor = Text;
            b.TextAlign = ContentAlignment.MiddleCenter;

            b.Font = new Font("Segoe UI", 10f);
            b.Padding = new Padding(12, 8, 12, 8);
        }

        private static Color Shade(Color c, double pct)
        {
            int Clamp(int v) => Math.Max(0, Math.Min(255, v));
            return Color.FromArgb(
                Clamp((int)(c.R * (1 + pct))),
                Clamp((int)(c.G * (1 + pct))),
                Clamp((int)(c.B * (1 + pct))));
        }
    }
}
