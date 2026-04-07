using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using UniversalCalculatorLab4.Core;
using UniversalCalculatorLab4.WinForms;

namespace UniversalCalculatorLab4.Tests;

public class Form1Tests
{
    [Fact]
    public void Form1_ComplexMode_LeavesBaseSelectorEnabled()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            modeSelector.SelectedIndex = 2;

            Assert.True(baseSelector.Enabled);
        });
    }

    [Fact]
    public void Form1_ComplexMode_KeyPressAcceptsHexDigits()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");
            var display = GetField<TextBox>(form, "display");

            modeSelector.SelectedIndex = 2;
            baseSelector.Value = 16;

            RaiseKeyPress(form, 'A');
            RaiseKeyPress(form, ';');
            RaiseKeyPress(form, 'F');

            Assert.Equal("A;F", display.Text);
        });
    }

    [Fact]
    public void Form1_ComplexMode_KeyPressSupportsFractionalNegativeImaginaryPart()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");
            var display = GetField<TextBox>(form, "display");

            modeSelector.SelectedIndex = 2;
            baseSelector.Value = 16;

            foreach (var ch in "A.5;-B.C")
            {
                RaiseKeyPress(form, ch);
            }

            Assert.Equal("A.5;-B.C", display.Text);
        });
    }

    [Fact]
    public void Form1_FractionMode_DisablesBaseSelector_AndHidesComplexButtons()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");
            var complexDecimalButton = GetField<Button>(form, "complexDecimalButton");
            var complexImaginarySignButton = GetField<Button>(form, "complexImaginarySignButton");
            var complexRealSignButton = GetField<Button>(form, "complexRealSignButton");

            modeSelector.SelectedIndex = 1;

            Assert.False(baseSelector.Enabled);
            Assert.False(complexDecimalButton.Visible);
            Assert.False(complexImaginarySignButton.Visible);
            Assert.False(complexRealSignButton.Visible);
        });
    }

    [Fact]
    public void Form1_ComplexMode_ShowsComplexButtons()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var complexDecimalButton = GetField<Button>(form, "complexDecimalButton");
            var complexImaginarySignButton = GetField<Button>(form, "complexImaginarySignButton");
            var complexRealSignButton = GetField<Button>(form, "complexRealSignButton");

            form.Show();
            Application.DoEvents();
            modeSelector.SelectedIndex = 2;
            Application.DoEvents();

            Assert.True(complexDecimalButton.Visible);
            Assert.True(complexImaginarySignButton.Visible);
            Assert.True(complexRealSignButton.Visible);
        });
    }

    [Fact]
    public void Form1_Enter_ComputesExpression()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var display = GetField<TextBox>(form, "display");

            RaiseKeyPress(form, '2');
            RaiseKeyPress(form, '+');
            RaiseKeyPress(form, '3');

            var handled = form.InvokeProcessCmdKey(Keys.Enter);

            Assert.True(handled);
            Assert.Equal("5", display.Text);
        });
    }

    [Fact]
    public void Form1_Escape_ResetsDisplay()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var display = GetField<TextBox>(form, "display");

            RaiseKeyPress(form, '7');
            var handled = form.InvokeProcessCmdKey(Keys.Escape);

            Assert.True(handled);
            Assert.Equal("0", display.Text);
        });
    }

    [Fact]
    public void Form1_NumPadInput_AndAddKey_Work()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var display = GetField<TextBox>(form, "display");

            RaiseKeyDown(form, Keys.NumPad2);
            RaiseKeyDown(form, Keys.Add);
            RaiseKeyDown(form, Keys.NumPad3);
            form.InvokeProcessCmdKey(Keys.Enter);

            Assert.Equal("5", display.Text);
        });
    }

    [Fact]
    public void Form1_MemoryStore_UpdatesIndicator()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var memoryIndicator = GetField<Label>(form, "memoryIndicator");

            RaiseKeyPress(form, '7');
            InvokeRunCommand(form, CalculatorCommand.MemoryStore);

            Assert.Equal("M: ON", memoryIndicator.Text);
        });
    }

    [Fact]
    public void Form1_PNumberBase2_DisablesHexButtons()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            baseSelector.Value = 2;

            Assert.True(FindButton(form, "1").Enabled);
            Assert.False(FindButton(form, "2").Enabled);
            Assert.False(FindButton(form, "A").Enabled);
        });
    }

    [Fact]
    public void Form1_ComplexBase16_EnablesHexButtons()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var modeSelector = GetField<ComboBox>(form, "modeSelector");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            modeSelector.SelectedIndex = 2;
            baseSelector.Value = 16;

            Assert.True(FindButton(form, "A").Enabled);
            Assert.True(FindButton(form, "F").Enabled);
        });
    }

    private static void RaiseKeyPress(Form1 form, char keyChar)
    {
        var method = typeof(Form1).GetMethod("OnFormKeyPress", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(form, [form, new KeyPressEventArgs(keyChar)]);
    }

    private static void RaiseKeyDown(Form1 form, Keys keyData)
    {
        var method = typeof(Form1).GetMethod("OnFormKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(form, [form, new KeyEventArgs(keyData)]);
    }

    private static T GetField<T>(Form1 form, string name) where T : class
    {
        var field = typeof(Form1).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        return (T)(field!.GetValue(form) ?? throw new InvalidOperationException($"Поле {name} не найдено."));
    }

    private static Button FindButton(Form form, string text)
    {
        foreach (var control in GetAllControls(form))
        {
            if (control is Button button && button.Text == text)
            {
                return button;
            }
        }

        throw new InvalidOperationException($"Кнопка {text} не найдена.");
    }

    private static void InvokeRunCommand(Form1 form, CalculatorCommand command)
    {
        var method = typeof(Form1).GetMethod("RunCommand", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(form, [command]);
    }

    private static IEnumerable<Control> GetAllControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;

            foreach (var descendant in GetAllControls(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private sealed class TestableForm1 : Form1
    {
        public TestableForm1()
        {
            Show();
            Application.DoEvents();
        }

        public bool InvokeProcessCmdKey(Keys keyData)
        {
            var message = new Message();
            return base.ProcessCmdKey(ref message, keyData);
        }
    }
}
