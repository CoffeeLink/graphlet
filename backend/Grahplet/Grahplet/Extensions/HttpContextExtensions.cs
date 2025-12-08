namespace Grahplet.Extensions;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue("UserId", out var userId) && userId is Guid guidUserId)
        {
            return guidUserId;
        }
        return null;
    }

    public static Guid GetRequiredUserId(this HttpContext context)
    {
        var userId = context.GetUserId();
        if (!userId.HasValue)
        {
            throw new InvalidOperationException("UserId not found in context");
        }
        return userId.Value;
    }
}

