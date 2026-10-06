/// index = 0 is Clear / 1 is Bounce / 2 is Swipe
public readonly struct StageStars
{
    public const int StarCount = 3;

    public readonly bool Clear;
    public readonly bool Bounce;
    public readonly bool Swipe;

    public StageStars(bool clear, bool bounce, bool swipe)
    {
        Clear = clear;
        Bounce = bounce;
        Swipe = swipe;
    }

    public static StageStars None => new StageStars(false, false, false);

    public int Count => (Clear ? 1 : 0) + (Bounce ? 1 : 0) + (Swipe ? 1 : 0);

    public bool Has(int index)
    {
        return index switch
        {
            0 => Clear,
            1 => Bounce,
            2 => Swipe,
            _ => false
        };
    }
}

public readonly struct StarResult
{
    public readonly StageStars Before;
    public readonly StageStars After;

    public StarResult(StageStars before, StageStars after)
    {
        Before = before;
        After = after;
    }

    public bool IsNew(int index) => After.Has(index) && !Before.Has(index);
}
