using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubDomain.App
{
    public partial class Form1 : Form
    {
        //http://www.cnblogs.com/kongyiyun/archive/2011/08/01/2123459.html
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "(*.dll)|*.dll";
            string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
            foreach (var file in Directory.GetFiles(p, "*.dll", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult rs = openFileDialog1.ShowDialog();
            if (rs == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                string dllFileName = Path.GetFileName(path);
                txtFile.Text = path;

                treeView1.Nodes.Clear();
                string domainName = Guid.NewGuid().ToString();
                AppDomain myDomain = this.NewAppDomain(domainName);

                string domainBinPath = Path.Combine(myDomain.RelativeSearchPath, domainName);
                if (!Directory.Exists(domainBinPath))
                {
                    Directory.CreateDirectory(domainBinPath);
                }

                string currentDllFile = txtFile.Text;
                string currentDllDir = Path.GetDirectoryName(currentDllFile);
                string[] allDllFiles = Directory.GetFiles(currentDllDir, "*.dll");
                foreach (var file in allDllFiles)
                {
                    File.Copy(file, Path.Combine(domainBinPath, Path.GetFileName(file)));
                }

                var files = Directory.GetFiles(Path.Combine(myDomain.RelativeSearchPath, domainName), "*.dll");
                foreach (var file in files)
                {
                    var assembly = Assembly.LoadFrom(file);
                    assembly.GetReferencedAssemblies();

                    if (file.Contains(dllFileName))
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (!type.IsClass || type.IsEnum)
                            {
                                continue;
                            }
                            var q = from p in type.GetMembers()
                                    where p.MemberType == MemberTypes.Custom || p.MemberType == MemberTypes.Property
                                    select new TreeNode(p.Name) { Tag = p.GetType().Name };
                            TreeNode node = new TreeNode(type.Name, q.ToArray());
                            treeView1.Nodes.Add(node);
                        }
                    }
                }
            }
        }

        private AppDomain NewAppDomain(string domainName)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = "ApplicationLoader";
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
            setup.CachePath = setup.ApplicationBase;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.ApplicationBase;
            AppDomain.CurrentDomain.SetShadowCopyFiles();
            return AppDomain.CreateDomain(domainName, null, setup);
        }
    }
}
