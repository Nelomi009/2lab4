using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MathFunction = System.Func<double, double>;

namespace _2lab2 {
  // Задание 3: поиск локального минимума методом золотого сечения
  public partial class task3 : Form {
    private readonly GoldenSectionSolver _solver = new GoldenSectionSolver();

    // Храним найденную точку для отображения на графике
    private double? _lastMinX = null;
    private double? _lastMinY = null;

    public task3() {
      InitializeComponent();
      SetupChart();
    }

    private void task3_Load(object sender, EventArgs e) {

    }

    // =========================================================================
    // БЛОК 1: НАСТРОЙКА ГРАФИКА (при старте)
    // =========================================================================
    private void SetupChart() {
      chart1.Series.Clear();
      chart1.ChartAreas.Clear();
      chart1.ChartAreas.Add("Area1");

      // Линия функции
      Series seriesFunc = new Series("Function") {
        ChartType = SeriesChartType.Line,
        BorderWidth = 3,
        Color = Color.SteelBlue,
        LegendText = "f(x)"
      };
      chart1.Series.Add(seriesFunc);

      // Найденная точка минимума
      Series seriesMin = new Series("MinPoint") {
        ChartType = SeriesChartType.Point,
        MarkerSize = 12,
        MarkerStyle = MarkerStyle.Circle,
        Color = Color.Red,
        LegendText = "Точка минимума"
      };
      chart1.Series.Add(seriesMin);

      // Оси пересекаются в (0, 0)
      chart1.ChartAreas[0].AxisX.Crossing = 0;
      chart1.ChartAreas[0].AxisY.Crossing = 0;
      chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Gainsboro;
      chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Gainsboro;
      chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.##";
      chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0.##";
    }

    // =========================================================================
    // БЛОК 2: РАССЧИТАТЬ (метод золотого сечения)
    // =========================================================================
    private void calculateToolStripMenuItem_Click(object sender, EventArgs e) {
      try {
        if (!TryParseDouble(txtA.Text, out double a))
          throw new Exception("Параметр 'a' должен быть вещественным числом.");
        if (!TryParseDouble(txtB.Text, out double b))
          throw new Exception("Параметр 'b' должен быть вещественным числом.");
        if (!TryParseDouble(txtE.Text, out double eps))
          throw new Exception("Параметр 'e' должен быть вещественным числом.");
        if (a >= b)
          throw new Exception("Интервал задан неверно: должно выполняться a < b.");
        if (eps <= 0)
          throw new Exception("Точность 'e' должна быть строго больше нуля.");

        string funcStr = txtFunc.Text;
        if (string.IsNullOrWhiteSpace(funcStr))
          throw new Exception("Введите формулу функции f(x).");

        // Вычисление методом золотого сечения
        var result = _solver.FindMinimum(funcStr, a, b, eps);

        // Сохраняем координаты для графика
        _lastMinX = result.X;
        _lastMinY = result.Y;

        // Вывод результата
        int precisionDigits = Math.Min(12, Math.Max(2, (int)Math.Ceiling(-Math.Log10(eps))));
        string format = "F" + (precisionDigits + 1);

        lblResult.Text = $"Минимум: x = {result.X.ToString(format)},  f(x) = {result.Y.ToString(format)}  (итераций: {result.Iterations})";

        PlotGraph(funcStr, a, b);
      }
      catch (Exception ex) {
        MessageBox.Show(ex.Message, "Ошибка расчёта",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    // =========================================================================
    // БЛОК 3: ПОСТРОИТЬ ГРАФИК
    // =========================================================================
    private void plotGraphToolStripMenuItem_Click(object sender, EventArgs e) {
      try {
        if (!TryParseDouble(txtA.Text, out double a))
          throw new Exception("Параметр 'a' должен быть вещественным числом.");
        if (!TryParseDouble(txtB.Text, out double b))
          throw new Exception("Параметр 'b' должен быть вещественным числом.");
        if (a >= b)
          throw new Exception("Интервал задан неверно: должно выполняться a < b.");

        string funcStr = txtFunc.Text;
        if (string.IsNullOrWhiteSpace(funcStr))
          throw new Exception("Введите формулу функции f(x).");

        PlotGraph(funcStr, a, b);
      }
      catch (Exception ex) {
        MessageBox.Show(ex.Message, "Ошибка отрисовки",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void PlotGraph(string funcStr, double a, double b) {
      chart1.Series["Function"].Points.Clear();
      chart1.Series["MinPoint"].Points.Clear();

      double delta = (b - a) * 0.15;
      double minX = a - delta;
      double maxX = b + delta;

      if (Math.Abs(maxX - minX) < 0.1) { minX -= 1; maxX += 1; }

      double step = (maxX - minX) / 500.0;
      const double yLimit = 1e6;

      for (double x = minX; x <= maxX; x += step) {
        try {
          double y = _solver.CalculateFunction(funcStr, x);
          if (double.IsNaN(y) || double.IsInfinity(y) || Math.Abs(y) > yLimit)
            continue;
          chart1.Series["Function"].Points.AddXY(x, y);
        }
        catch { }
      }

      if (_lastMinX.HasValue && _lastMinY.HasValue) {
        chart1.Series["MinPoint"].Points.AddXY(_lastMinX.Value, _lastMinY.Value);
      }

      chart1.ChartAreas[0].RecalculateAxesScale();
    }

    // =========================================================================
    // БЛОК 4: ОЧИСТИТЬ И ВЫХОД
    // =========================================================================
    private void clearToolStripMenuItem_Click(object sender, EventArgs e) {
      txtA.Clear();
      txtB.Clear();
      txtE.Clear();
      txtFunc.Clear();
      _lastMinX = null;
      _lastMinY = null;

      lblResult.Text = "Результат: —";
      chart1.Series["Function"].Points.Clear();
      chart1.Series["MinPoint"].Points.Clear();
      txtA.Focus();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Close();
    }

    private static bool TryParseDouble(string text, out double number) {
      number = 0;
      if (string.IsNullOrWhiteSpace(text)) return false;
      text = text.Trim().Replace(',', '.');
      if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return false;
      return !double.IsNaN(number) && !double.IsInfinity(number);
    }
  }

  // ---------- КЛАСС РЕШАТЕЛЯ МЕТОДОМ ЗОЛОТОГО СЕЧЕНИЯ ----------
  public class GoldenSectionResult {
    public double X { get; set; }
    public double Y { get; set; }
    public int Iterations { get; set; }
  }

  public class GoldenSectionSolver {
    private static readonly double Phi = (Math.Sqrt(5.0) - 1.0) / 2.0;

    public double CalculateFunction(string funcStr, double x) {
      MathFunction fn = FormulaParser.Compile(funcStr);
      return fn(x);
    }

    public GoldenSectionResult FindMinimum(string funcStr, double a, double b, double eps) {
      MathFunction fn;
      try {
        fn = FormulaParser.Compile(funcStr);
      }
      catch (FormatException ex) {
        throw new Exception("Ошибка в формуле: " + ex.Message);
      }

      double left = a;
      double right = b;

      double x1 = right - Phi * (right - left);
      double x2 = left + Phi * (right - left);

      double f1 = EvaluateOrThrow(fn, x1);
      double f2 = EvaluateOrThrow(fn, x2);

      int iterations = 0;
      const int maxIterations = 1000;

      while ((right - left) / 2.0 > eps) {
        iterations++;
        if (iterations > maxIterations) {
          throw new Exception("Не удалось достичь заданной точности: значение 'e' слишком мало для этого интервала.");
        }

        if (f1 < f2) {
          right = x2;
          x2 = x1;
          f2 = f1;
          x1 = right - Phi * (right - left);
          f1 = EvaluateOrThrow(fn, x1);
        }
        else {
          left = x1;
          x1 = x2;
          f1 = f2;
          x2 = left + Phi * (right - left);
          f2 = EvaluateOrThrow(fn, x2);
        }
      }

      double minX = (left + right) / 2.0;
      double minY = EvaluateOrThrow(fn, minX);

      return new GoldenSectionResult {
        X = minX,
        Y = minY,
        Iterations = iterations
      };
    }

    private static double EvaluateOrThrow(MathFunction fn, double x) {
      double y = fn(x);
      if (double.IsNaN(y) || double.IsInfinity(y))
        throw new Exception($"Функция не определена в точке x = {x:G6}. Выберите другой интервал.");
      return y;
    }
  }
}