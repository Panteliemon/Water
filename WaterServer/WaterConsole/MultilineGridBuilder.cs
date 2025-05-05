using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterConsole;

public class MultilineGridBuilder
{
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
        if ((RowsCount == 0) || (ColumnsCount == 0))
            return string.Empty;

        List<int> rowHeights = new(rows.Count);
        List<List<List<string>>> splitIntoLines = new();
        List<int> rowStarts = new(rows.Count);

        int currentRowStart = 0;
        int totalLines = 0; // without delimiters, of course
        for (int i = 0; i < rows.Count; i++)
        {
            splitIntoLines.Add(new List<List<string>>());

            int maxCellHeight = 0;
            for (int j = 0; j < columnsCount; j++)
            {
                List<string> lines = StringUtils.SplitIntoLines(rows[i][j]);
                splitIntoLines[i].Add(lines);

                if (lines.Count > maxCellHeight)
                    maxCellHeight = lines.Count;
            }

            rowHeights.Add(maxCellHeight);
            totalLines += maxCellHeight;

            rowStarts.Add(currentRowStart);
            currentRowStart += maxCellHeight;
        }

        GridBuilder innerGrid = new GridBuilder(totalLines, columnsCount);
        innerGrid.HeaderRowSeparator = HeaderRowSeparator;
        innerGrid.RowSeparator = RowSeparator;
        innerGrid.ColumnSeparator = ColumnSeparator;

        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                List<string> lines = splitIntoLines[i][j];
                for (int k = 0; k < lines.Count; k++)
                {
                    innerGrid[rowStarts[i] + k, j] = lines[k];
                }
            }
        }

        if (rows.Count > 1)
        {
            innerGrid.CustomShouldRenderHeaderRowSeparator = rowIndex => rowIndex == rowStarts[1];
            innerGrid.CustomShouldRenderRowSeparator = rowIndex => rowStarts.Contains(rowIndex);
        }
        else
        {
            innerGrid.CustomShouldRenderHeaderRowSeparator = rowIndex => false;
            innerGrid.CustomShouldRenderRowSeparator = rowIndex => false;
        }

        return innerGrid.ToString();
    }

    private void IncreaseColumnCount(int targetValue)
    {
        for (int i = 0; i < rows.Count; i++)
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
