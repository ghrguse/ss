//------------------------------------------------------------------------------
// 模块中心处理器
// piaochen
// 2014.12.16
//------------------------------------------------------------------------------
using System;

public interface IModuleCenter
{
	/// <summary>
	/// Setup the specified model, view, ctrl, data and server.
	/// </summary>
	/// <param name="model">Model.</param>
	/// <param name="data">Data.</param>
	/// <param name="server">Server.</param>
	void Setup(BaseModelFactory model,BaseDataFactory data,BaseServerFactory server);
}