using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterConsole;

/// <summary>
/// Organizes strings into "grid" for display in console via monospaced font.
/// </summary>
public class GridBuilder
{
    public const string DefaultColumnSeparator = " ";

    private List<List<string>> rows = new();
    private int columnsCount;

    public char? HeaderRowSeparator { get; set; }
    public char? RowSeparator { get; set; }
    public string ColumnSeparator { get; set; }

    public string this[int row, int column]
    {
        get
        {
            if ((row < 0) || (column < 0) || (row >= rows.Count) || (column >= columnsCount))
                throw new IndexOutOfRangeException();
            return rows[row][column];
        }
        set
        {
            if ((row < 0) || (column < 0))
                throw new IndexOutOfRangeException();
            IncreaseColumnCount(column + 1);
            IncreaseRowCount(row + 1);
            rows[row][column] = value;
        }
    }

    public int RowsCount => rows.Count;
    public int ColumnsCount => columnsCount;

    public override string ToString()
    {
        string columnSeparator = ColumnSeparator ?? DefaultColumnSeparator;

        // Find maximum lengths of strings by columns
        List<int> maxLengths = new List<int>(columnsCount);
        for (int j = 0; j < columnsCount; j++)
            maxLengths.Add(0);

        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                int length = rows[i][j]?.Length ?? 0;
                if (length > maxLengths[j])
                    maxLengths[j] = length;
            }
        }

        int rowLength = 0;
        for (int j = 0; j < columnsCount; j++)
        {
            if (j > 0)
                rowLength += columnSeparator.Length;
            rowLength += maxLengths[j];
        }

        // Write
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < rows.Count; i++)
        {
            // Row separator
            if (i > 0)
            {
                sb.AppendLine();

                if (i == 1)
                {
                    if (RowSeparator.HasValue || HeaderRowSeparator.HasValue)
                    {
                        char headerSeparator = HeaderRowSeparator ?? RowSeparator.Value;
                        sb.Append(headerSeparator, rowLength);
                        sb.AppendLine();
                    }
                }
                else
                {
                    if (RowSeparator.HasValue)
                    {
                        sb.Append(RowSeparator.Value, rowLength);
                        sb.AppendLine();
                    }
                }
            }

            // Cells within row
            for (int j = 0; j < columnsCount; j++)
            {
                if (j > 0)
                {
                    sb.Append(columnSeparator);
                }

                string cellStr = rows[i][j] ?? string.Empty;
                sb.Append(cellStr);
                for (int k = cellStr.Length; k < maxLengths[j]; k++)
                    sb.Append(' ');
            }
        }

        return sb.ToString();
    }

    private void IncreaseColumnCount(int targetValue)
    {
        for (int i=0; i<rows.Count; i++)
        {
            IncreaseListCount(rows[i], targetValue);
        }

        if (targetValue > columnsCount)
            columnsCount = targetValue;
    }

    private void IncreaseRowCount(int targetValue)
    {
        for (int i = rows.Count; i < targetValue; i++)
        {
            List<string> newRow = new();
            IncreaseListCount(newRow, columnsCount);
            rows.Add(newRow);
        }
    }

    private static void IncreaseListCount(List<string> list, int targetValue)
    {
        for (int i = list.Count; i < targetValue; i++)
        {
            list.Add(null);
        }
    }
}
