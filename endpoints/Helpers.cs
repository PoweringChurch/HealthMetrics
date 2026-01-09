namespace HealthMetrics.Endpoints;
public static class Helpers
{
    public static void MapParameters<TSource, TDest>(TSource source, TDest destination)
    {
        var sourceProps = typeof(TSource).GetProperties();
        var destProps = typeof(TDest).GetProperties().ToDictionary(p => p.Name);

        foreach (var sourceProp in sourceProps)
        {
            var value = sourceProp.GetValue(source);
            if (value is not null && destProps.TryGetValue(sourceProp.Name, out var destProp))
                destProp.SetValue(destination, value);
        }
    }
}