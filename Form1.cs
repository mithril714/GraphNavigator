using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GraphNavigator
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Form2のインスタンスを生成
            Form2 form2 = new Form2();
            // form2を表示
            form2.Show();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            string folderPath = textBox1.Text;
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("指定されたフォルダが存在しません。");
                return;
            }

            // 指定フォルダ内のすべての .txt ファイルを取得
            string[] fileNames = Directory.GetFiles(folderPath, "*.txt");

            // ファイル間の関係を格納する辞書
            Dictionary<string, List<string>> relationships = new Dictionary<string, List<string>>();

            foreach (var fileName in fileNames)
            {
                // ファイルの中身を読み込む
                if (File.Exists(fileName))
                {
                    var lines = File.ReadAllLines(fileName);
                    var targets = lines
                        .Where(line => line.StartsWith("MENU"))
                        .Select(line => line.Split(' ')[1].Trim())
                        .ToList();

                    // 関係性を記録
                    relationships[fileName] = targets;
                }
                else
                {
                    Console.WriteLine($"File {fileName} does not exist.");
                }
            }

            // DOT 言語形式で出力
            string dotContent = GenerateDotContent(relationships);

            // DOT ファイルを保存
            File.WriteAllText("graph.dot", dotContent);
            Console.WriteLine("Graphviz DOT file generated: graph.dot");

            GraphvizHelper.GenerateImage("graph.dot", "graph.png");
        }
        static string GenerateDotContent(Dictionary<string, List<string>> relationships)
        {
            var dot = "digraph G {\n";

            foreach (var kvp in relationships)
            {
                string source = Path.GetFileName(kvp.Key); // ファイル名
                foreach (var target in kvp.Value)
                {
                    string targetFile = Path.GetFileName(target);
                    dot += $"    \"{source}\" -> \"{targetFile}\";\n";
                }
            }

            dot += "}";
            return dot;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "フォルダを選択してください。";
                dlg.FileName = "SelectFolder";
                dlg.Filter = "Folder|.";
                dlg.CheckFileExists = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                {

//                    MessageBox.Show(System.IO.Path.GetDirectoryName(dlg.FileName));
                    textBox1.Text = System.IO.Path.GetDirectoryName(dlg.FileName);
                }
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}