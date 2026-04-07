namespace UniversalCalculatorLab4.Core;

public sealed class ComplexEditor : AEditor
{
    private readonly PNumberEditor realEditor;
    private readonly PNumberEditor imaginaryEditor;
    private string realText = "0";
    private string imaginaryText = string.Empty;
    private bool hasImaginaryPart;
    private bool editingImaginary;

    public ComplexEditor(int numberBase = 10)
    {
        realEditor = new PNumberEditor(numberBase);
        imaginaryEditor = new PNumberEditor(numberBase);
        SyncValue();
    }

    public int NumberBase => realEditor.NumberBase;

    public void SetBase(int numberBase)
    {
        realEditor.SetBase(numberBase);
        imaginaryEditor.SetBase(numberBase);
        Clear();
    }

    public override bool IsZero()
    {
        return IsZeroComponent(realText) && (!hasImaginaryPart || IsZeroComponent(imaginaryText));
    }

    public override string AddDigit(int digit)
    {
        if (digit < 0 || digit >= NumberBase)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), $"Цифра должна быть в диапазоне 0..{NumberBase - 1}.");
        }

        ApplyToActiveComponent(editor => editor.AddDigit(digit));
        return value;
    }

    public override string AddZero()
    {
        ApplyToActiveComponent(editor => editor.AddZero());
        return value;
    }

    public override string AddSeparator()
    {
        if (!hasImaginaryPart)
        {
            hasImaginaryPart = true;
            imaginaryText = string.Empty;
            editingImaginary = true;
            SyncValue();
        }
        else
        {
            editingImaginary = true;
        }

        return value;
    }

    public override string ToggleSign()
    {
        realText = ToggleComponentSign(realText, realEditor, allowEmpty: false);
        if (hasImaginaryPart && imaginaryText.Length > 0)
        {
            imaginaryText = ToggleComponentSign(imaginaryText, imaginaryEditor, allowEmpty: true);
        }

        SyncValue();
        return value;
    }

    public override string Backspace()
    {
        if (hasImaginaryPart)
        {
            if (imaginaryText.Length > 0)
            {
                imaginaryText = BackspaceComponent(imaginaryText, allowEmpty: true);
            }
            else
            {
                hasImaginaryPart = false;
                editingImaginary = false;
            }

            SyncValue();
            return value;
        }

        realText = BackspaceComponent(realText, allowEmpty: false);
        SyncValue();
        return value;
    }

    public override string Clear()
    {
        realText = "0";
        imaginaryText = string.Empty;
        hasImaginaryPart = false;
        editingImaginary = false;
        SyncValue();
        return value;
    }

    public override void SetValue(string text)
    {
        var source = string.IsNullOrWhiteSpace(text) ? "0" : text.Trim();
        value = source;
        TrySeedEditorsFromText(source);
    }

    public override string Edit(int command)
    {
        return command switch
        {
            0 => AddZero(),
            >= 1 and <= 15 => AddDigit(command),
            16 => AddSeparator(),
            17 => Backspace(),
            18 => Clear(),
            19 => AddDecimalSeparator(),
            20 => ToggleSign(),
            21 => ToggleImaginarySign(),
            22 => ToggleRealSign(),
            _ => value
        };
    }

    public string AddDecimalSeparator()
    {
        ApplyToActiveComponent(editor => editor.AddSeparator());
        return value;
    }

    public string ToggleImaginarySign()
    {
        if (!hasImaginaryPart)
        {
            hasImaginaryPart = true;
        }

        editingImaginary = true;
        imaginaryText = ToggleComponentSign(imaginaryText, imaginaryEditor, allowEmpty: true);
        SyncValue();
        return value;
    }

    public string ToggleRealSign()
    {
        realText = ToggleComponentSign(realText, realEditor, allowEmpty: false);
        SyncValue();
        return value;
    }

    private void ApplyToActiveComponent(Func<PNumberEditor, string> action)
    {
        if (editingImaginary)
        {
            imaginaryText = ApplyToComponent(imaginaryEditor, imaginaryText, action);
        }
        else
        {
            realText = ApplyToComponent(realEditor, realText, action);
        }

        SyncValue();
    }

    private static string ApplyToComponent(PNumberEditor editor, string componentText, Func<PNumberEditor, string> action)
    {
        editor.SetValue(componentText.Length == 0 ? "0" : componentText);
        return action(editor);
    }

    private static string ToggleComponentSign(string componentText, PNumberEditor editor, bool allowEmpty)
    {
        editor.SetValue(componentText.Length == 0 ? "0" : componentText);
        var toggled = editor.ToggleSign();
        if (allowEmpty && componentText.Length == 0 && toggled == "0")
        {
            return string.Empty;
        }

        return toggled;
    }

    private void SyncValue()
    {
        value = hasImaginaryPart
            ? $"{realText};{imaginaryText}"
            : realText;
    }

    private void TrySeedEditorsFromText(string source)
    {
        try
        {
            if (source.Contains(';', StringComparison.Ordinal))
            {
                var parts = source.Split(';');
                if (parts.Length == 2)
                {
                    realText = parts[0];
                    imaginaryText = parts[1];
                    hasImaginaryPart = true;
                    editingImaginary = imaginaryText.Length > 0;
                    return;
                }
            }

            if (source.Contains("i*", StringComparison.Ordinal))
            {
                var complex = new TComp(source, NumberBase, 10);
                realText = complex.Re.ToString();
                imaginaryText = complex.Im.ToString();
                hasImaginaryPart = true;
                editingImaginary = false;
                return;
            }

            realText = source;
            imaginaryText = string.Empty;
            hasImaginaryPart = false;
            editingImaginary = false;
        }
        catch (Exception) when (source.Length > 0)
        {
            // Result strings or incomplete input should still remain displayable even if
            // they are not meant to be edited further in the current state.
            realText = source;
            imaginaryText = string.Empty;
            hasImaginaryPart = false;
            editingImaginary = false;
        }
    }

    private static string BackspaceComponent(string componentText, bool allowEmpty)
    {
        if (componentText.Length <= 1)
        {
            return allowEmpty ? string.Empty : "0";
        }

        var shortened = componentText[..^1];
        if (shortened == "-")
        {
            return allowEmpty ? string.Empty : "0";
        }

        return shortened;
    }

    private static bool IsZeroComponent(string componentText)
    {
        return componentText is "" or "-" or "0" or "-0" or "0." or "-0.";
    }
}
