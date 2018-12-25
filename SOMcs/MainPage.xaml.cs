using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace SOMcs
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SOM som;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string map_size = cbx_map_size.SelectedValue.ToString();
            //int size = Convert.ToInt32(map_size);
           som = new SOM();
            canvas.Height = som.Height * 32;
            canvas.Width = som.Width * 32;
            double[] r = new double[3];
            r[0] = 1.0;
            r[1] = 0;
            r[2] = 0;
            SOM.Node rNode = new SOM.Node(3, vector: r, label:"R");

            double[] g = new double[3];
            g[0] = 0;
            g[1] = 1.0;
            g[2] = 0;
            SOM.Node gNode = new SOM.Node(3, vector: g, label: "G");

            double[] b = new double[3];
            b[0] = 0;
            b[1] = 0;
            b[2] = 1.0;
            SOM.Node bNode = new SOM.Node(3, vector: b, label: "B");

            double[] rg = new double[3];
            rg[0] = 1.0;
            rg[1] = 1.0;
            rg[2] = 0.0;
            SOM.Node rgNode = new SOM.Node(3, vector: rg, label: "RG");

            double[] rb = new double[3];
            rb[0] = 1.0;
            rb[1] = 0.0;
            rb[2] = 1.0;
            SOM.Node rbNode = new SOM.Node(3, vector: rb, label: "RB");

            double[] gb = new double[3];
            gb[0] = 0.0;
            gb[1] = 1.0;
            gb[2] = 1.0;
            SOM.Node gbNode = new SOM.Node(3, vector: gb, label: "GB");

            List<SOM.Node> nodes = new List<SOM.Node>();
            nodes.Add(rNode);
            nodes.Add(gNode);
            nodes.Add(bNode);
            nodes.Add(rgNode);
            nodes.Add(rbNode);
            nodes.Add(gbNode);
            som.Learning(nodes);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            byte r, g, b;

            int radius = 10;
            for (int h = 0; h < som.Height; h++)
            {
                for (int w = 0; w < som.Width; w++)
                {
                    if ((h + w) % 2 == 0)
                    {
                        r = (byte)(som.Nodes[h, w].WeightVector[0] * 255);
                        g = (byte)(som.Nodes[h, w].WeightVector[1] * 255);
                        b = (byte)(som.Nodes[h, w].WeightVector[2] * 255);
                        Polygon p = CreateHex(radius, Color.FromArgb(255, r, g, b));
                        p.Tag = som.Nodes[h, w];
                        canvas.Children.Add(p);
                        p.PointerPressed += HexClick;
                        if (h % 2 == 0)
                        {
                            //Canvas.SetLeft(p, 3 * radius * w);
                        }
                        else
                        {
                            //Canvas.SetLeft(p, radius * 3 * w + 1.5 * radius);
                        }
                        Canvas.SetLeft(p, 1.5 * radius * w);
                        Canvas.SetTop(p, radius * h);
                    }
                }
            }
        }

        private async Task UpdateCanvas(SOM.Node[,] nodes, int height, int width)
        {
            byte r, g, b;

            int radius = 30;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        if ((h + w) % 2 == 0)
                        {
                            r = (byte)(nodes[h, w].WeightVector[0] * 255);
                            g = (byte)(nodes[h, w].WeightVector[1] * 255);
                            b = (byte)(nodes[h, w].WeightVector[2] * 255);
                            Polygon p = CreateHex(radius, Color.FromArgb(255, r, g, b));
                            p.Tag = nodes[h, w].Coordinate;
                            canvas.Children.Add(p);
                            p.PointerPressed += HexClick;
                            if (h % 2 == 0)
                            {
                                //Canvas.SetLeft(p, 3 * radius * w);
                            }
                            else
                            {
                                //Canvas.SetLeft(p, radius * 3 * w + 1.5 * radius);
                            }
                            Canvas.SetLeft(p, 1.5 * radius * w);
                            Canvas.SetTop(p, radius * h);
                        }
                    }
                }
            });
        }

        private Polygon CreateHex(int radius, Color color)
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(0.5 * radius, Math.Sqrt(3) / 2 * radius));
            myPointCollection.Add(new Point(-0.5 * radius, Math.Sqrt(3) / 2 * radius ));
            myPointCollection.Add(new Point(-1 * radius, 0 * radius ));
            myPointCollection.Add(new Point(-0.5 * radius, -Math.Sqrt(3) / 2 * radius));
            myPointCollection.Add(new Point(0.5 * radius, -Math.Sqrt(3) / 2 * radius ));
            myPointCollection.Add(new Point(1 * radius, 0 * radius));
            Polygon myPolygon = new Polygon();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush(color);
            SolidColorBrush brush = new SolidColorBrush(Colors.White);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = mySolidColorBrush;
            myPolygon.Width = radius * 2;
            myPolygon.Height = radius * 2;
            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = brush;
            myPolygon.StrokeThickness = 2;            
            return myPolygon;
        }

        private void HexClick(object sender, PointerRoutedEventArgs e)
        {
            Polygon p = sender as Polygon;
            SOM.Node buf = p.Tag as SOM.Node;
            Debug.WriteLine($"{buf.Coordinate}, label = {buf.Label}, vector = {buf.WeightVector[0]}, {buf.WeightVector[1]}, {buf.WeightVector[2]}");
        }
    }
}
