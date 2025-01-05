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

namespace GraphNavigator
{

    public partial class Form2 : Form
    {
        private PictureBox pictureBox;
        private Dictionary<string, (float x, float y)> nodePositions;
        public Form2()
        {
            InitializeComponent();
            pictureBox = new PictureBox()
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(pictureBox);

            var dotPath = "chart.dot";
            var imagePath = "chart.png";
            var svgPath = "chart.svg";

            GraphvizHelper.CreateDotFile(dotPath);
            GraphvizHelper.GenerateSvg(dotPath, svgPath);
            GraphvizHelper.GenerateImage(dotPath, imagePath);


            // SVGからノード情報を抽出
            nodePositions = SvgParser.ExtractNodePositions(svgPath);

            pictureBox.Image = System.Drawing.Image.FromFile(imagePath);
            pictureBox.MouseClick += PictureBox_MouseClick;
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {

            // クリック位置（PictureBox内の座標）
            var clickedX = e.X;
            var clickedY = e.Y;

            // SVGのviewBoxから画像の元サイズを取得
            float svgWidth = 0, svgHeight = 0;
            using (var reader = new System.Xml.XmlTextReader("chart.svg"))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "svg")
                    {
                        var viewBox = reader.GetAttribute("viewBox");
                        if (!string.IsNullOrEmpty(viewBox))
                        {
                            var parts = viewBox.Split(' ');
                            if (parts.Length == 4 &&
                                float.TryParse(parts[2], out svgWidth) &&
                                float.TryParse(parts[3], out svgHeight))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // PictureBoxの表示サイズを取得
            var displayWidth = pictureBox.ClientSize.Width;
            var displayHeight = pictureBox.ClientSize.Height;

            // クリック位置を元画像の座標系に変換
            var originalX = clickedX * (svgWidth / displayWidth);
            var originalY = (displayHeight - clickedY) * (svgHeight / displayHeight); // Y座標を反転j
            // ノードとの一致を判定
            foreach (var node in nodePositions)
            {
                var (nodeX, nodeY) = node.Value;

                // クリック位置がノードの近くであるか判定（半径10ピクセル以内）
                if (Math.Abs(originalX - nodeX) < 30 && Math.Abs(originalY - nodeY) < 30)
                {
                    MessageBox.Show($"Clicked node: {node.Key}");
                    return;
                }
            }

            MessageBox.Show("No node clicked.");
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PictureBoxがクリックされました！");
        }

    }
    public class GraphvizHelper
    {
        public static void CreateDotFile(string path)
        {
            var dotContent = @"
        digraph G {
            A [label=""Start""];
            B [label=""Process""];
            C [label=""End""];
            A -> B;
            B -> C;
        }";
            File.WriteAllText(path, dotContent);
        }

        public static void GenerateImage(string dotPath, string outputPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dot",
                    Arguments = $"-Tpng \"{dotPath}\" -o \"{outputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
        public static void GenerateSvg(string dotPath, string svgPath)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dot",
                    Arguments = $"-Tsvg \"{dotPath}\" -o \"{svgPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

    }
    public class SvgParser
    {
        public static Dictionary<string, (float x, float y)> ExtractNodePositions(string svgPath)
        {
            var nodePositions = new Dictionary<string, (float x, float y)>();
            var svgDocument = XDocument.Load(svgPath);

            // 名前空間の取得
            XNamespace ns = svgDocument.Root.Name.Namespace;

            // <g>タグ内のノード情報を取得
            foreach (var group in svgDocument.Descendants(ns + "g"))
            {
                var titleElement = group.Element(ns + "title");
                var ellipseElement = group.Element(ns + "ellipse");

                if (titleElement != null && ellipseElement != null)
                {
                    var nodeName = titleElement.Value;

                    if (float.TryParse(ellipseElement.Attribute("cx")?.Value, out float x) &&
                        float.TryParse(ellipseElement.Attribute("cy")?.Value, out float y))
                    {
                        // SVGの座標系に合わせてY座標を反転
                        nodePositions[nodeName] = (x, -y);
                    }
                }
            }

            return nodePositions;
        }
    }
}