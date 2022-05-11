using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drone.Models;

public sealed class ObjectResult : Result
{
    public object Result { get; }

    protected internal override IList<ResultProperty> ResultProperties =>
        new List<ResultProperty>
        {
            new ResultProperty
            {
                Name = Result.GetType().Name,
                Value = Result
            }
        };

    public ObjectResult(object result)
    {
        Result = result;
    }
}

public class ResultList<T> : IList<T> where T : Result
{
    private List<T> Results { get; } = new List<T>();

    public int Count => Results.Count;
    public bool IsReadOnly => ((IList<T>)Results).IsReadOnly;

    public override string ToString()
    {
        if (Results.Count <= 0) return "";
        
        var labels = new StringBuilder();
        var underlines = new StringBuilder();
        var rows = new List<StringBuilder>();
            
        for (var i = 0; i < Results.Count; i++)
        {
            rows.Add(new StringBuilder());
        }

        for (var i = 0; i < Results[0].ResultProperties.Count; i++)
        {
            labels.Append(Results[0].ResultProperties[i].Name);
            underlines.Append(new string('-', Results[0].ResultProperties[i].Name.Length));
            
            var maxPropLen = 0;
            
            for (var j = 0; j < rows.Count; j++)
            {
                var property = Results[j].ResultProperties[i];
                var valueString = property.Value.ToString();
                rows[j].Append(valueString);
                if (maxPropLen < valueString.Length)
                {
                    maxPropLen = valueString.Length;
                }
            }

            if (i == Results[0].ResultProperties.Count - 1) continue;
            {
                labels.Append(new string(' ',
                    Math.Max(2, maxPropLen + 2 - Results[0].ResultProperties[i].Name.Length)));
                
                underlines.Append(new string(' ',
                    Math.Max(2, maxPropLen + 2 - Results[0].ResultProperties[i].Name.Length)));
                
                for (var j = 0; j < rows.Count; j++)
                {
                    var property = Results[j].ResultProperties[i];
                    var valueString = property.Value.ToString();
                    
                    rows[j].Append(new string(' ',
                        Math.Max(Results[0].ResultProperties[i].Name.Length - valueString.Length + 2,
                            maxPropLen - valueString.Length + 2)));
                }
            }
        }

        labels.AppendLine();
        labels.Append(underlines);
        
        foreach (var row in rows)
        {
            labels.AppendLine();
            labels.Append(row);
        }

        return labels.ToString();
    }

    public T this[int index]
    {
        get => Results[index];
        set => Results[index] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Results.Cast<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Results.Cast<T>().GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return Results.IndexOf(item);
    }

    public void Add(T t)
    {
        Results.Add(t);
    }

    public void AddRange(IEnumerable<T> range)
    {
        Results.AddRange(range);
    }

    public void Insert(int index, T item)
    {
        Results.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        Results.RemoveAt(index);
    }

    public void Clear()
    {
        Results.Clear();
    }

    public bool Contains(T item)
    {
        return Results.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Results.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return Results.Remove(item);
    }
}

public abstract class Result
{
    protected internal abstract IList<ResultProperty> ResultProperties { get; }
}

public class ResultProperty
{
    public string Name { get; set; }
    public object Value { get; set; }
}