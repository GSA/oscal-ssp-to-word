using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCALHelperClasses
{
    public class XmlTable
    {
        public List<Row> Rows;
        private const int LineLength = 60;
        private int _position, _endPosition;
#pragma warning disable CS0169 // The field 'XmlTable._xmlLenght' is never used
        private int _xmlLenght;
#pragma warning restore CS0169 // The field 'XmlTable._xmlLenght' is never used
        public int StartPosition
        {
            get
            {
                return _position;
            }
        }

        public int  EndPosition
        {
            get
            {
                return _endPosition;
            }
        }

        private int NberHeader;
        public TableDimension Dimension
        {
            get
            {
                return GetDimension();
            }
        }

        private const string Horizontal = "_____________________________________________________\n";
        private const string EqualLine = "=====================================================\n";


        public XmlTable(string xmlTable, int position)
        {
            _position = position;
            Rows = new List<Row>();
            xmlTable = xmlTable.Replace("\n", "");
            xmlTable = xmlTable.Replace("\a", "");
            xmlTable = xmlTable.Replace("| \n | ", "");
            while (xmlTable.IndexOf("<tr>") > 0)
            {
                int start = xmlTable.IndexOf("<tr>");
                int end = xmlTable.IndexOf("</tr>", start);
                var tem = xmlTable.Substring(start, end - start + 5);

                tem = tem.Replace("<tr>", "");
                tem = tem.Replace("</tr>", "");

                var row = new Row
                {
                    RowElements = GetRowElements(tem)
                };
                Rows.Add(row);

                xmlTable = xmlTable.Remove(start, end - start + 5);
            }

            if (Rows.Count > 0)
                NberHeader = Rows[0].RowElements.Count;
        }

        public XmlTable(string xmlTable, int position, int endPosition, string XMLNamespace)
        {
            _position = position;
            _endPosition = endPosition;
            Rows = new List<Row>();
            xmlTable = xmlTable.Replace("\n", "");
            xmlTable = xmlTable.Replace("\a", "");
            xmlTable = xmlTable.Replace("| \n | ", "");
            var rowTag = string.Format("<tr xmlns=\"{0}\">", XMLNamespace);
            while (xmlTable.IndexOf(rowTag) >= 0)
            {
                int start = xmlTable.IndexOf(rowTag);
                int end = xmlTable.IndexOf("</tr>", start);
                int realend = end - start + 5;
                var lenght = xmlTable.Length;
                var tem = xmlTable.Substring(start, realend);

                tem = tem.Replace(rowTag, "");
                tem = tem.Replace("</tr>", "");

                var row = new Row
                {
                    RowElements = GetRowElements(tem)
                };
                Rows.Add(row);

                xmlTable = xmlTable.Remove(start, realend);
            }

            if (Rows.Count > 0)
                NberHeader = Rows[0].RowElements.Count;
        }

        private int NumberOfLines(Row row, int divLength)
        {

            int a = 5;
            foreach (var elt in row.RowElements)
            {
                if (elt.Value.Length >= a)
                    a = elt.Value.Length;
            }

            if (a - 3 < divLength)
                return 1;


            int nber = (int)Math.Ceiling((decimal)(a - 3) / divLength);

            return nber;
        }

        private string BuildRowString(Row row)
        {
            string result = "";
            var matrix = new List<List<string>>();
            StringBuilder sb = new StringBuilder();
            var nber = row.RowElements.Count;
            if (nber == 0)
                return result;
            int divLength = (int)Math.Floor((decimal)LineLength / nber);
            var nberLines = NumberOfLines(row, divLength);
            for (int i = 0; i < row.RowElements.Count; i++)
            {
                int remainder;
                var value = row.RowElements[i].Value.Replace("\n", "");
                var lineExpressions = new List<string>();
                var lengthOfCharacters = Math.DivRem(row.RowElements[i].Value.Length, nberLines, out remainder);
                for (int j = 0; j < nberLines; j++)
                {
                    if (value.Length <= divLength)
                    {
                        var diff = divLength - value.Length;
                        var spacing = (int)Math.Floor((decimal)diff / 2);
                        var space = "";
                        for (int k = 0; k <= spacing; k++)
                        {
                            space += " ";
                        }
                        lineExpressions.Add(space + value + space);

                    }
                    else
                    {
                        var temp = value.Substring(0, divLength - 1);
                        var last = temp.LastIndexOf(" ");
                        var real = last > 0 ? value.Substring(0, last) : temp;


                        var diff = divLength - real.Length;
                        var spacing = (int)Math.Floor((decimal)diff / 2);
                        var space = "";
                        for (int k = 0; k <= spacing; k++)
                        {
                            space += " ";
                        }
                        var realtemp = space + real + space;

                        lineExpressions.Add(realtemp);




                        value = value.Length > realtemp.Length ? value.Remove(0, realtemp.Length) : "";
                    }
                }
                matrix.Add(lineExpressions);
            }
            var position = (int)Math.Floor((decimal)LineLength / row.RowElements.Count) + 3;
            for (int j = 0; j < nberLines; j++)
            {
                var line = "|";
                for (int i = 0; i < row.RowElements.Count; i++)
                {
                    var lf = matrix[i][j].Length;
                    var va = matrix[i][j];
                    line += matrix[i][j];

                    if (lf < position)
                    {
                        var space = "";
                        for (int k = 0; k <= position - lf; k++)
                        {
                            space += " ";
                        }
                        line = line + space + "|";
                    }
                    else
                    {
                        line = line + "|";
                    }
                    ////var pace = (i + 1) * position;

                    //line = line.Insert(pace, "|");
                }
                sb.AppendLine(line);

            }

            result = sb.ToString();
            return result;
        }

        public string BuildTableString()
        {
            string result;
            StringBuilder sb = new StringBuilder();
            sb.Append("<code>");
            sb.Append(Horizontal);
            foreach (var rw in Rows)
            {
                var rs = BuildRowString(rw);
                sb.Append(rs);
                sb.Append(Horizontal);
            }

            sb.Append("</code>");

            result = sb.ToString();

            return result;
        }

        List<RowElement> GetRowElements(string xmlRow)
        {
            var result = new List<RowElement>();
            while (xmlRow.IndexOf("<th>") >= 0)
            {
                int start = xmlRow.IndexOf("<th>");
                int end = xmlRow.IndexOf("</th>", start);
                var tem = xmlRow.Substring(start, end - start + 5);
                tem = tem.Replace("<th>", "");
                tem = tem.Replace("</th>", "");

                var elt = new RowElement
                {
                    Value = tem,
                    Type = RowElementType.Header
                };
                result.Add(elt);
                xmlRow = xmlRow.Remove(start, end - start + 5);

            }

            while (xmlRow.IndexOf("<td>") >= 0)
            {
                int start = xmlRow.IndexOf("<td>");
                int end = xmlRow.IndexOf("</td>", start);
                var tem = xmlRow.Substring(start, end - start + 5);
                tem = tem.Replace("<td>", "");
                tem = tem.Replace("</td>", "");
                var elt = new RowElement
                {
                    Value = tem,
                    Type = RowElementType.Division
                };
                result.Add(elt);
                xmlRow = xmlRow.Remove(start, end - start + 5);

            }

            return result;

        }

        private TableDimension GetDimension()
        {
            int rows = Rows.Count;
            int n = 0;
            foreach(var rw in Rows)
            {
                if(rw.RowElements.Count>=n)
                {
                    n = rw.RowElements.Count;
                }
            }
            var res = new TableDimension
            {
                NumberRows = rows,
                NumberColumns = n
            };

            return res;
        }


    }

    public enum RowElementType
    {
        Header,
        Division
    };

    public struct RowElement
    {
        public string Value;
        public RowElementType Type;


    }

    public struct Row
    {
        public List<RowElement> RowElements;
    }

    public struct TableDimension
    {
       public int NumberRows;
        public int NumberColumns;
    }
}
