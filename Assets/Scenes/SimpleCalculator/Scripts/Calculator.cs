using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Calculator : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI InputText;

    private string _expression = "";     // Full expression e.g. "2222+55"
    private string _currentNumber = "";  // Current number being typed
    private bool _justEvaluated = false; // True if last action was "="

    private void Awake()
    {
        if (InputText == null)
        {
            InputText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (InputText == null)
        {
            Debug.LogError("Calculator: No TextMeshProUGUI assigned!");
        }
    }

    private void Start()
    {
        if (InputText != null)
            InputText.SetText("0");
    }

    // Called when number button pressed
    public void ClickNumber(int val)
    {
        Debug.Log($"Number clicked: {val}");

        if (_justEvaluated)
        {
            _expression = "";
            _currentNumber = "";
            _justEvaluated = false;
        }

        _currentNumber += val.ToString();
        _expression += val.ToString();
        InputText.SetText(_expression);
    }

    // Called when operator pressed (+ - * /)
    public void ClickOperation(string op)
    {
        Debug.Log($"Operator clicked: {op}");

        if (string.IsNullOrEmpty(op)) return;
        _justEvaluated = false;

        if (string.IsNullOrEmpty(_expression))
        {
            if (op == "-")  // allow negative numbers at start
            {
                _expression = "-";
                _currentNumber = "-";
                InputText.SetText(_expression);
            }
            return;
        }

        // Replace operator if last char is already an operator
        if (_expression.Length > 0 && IsOperator(_expression[^1]))
        {
            _expression = _expression[..^1] + op;
        }
        else
        {
            _expression += op;
        }

        _currentNumber = "";
        InputText.SetText(_expression);
    }

    // Decimal point
    public void ClickPeriod()
    {
        Debug.Log("Decimal point clicked");

        if (_justEvaluated)
        {
            _expression = "";
            _currentNumber = "";
            _justEvaluated = false;
        }

        if (string.IsNullOrEmpty(_currentNumber))
        {
            _currentNumber = "0.";
            _expression += "0.";
        }
        else if (!_currentNumber.Contains("."))
        {
            _currentNumber += ".";
            _expression += ".";
        }

        InputText.SetText(_expression);
    }

    // Equal button
    public void ClickEqual()
    {
        Debug.Log("Equal clicked");

        if (string.IsNullOrWhiteSpace(_expression))
        {
            InputText.SetText("0");
            return;
        }

        // Remove invalid ending chars
        if (IsOperator(_expression[^1]) || _expression[^1] == '.')
            _expression = _expression[..^1];

        try
        {
            double result = EvaluateExpression(_expression);
            string formatted = FormatResult(result);

            InputText.SetText(formatted);

            _expression = formatted;
            _currentNumber = formatted;
            _justEvaluated = true;
        }
        catch (DivideByZeroException)
        {
            InputText.SetText("Cannot divide by 0");
            _expression = "";
            _currentNumber = "";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Evaluation error: {ex.Message}");
            InputText.SetText("Error");
            _expression = "";
            _currentNumber = "";
        }
    }

    // AC button
    public void ClickAC()
    {
        Debug.Log("AC clicked");
        _expression = "";
        _currentNumber = "";
        _justEvaluated = false;
        InputText.SetText(" ");
    }

    // Delete last char
    public void ClickDelete()
    {
        Debug.Log("Delete clicked");

        if (string.IsNullOrEmpty(_expression))
        {
            InputText.SetText(" ");
            return;
        }

        _expression = _expression[..^1];
        int lastOp = LastOperatorIndex(_expression);
        _currentNumber = lastOp >= 0 ? _expression[(lastOp + 1)..] : _expression;

        InputText.SetText(!string.IsNullOrEmpty(_expression) ? _expression : "0");
        _justEvaluated = false;
    }

    // --- New Features ---

    // Percentage button
    public void ClickPercent()
{
    Debug.Log("% clicked");

    if (string.IsNullOrEmpty(_currentNumber)) return;

    try
    {
        double number = double.Parse(_currentNumber, System.Globalization.CultureInfo.InvariantCulture);
        number /= 100.0; // convert to percentage

        string formatted = FormatResult(number);

        // Replace current number in expression
        int lastOp = LastOperatorIndex(_expression);
        if (lastOp >= 0)
        {
            _expression = _expression.Substring(0, lastOp + 1) + formatted + "%";
        }
        else
        {
            _expression = formatted + "%";
        }

        _currentNumber = formatted;
        InputText.SetText(_expression);
    }
    catch (Exception ex)
    {
        Debug.LogError($"% error: {ex.Message}");
    }
}

         

    // Double zero button
    public void ClickDoubleZero()
    {
        Debug.Log("00 clicked");

        if (_justEvaluated)
        {
            _expression = "";
            _currentNumber = "";
            _justEvaluated = false;
        }

        if (string.IsNullOrEmpty(_currentNumber))
        {
            // Prevent leading "00"
            _currentNumber = "00";
            _expression += "00";
        }
        else
        {
            _currentNumber += "00";
            _expression += "00";
        }

        InputText.SetText(_expression);
    }

    // --- Helpers ---

    private static bool IsOperator(char c) => c == '+' || c == '-' || c == '*' || c == '/';

    private static int LastOperatorIndex(string s)
    {
        for (int i = s.Length - 1; i >= 0; i--)
            if (IsOperator(s[i])) return i;
        return -1;
    }

    private static double EvaluateExpression(string expr)
    {
        var tokens = Tokenize(expr);
        var rpn = ToRPN(tokens);
        return EvaluateRPN(rpn);
    }

    private static List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < expr.Length)
        {
            char c = expr[i];

            // Number (including unary minus)
            if (char.IsDigit(c) || c == '.' || (c == '-' && (i == 0 || IsOperator(expr[i - 1]))))
            {
                int start = i;
                if (c == '-') i++;
                bool hasDot = false;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                {
                    if (expr[i] == '.')
                    {
                        if (hasDot) break;
                        hasDot = true;
                    }
                    i++;
                }
                tokens.Add(expr[start..i]);
            }
            else if (IsOperator(c))
            {
                tokens.Add(c.ToString());
                i++;
            }
            else i++;
        }
        return tokens;
    }

    private static List<string> ToRPN(List<string> tokens)
    {
        var output = new List<string>();
        var ops = new Stack<string>();

        foreach (var t in tokens)
        {
            if (t.Length == 1 && IsOperator(t[0]))
            {
                while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(t))
                    output.Add(ops.Pop());
                ops.Push(t);
            }
            else output.Add(t);
        }

        while (ops.Count > 0) output.Add(ops.Pop());
        return output;
    }

    private static double EvaluateRPN(List<string> rpn)
    {
        var stack = new Stack<double>();

        foreach (var t in rpn)
        {
            if (t.Length == 1 && IsOperator(t[0]))
            {
                double b = stack.Pop();
                double a = stack.Pop();
                switch (t)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/":
                        if (Math.Abs(b) < double.Epsilon) throw new DivideByZeroException();
                        stack.Push(a / b);
                        break;
                }
            }
            else
            {
                stack.Push(double.Parse(t, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        return stack.Pop();
    }

    private static int Precedence(string op) => (op == "*" || op == "/") ? 2 : 1;

    private static string FormatResult(double val)
    {
        return val.ToString("0.###############", System.Globalization.CultureInfo.InvariantCulture);
    }
}
