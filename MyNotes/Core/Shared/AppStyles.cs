using MyNotes.Core.Model;

namespace MyNotes.Core.Shared;
internal static class AppStyles
{
  public static int GetNoteViewMaxLength(BoardViewStyle viewStyle) => viewStyle switch
  {
    BoardViewStyle.Grid_200_0 => 46,
    BoardViewStyle.Grid_240_0 => 87,
    BoardViewStyle.Grid_280_0 => 140,
    BoardViewStyle.Grid_320_0 => 200,
    BoardViewStyle.Grid_360_0 => 270,
    BoardViewStyle.Grid_400_0 => 357,
    BoardViewStyle.Grid_440_0 => 448,

    BoardViewStyle.Grid_200_50 => 115,
    BoardViewStyle.Grid_240_50 => 174,
    BoardViewStyle.Grid_280_50 => 280,
    BoardViewStyle.Grid_320_50 => 360,
    BoardViewStyle.Grid_360_50 => 495,
    BoardViewStyle.Grid_400_50 => 663,
    BoardViewStyle.Grid_440_50 => 784,

    BoardViewStyle.Grid_200_100 => 161,
    BoardViewStyle.Grid_240_100 => 261,
    BoardViewStyle.Grid_280_100 => 420,
    BoardViewStyle.Grid_320_100 => 560,
    BoardViewStyle.Grid_360_100 => 720,
    BoardViewStyle.Grid_400_100 => 918,
    BoardViewStyle.Grid_440_100 => 1120,

    BoardViewStyle.Grid_200_150 => 230,
    BoardViewStyle.Grid_240_150 => 377,
    BoardViewStyle.Grid_280_150 => 525,
    BoardViewStyle.Grid_320_150 => 720,
    BoardViewStyle.Grid_360_150 => 945,
    BoardViewStyle.Grid_400_150 => 1173,
    BoardViewStyle.Grid_440_150 => 1456,

    BoardViewStyle.Grid_200_200 => 299,
    BoardViewStyle.Grid_240_200 => 464,
    BoardViewStyle.Grid_280_200 => 665,
    BoardViewStyle.Grid_320_200 => 880,
    BoardViewStyle.Grid_360_200 => 1170,
    BoardViewStyle.Grid_400_200 => 1479,
    BoardViewStyle.Grid_440_200 => 1792,

    _ => 100
  };
}
