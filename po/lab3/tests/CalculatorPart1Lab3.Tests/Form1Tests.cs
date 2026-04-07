using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using CalculatorPart1Lab3.Core;
using CalculatorPart1Lab3.WinForms;

namespace CalculatorPart1Lab3.Tests;

public class Form1Tests
{
    [Fact]
    public void Form1_KeyPressAcceptsHexDigits()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var display = GetField<TextBox>(form, "display");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            baseSelector.Value = 16;
            RaiseKeyPress(form, 'A');
            RaiseKeyPress(form, 'F');

            Assert.Equal("AF", display.Text);
        });
    }

    [Fact]
    public void Form1_Base2_DisablesHigherDigits()
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
    public void Form1_Base16_EnablesHexDigits()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            baseSelector.Value = 16;

            Assert.True(FindButton(form, "A").Enabled);
            Assert.True(FindButton(form, "F").Enabled);
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
    public void Form1_BaseSelector_ConvertsCurrentDisplay()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var display = GetField<TextBox>(form, "display");
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");

            foreach (var ch in "10.5")
            {
                RaiseKeyPress(form, ch);
            }

            baseSelector.Value = 2;

            Assert.Equal("1010.1", display.Text);
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
            InvokePrivate(form, "RunMemory", 0);

            Assert.Equal("M: ON", memoryIndicator.Text);
        });
    }

    [Fact]
    public void Form1_MemoryClear_ResetsIndicator()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var memoryIndicator = GetField<Label>(form, "memoryIndicator");

            RaiseKeyPress(form, '7');
            InvokePrivate(form, "RunMemory", 0);
            InvokePrivate(form, "RunMemory", 3);

            Assert.Equal("M: OFF", memoryIndicator.Text);
        });
    }

    [Fact]
    public void Form1_BaseMenuReflectsCurrentSelectorValue()
    {
        RunInSta(() =>
        {
            using var form = new TestableForm1();
            var baseSelector = GetField<NumericUpDown>(form, "baseSelector");
            var baseMenuItems = GetField<Dictionary<int, ToolStripMenuItem>>(form, "baseMenuItems");

            baseSelector.Value = 16;

            Assert.True(baseMenuItems[16].Checked);
            Assert.False(baseMenuItems[10].Checked);
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

    private static void InvokePrivate(Form1 form, string methodName, params object[] args)
    {
        var method = typeof(Form1).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(form, args);
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
        public bool InvokeProcessCmdKey(Keys keyData)
        {
            var message = new Message();
            return base.ProcessCmdKey(ref message, keyData);
        }
    }
}
