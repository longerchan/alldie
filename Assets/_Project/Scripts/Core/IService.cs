namespace FrostShelter.Core
{
    /// <summary>
    /// 标记接口，所有通过 ServiceLocator 注册的服务必须实现此接口
    /// </summary>
    public interface IService
    {
        void Initialize();
        void Shutdown();
    }
}
