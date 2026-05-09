#nullable enable
using DeepForestLabs.Data;

namespace DeepForestLabs.Factories
{
	public class ContainerBuilderFactory : ValidatedData
	{
		public virtual IContainerBuilder AddToBuilder(IContainerBuilder builder)
		{
			return builder;
		}
	}
	
	public abstract class ContainerBuilderFactory<TArg> : ContainerBuilderFactory
	{
		public abstract IContainerBuilder AddToBuilder(IContainerBuilder builder, TArg arg);
	}
	
	public abstract class ContainerBuilderFactory<TArg1, TArg2> : ContainerBuilderFactory
	{
		public abstract IContainerBuilder AddToBuilder(IContainerBuilder builder, TArg1 arg1, TArg2 arg2);
	}
}
#nullable disable
