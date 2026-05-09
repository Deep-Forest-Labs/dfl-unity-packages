#nullable enable
using System;

namespace DeepForestLabs.States.Error.Controllers
{
	public interface IErrorStateController : IRunnableWith<Exception>
	{
	}
}
#nullable disable