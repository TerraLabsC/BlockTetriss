using UnityEngine;

public static class Polyominus
{
    private static readonly int[][,] polyominus = new int[][,]
    {
        new int[,]
        {
            { 0, 0, 1, },
            { 0, 0, 1, },
            { 1, 1, 1, }
        }
    };

    static Polyominus()
    {
        foreach (var i in polyominus) 
        { 
            ReserveRows(i);
        }
    }

    public static int[,] Get(int index) => polyominus[index];

    public static int Length => polyominus.Length;

    private static void ReserveRows(int[,] polyomino)
    {
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);

        for (var r = 0; r < polyominoRows/2; ++r)
        {
            var topRow = r;
            var bottomRow = polyominoRows - 1 - r;

            for (var c = 0; c < poluominoColumns; ++c)
            {
                var tmp = polyomino[topRow, c];
                polyomino[topRow, c] = polyomino[bottomRow, c];
                polyomino[bottomRow, c] = tmp;
            }
        }
    }
}
