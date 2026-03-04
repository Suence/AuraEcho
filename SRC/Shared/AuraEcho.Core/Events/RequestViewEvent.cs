using Prism.Events;

namespace AuraEcho.Core.Events;

/// <summary>
/// 视图导航请求事件
/// </summary>
public class RequestViewEvent : PubSubEvent<string>
{
}
