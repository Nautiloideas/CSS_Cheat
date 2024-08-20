using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CSS_Cheat
{
    public partial class ModuleListForm : Form
    {

        public ModuleListForm()
        {
            InitializeComponent();
        }

        public void LoadModules(List<ModuleInfo> modules)
        {
            moduleListBox.Items.Clear();

            foreach (var module in modules)
            {
                string moduleInfo = $"{module.ModuleName} (0x{module.BaseAddress.ToString("X")})";
                moduleListBox.Items.Add(moduleInfo);
            }
        }

        public void ClearModules()
        {
            moduleListBox.Items.Clear();
        }
    }
}
