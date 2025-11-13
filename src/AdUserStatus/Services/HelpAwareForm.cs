namespace AdUserStatus.Services
{
    public class HelpAwareForm : Form
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                var topic = ActiveControl?.Tag as string ?? "index.html";
                var hf = new HelpForm(topic) { StartPosition = FormStartPosition.CenterParent };
                hf.Show(this);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
