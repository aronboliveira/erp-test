namespace Acme.Admin.Api.Validation;

public static class Paging
{
    public const int DefaultPage = 0;
    public const int DefaultSize = 20;
    public const int MaxSize = 100;

    public static (int Page, int Size) Normalize(int page, int size)
    {
        if (page < 0)
        {
            throw new ArgumentException("page must be >= 0");
        }

        if (size < 1 || size > MaxSize)
        {
            throw new ArgumentException($"size must be between 1 and {MaxSize}");
        }

        return (page, size);
    }
}
