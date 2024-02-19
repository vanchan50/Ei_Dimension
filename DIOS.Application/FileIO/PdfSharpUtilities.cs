using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp;
using System.Diagnostics;
using System.Drawing;

namespace DIOS.Application.FileIO;

public class PdfSharpUtilities
{
  private double _topMargin = 0;
  private double _leftMargin = 0;
  private double _rightMargin = 0;
  private double _bottomMargin = 0;
  private double _cm;

  private PdfDocument _document = new();
  private PdfPage _page;
  private XGraphics _gfx;
  private XPen _pen = new(XColors.Black, 0.5);  //used for drawing lines (in a table)
  private string _outputPath;

  public PdfSharpUtilities(string argOutputpath, string title, bool argAddMarginGuides = false)
  {
    _outputPath = argOutputpath;
    _document.Info.Title = title;
    _document.Info.Author = "Dimension inc.";

    _page = _document.AddPage();
    _page.Size = PageSize.A4;

    //Define how much a cm is in document's units
    _cm = new Interpolation().linearInterpolation(0, 0, 27.9, _page.Height, 1);
    Console.WriteLine("1 cm:" + _cm);

    _gfx = XGraphics.FromPdfPage(_page);

    GlobalFontSettings.FontResolver = new FailsafeFontResolver();
    //NewFontResolver.Get();

    _topMargin = 1.25 * _cm;
    _leftMargin = 1.5 * _cm;
    _rightMargin = _page.Width - (1.5 * _cm);
    _bottomMargin = _page.Height - (1.25 * _cm);

    XFont font = new XFont(new Font("Segoe UI Semilight", 5).Name, 5);
    if (argAddMarginGuides)
    {
      _gfx.DrawString("+", font, XBrushes.Black, _rightMargin, _topMargin);
      _gfx.DrawString("+", font, XBrushes.Black, _leftMargin, _topMargin);
      _gfx.DrawString("+", font, XBrushes.Black, _rightMargin, _bottomMargin);
      _gfx.DrawString("+", font, XBrushes.Black, _leftMargin, _bottomMargin);
    }
  }

  public void drawTable(double initialPosX, double initialPosY, double width, double height, XFont font, XStringFormat format, List<string[]> contents)
  {
    int columns = contents[0].Length;
    int rows = contents.Count;

    double distanceBetweenRows = height / rows;
    double distanceBetweenColumns = width / columns;

    /*******************************************************************/
    // Draw the row lines
    /*******************************************************************/

    DPoint pointA = new DPoint(initialPosX, initialPosY);
    DPoint pointB = new DPoint(initialPosX + width, initialPosY);

    for (int i = 0; i <= rows; i++)
    {
      drawLine(pointA, pointB);

      pointA.y += distanceBetweenRows;
      pointB.y += distanceBetweenRows;
    }

    /*******************************************************************/
    // Draw the column lines
    /*******************************************************************/

    pointA = new DPoint(initialPosX, initialPosY);
    pointB = new DPoint(initialPosX, initialPosY + height);

    for (int i = 0; i <= columns; i++)
    {
      drawLine(pointA, pointB);

      pointA.x += distanceBetweenColumns;
      pointB.x += distanceBetweenColumns;
    }

    /*******************************************************************/
    // Insert text corresponding to each cell
    /*******************************************************************/

    pointA = new DPoint(initialPosX, initialPosY);

    foreach (string[] rowDataArray in contents)
    {
      foreach (string cellText in rowDataArray)
      {
        _gfx.DrawString(cellText, font, XBrushes.Black,
          new XRect(_leftMargin + (pointA.x * _cm),
            _topMargin + (pointA.y * _cm),
            distanceBetweenColumns * _cm,
            distanceBetweenRows * _cm),
          format);

        pointA.x += distanceBetweenColumns;
      }

      pointA.x = initialPosX;
      pointA.y += distanceBetweenRows;
    }
  }

  public void addText(string text, XFont font, DPoint xyStartingPosition)
  {
    _gfx.DrawString(text,
      font,
      XBrushes.Black,
      _leftMargin + (xyStartingPosition.x * _cm),
      _topMargin + (xyStartingPosition.y * _cm));
  }

  public void drawSquare(DPoint xyStartingPosition, double width, double height, XBrush xbrush)
  {
    Console.WriteLine("Drawing square starting at: " + xyStartingPosition.x + "," + xyStartingPosition.y + " width: " + width + " height: " + height);
    _gfx.DrawRectangle(xbrush,
      new XRect(_leftMargin + (xyStartingPosition.x * _cm),
        _topMargin + (xyStartingPosition.y * _cm),
        (width * _cm),
        (height * _cm)));
  }

  public void drawLine(DPoint fromXyPosition, DPoint toXyPosition)
  {
    _gfx.DrawLine(_pen,
      _leftMargin + (fromXyPosition.x * _cm),
      _topMargin + (fromXyPosition.y * _cm),
      _leftMargin + (toXyPosition.x * _cm),
      _topMargin + (toXyPosition.y * _cm));
  }

  public void Save()
  {
    _document.Save(this._outputPath);
  }

  public void Show()
  {
    Process.Start(new ProcessStartInfo(_outputPath) { UseShellExecute = true });
  }
}
