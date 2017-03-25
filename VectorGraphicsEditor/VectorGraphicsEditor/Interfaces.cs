using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVGLib;

namespace Interfaces
{
    public struct Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public interface ICommand
    {
        //...
    }

    public abstract class Segment
    {
        public string Name { get; protected set; }
    }

    public class Line : Segment
    {
        public Point Beg { set; get; }
        public Point End { set; get; }
        public Line(Point beg, Point end)
        {
            Name = "Line";
            Beg = beg;
            End = end;
        }
    }

    public class Arc : Segment
    {
        public Point Center { set; get; }
        public double Rad { set; get; }
        public double Beg { set; get; }
        public double End { set; get; }
        public Arc(Point center, double rad, double beg, double end)
        {
            Name = "Arc";
            Center = center;
            Rad = rad;
            Beg = beg;
            End = end;
        }
    }

    public struct Color
    {
        public Color(int r, int g, int b, int a) : this()
        {
            R = r; G = g; B = b; A = a;
        }
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }
        public int A { get; private set; }
    }
    public class PointComparer : IEqualityComparer<Point>
    {

        public bool Equals(Point x, Point y)
        {
            return (x.X == y.X && x.Y == y.Y);
        }

        public int GetHashCode(Point obj)
        {
            return Math.Pow(obj.X, obj.Y).GetHashCode();
        }
    }
    public class Point
    {
        public Point() { X = 0;Y = 0; }
        public Point(double x, double y)
        {
            X = x; Y = y;
        }

        public Point(Point NeedCopy)
        {
            X = NeedCopy.X;
            Y = NeedCopy.Y;
        }
        public double X { get; private set; }
        public double Y { get; private set; }

        }
    public interface IPath
    {
        IEnumerable<Segment> Path { get; }
    }

    public interface ILineContainer
    {
        IEnumerable<Point> Path { get; }

    }

    public class Triangle
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }
        public Point GetCenter()
        {
            return( new Point((A.X + B.X + C.X) / 3, (A.Y + B.Y + C.Y) / 3));
        }
        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
    public interface ITransformation
    {
        Point TransformPoint(Point p);
    }
    public interface IFigure
    {
        /* заметим, что Paths хранит отрезки и дуги, так что может хранить несколько кривых,
         * а Lines - точки, так что для представления разных кривых понадобится массив контейнеров точек.*/
        string type { get; set; }

        Dictionary<string, object> Parameters
        {
            get;
            set;
        }
        // тип параметры фигуры. для прямоугольника две точки, для окружности точка и радиус...

        IPath Paths { get; }//geometries

        bool IsPointInner(Point point);
        void FillPaths(); // это чо вообще??
        Tuple<IEnumerable<Triangle>,IEnumerable<ILineContainer>> NewTriangulation(double eps);
        // стоит хранить предыдущий результат, что бы не перещитывать его, если функция вызывается с тем же eps

        bool Colored { get; set; }
        Color FillColor { get; set; }
        Color LineColor { get; set; }
        bool Is1D { get; }
        IFigure Clone(Dictionary<string, object> parms); // создать такую же фигуру с такими же параметрами
        IFigure Transform(ITransformation transform);
        /*Трансформация возвращает новую фигуру, трансформированную. однако очевидно что после этого
          фигура может сменить свой тип.*/
    }
}

namespace Logic
{
    using Interfaces;
    public interface ILogicForGUI
    {
        IEnumerable<IFigure> Figures { get; }
        Interfaces.Point ToScreen(Interfaces.Point xy);

        /*Тут будут функции заказа команды у фабрики и все такое*/
 
    }
}

namespace NGeometry
{
    using Interfaces;
    interface IGeometryForLogic
    {
        /*допилить фабрику*/

        IFigure Intersection(IFigure first, IFigure second);
        IFigure Union(IFigure first, IFigure second);
        IFigure Subtraction(IFigure first, IFigure second);
    }
}

namespace IO
{
    using Interfaces;
    //interface ISavePicture
    //{
    //    bool Save(string path, IEnumerable<IFigure> figures);
    //    IEnumerable<IFigure> Load(string path);

    //}
    //interface ISaveSettings
    //{
    //    /* понятия не имею в каком формате они будут */
    //    bool SaveSettings(string path, Parameter parametr /*запомненные новые фигуры, на пример*/);
    //    Parameter /*те же настройки */ LoadSettings(string path);
    //}

    public abstract class SVGShape {
        public Color fill { get; protected set; }
        public Color stroke { get; protected set; }
        public int w { get; protected set; } // stroke width
        public abstract SvgBasicShape ToSVGLibShape(SvgDoc doc);
    }

    public static class SVGIO {
        //static List<SVGShape> Import(string filename) {
            
        //}

        public static void export(List<SVGShape> shapes, string filename, int width, int height)
        {
            SvgDoc doc = new SvgDoc();
            SvgRoot root = doc.CreateNewDocument();
            root.Width = width.ToString() + "px";
            root.Height = height.ToString() + "px";
            foreach(var shape in shapes)
            {
                var figure = shape.ToSVGLibShape(doc);
                doc.AddElement(root, figure);
            }
            doc.SaveToFile(filename);
        }

        public static Tuple<List<SVGShape>, int, int> import(string filename)
        {
            List<SVGShape> shapes = new List<SVGShape>() {new SVGEllipse(new Point(50.0, 50.0), 20.0, 10.0, 
                new Color(100, 255, 56, 0),
                new Color(0, 0, 0, 0),
                2)};

            SvgDoc doc = new SvgDoc();
            doc.LoadFromFile(filename);
            SvgRoot root = doc.GetSvgRoot();
            int width, height = 1;
            Int32.TryParse(root.Width.ToString().Substring(0, root.Width.Length - 2), out width);
            Int32.TryParse(root.Height.ToString().Substring(0, root.Height.Length - 2), out height);
            Console.WriteLine(width);
            Console.WriteLine(height); 

            int i = 2;
            SvgElement el;
            while ((el = doc.GetSvgElement(i)) != null) {
                el = doc.GetSvgElement(i);
                Console.WriteLine(el.getElementName());
                ++i;
            }
            Console.WriteLine(i); 

            //foreach (var shape in shapes)
            //{
            //    var figure = shape.ToSVGLibShape(doc);
            //    doc.AddElement(root, figure);
            //}
            //doc.SaveToFile(filename);

            return Tuple.Create(shapes, width, height);
        }

    }

}
