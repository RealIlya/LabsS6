namespace ConverterLab1;

public class Control_
{
    private const int pin = 10;
    private const int pout = 16;

    public enum State
    {
        Редактирование,
        Преобразовано
    }

    public History his = new History();
    public Editor ed = new Editor();

    public State St { get; set; }
    public int Pin { get; set; }
    public int Pout { get; set; }

    public Control_()
    {
        St = State.Редактирование;
        Pin = pin;
        Pout = pout;
    }

    public string DoCmnd(int j)
    {
        if (j == 19)
        {
            var input = ed.Number;
            var result = RecalculateWithoutHistory();
            his.ДобавитьЗапись(Pin, Pout, input, result);
            return result;
        }

        St = State.Редактирование;
        return ed.DoEdit(j);
    }

    public string RecalculateWithoutHistory()
    {
        var decimalValue = Conver_P_10.dval(ed.Number, Pin);
        var result = Conver_10_P.Do(decimalValue, Pout, acc());
        St = State.Преобразовано;
        return result;
    }

    private int acc()
    {
        var value = (int)Math.Round(ed.Acc() * Math.Log(Pin) / Math.Log(Pout) + 0.5);
        return Math.Max(value, 1);
    }
}
