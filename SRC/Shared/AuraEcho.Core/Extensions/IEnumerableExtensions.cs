using System.Collections.ObjectModel;

namespace AuraEcho.Core.Extensions;

/// <summary>
/// <see cref="IEnumerable{T}"/> 扩展方法
/// </summary>
public static class IEnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> @this)
        => new(@this);

    /// <summary>
    /// 对每个元素指定特定的动作
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="this">列表</param>
    /// <param name="action">动作</param>
    public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
    {
        foreach (var item in @this)
            action(item);
    }

    /// <summary>
    /// 对每个元素指定特定的动作
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="this">列表</param>
    /// <param name="action">动作</param>
    public static void ForEach<T>(this IEnumerable<T> @this, Action<T, int> action)
    {
        int index = 0;
        foreach (var item in @this)
        {
            action(item, index);
            index++;
        }
    }
}
